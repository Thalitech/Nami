using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Logging;
using Nami.Database;
using Nami.EventListeners;
using Nami.Exceptions;
using Nami.Extensions;
using Nami.Modules.Administration.Services;
using Nami.Modules.Misc.Common;
using Nami.Services;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Microsoft.EntityFrameworkCore;

namespace Nami
{
    public sealed class NamiBot : BackgroundService
    {
        public static string ApplicationName { get; set; } = string.Empty;
        public static string ApplicationVersion { get; set; } = string.Empty;

        internal static NamiBot? Bot { get; set; }
        private static PeriodicTasksService? PeriodicService { get; set; }
        public static ILogger logger { get; private set; }

        public ServiceProvider Services => this.services ?? throw new BotUninitializedException();
        public BotConfigService Config => this.config ?? throw new BotUninitializedException();
        public DbContextBuilder Database => this.database ?? throw new BotUninitializedException();
        public DiscordShardedClient Client => this.client ?? throw new BotUninitializedException();
        public IReadOnlyDictionary<int, InteractivityExtension> Interactivity => this.interactivity ?? throw new BotUninitializedException();
        public IReadOnlyDictionary<int, CommandsNextExtension> CNext => this.cnext ?? throw new BotUninitializedException();
        public IReadOnlyDictionary<int, VoiceNextExtension> VNext => this.vnext ?? throw new BotUninitializedException();
        public IReadOnlyDictionary<int, LavalinkExtension> Lavalink => this.lavalink ?? throw new BotUninitializedException();
        public IReadOnlyDictionary<string, Command> Commands => this.commands ?? throw new BotUninitializedException();

        private readonly BotConfigService? config;
        private readonly DbContextBuilder? database;
        private DiscordShardedClient? client;
        private ServiceProvider? services;
        private IReadOnlyDictionary<int, InteractivityExtension>? interactivity;
        private IReadOnlyDictionary<int, CommandsNextExtension>? cnext;
        private IReadOnlyDictionary<int, VoiceNextExtension>? vnext;
        private IReadOnlyDictionary<int, LavalinkExtension>? lavalink;
        private IReadOnlyDictionary<string, Command>? commands;

        public NamiBot(ILogger<NamiBot> _logger)
        {
            AssemblyName info = Assembly.GetExecutingAssembly().GetName();
            ApplicationName = info.Name ?? "Nami";
            ApplicationVersion = $"v{info.Version?.ToString() ?? "<unknown>"}";

            logger = _logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            mPrintBuildInformation();

            try
            {
                BotConfigService cfg = await mLoadBotConfigAsync();
                Log.Logger = LogExt.CreateLogger(cfg.CurrentConfiguration);
                NamiBot.logger.LogInformation("Logger created.");

                DbContextBuilder dbb = await mInitializeDatabaseAsync(cfg);

                await mStartAsync(cfg, dbb);
                logger.LogInformation("Booting complete!");

                CancellationToken token = Bot?.Services.GetRequiredService<BotActivityService>().MainLoopCts.Token
                    ?? throw new InvalidOperationException("Bot not initialized");
                await Task.Delay(Timeout.Infinite, token);
            } 
            catch (TaskCanceledException ex) 
            {
                logger.LogInformation(ex, "Shutdown signal received!");
                Console.ReadLine();
            } 
            catch (Exception e) 
            {
                logger.LogCritical(e, "Critical exception occurred");
                Environment.ExitCode = 1;
            } finally 
            {
                await mDisposeAsync();
            }
        }


        #region Setup
        private static void mPrintBuildInformation()
        {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
#if (DEBUG)
            Console.Title = $"{Assembly.GetExecutingAssembly().GetName().Name} {fileVersionInfo.FileVersion} (DEBUG)";
#elif (RELEASE)
            Console.Title = $"{Assembly.GetExecutingAssembly().GetName().Name} {fileVersionInfo.FileVersion} (RELEASE)";
#endif
            logger.LogInformation($"{ApplicationName} {ApplicationVersion} ({fileVersionInfo.FileVersion})");
        }
        private static async Task<BotConfigService> mLoadBotConfigAsync()
        {
            logger.LogInformation("Loading configuration... ");

            var cfg = new BotConfigService();
            await cfg.LoadConfigAsync();
            return cfg;
        }
        private static async Task<DbContextBuilder> mInitializeDatabaseAsync(BotConfigService cfg)
        {
            logger.LogInformation("Establishing database connection");
            var dbb = new DbContextBuilder(cfg.CurrentConfiguration.DatabaseConfig);

            logger.LogInformation("Testing database context creation");
            using (NamiDbContext db = dbb.CreateContext()) {
                IEnumerable<string> pendingMigrations = await db.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any()) {
                    logger.LogInformation("Applying pending database migrations: {PendingDbMigrations}", pendingMigrations);
                    await db.Database.MigrateAsync();
                }
            }

            return dbb;
        }
        private static Task mStartAsync(BotConfigService cfg, DbContextBuilder dbb)
        {
            Bot = new NamiBot(cfg, dbb);
            PeriodicService = new PeriodicTasksService(Bot, cfg.CurrentConfiguration);
            return Bot.StartAsync();
        }
        private static async Task mDisposeAsync()
        {
            logger.LogInformation("Cleaning up ...");

            PeriodicService?.Dispose();
            if (Bot is { })
                await Bot.DisposeAsync();

            logger.LogInformation("Cleanup complete! Powering off");
        }
        #endregion


        public NamiBot(BotConfigService cfg, DbContextBuilder dbb)
        {
            this.config = cfg;
            this.database = dbb;
        }

        public async Task DisposeAsync()
        {
            if (this.Client is { })
                await this.Client.StopAsync();
            await this.Services.DisposeAsync();
        }


        public int GetId(ulong? gid)
            => gid is null ? 0 : this.Client.GetShard(gid.Value).ShardId;

        public async Task StartAsync()
        {
            logger.LogInformation("Initializing the bot...");

            this.client = this.SetupClient();

            this.services = this.SetupServices();
            this.cnext = await this.SetupCommandsAsync();
            this.UpdateCommandList();

            this.interactivity = await this.SetupInteractivityAsync();
            this.lavalink = await this.client.UseLavalinkAsync();
            this.vnext = await this.client.UseVoiceNextAsync(new VoiceNextConfiguration());

            Listeners.FindAndRegister(this);

            await this.Client.StartAsync();
        }

        public void UpdateCommandList()
        {
            this.commands = this.CNext.First().Value.GetRegisteredCommands()
                .Where(cmd => cmd.Parent is null)
                .SelectMany(cmd => cmd.Aliases.Select(alias => (alias, cmd)).Concat(new[] { (cmd.Name, cmd) }))
                .ToDictionary(tup => tup.Item1, tup => tup.cmd);
        }


        private DiscordShardedClient SetupClient()
        {
            var cfg = new DiscordConfiguration {
                Token = this.Config.CurrentConfiguration.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LargeThreshold = 500,
                ShardCount = this.Config.CurrentConfiguration.ShardCount,
                LoggerFactory = new SerilogLoggerFactory(dispose: true),
                Intents = DiscordIntents.All
                    .RemoveIntent(DiscordIntents.GuildMessageTyping)
                    .RemoveIntent(DiscordIntents.DirectMessageTyping)
            };

            var client = new DiscordShardedClient(cfg);
            client.Ready += (s, e) => {
                LogExt.Information(s.ShardId, "Client ready!");
                return Task.CompletedTask;
            };

            return client;
        }

        private ServiceProvider SetupServices()
        {
            logger.LogInformation("Initializing services...");
            return new ServiceCollection()
                .AddSingleton(this.Config)
                .AddSingleton(this.Database)
                .AddSingleton(this.Client)
                .AddSharedServices()
                .BuildServiceProvider()
                .Initialize();
        }

        private async Task<IReadOnlyDictionary<int, CommandsNextExtension>> SetupCommandsAsync()
        {
            var cfg = new CommandsNextConfiguration {
                CaseSensitive = false,
                EnableDefaultHelp = false,
                EnableDms = true,
                EnableMentionPrefix = true,
                IgnoreExtraArguments = false,
                PrefixResolver = m => {
                    string p = m.Channel.Guild is null
                        ? this.Config.CurrentConfiguration.Prefix
                        : this.Services.GetRequiredService<GuildConfigService>().GetGuildPrefix(m.Channel.Guild.Id) ?? this.Config.CurrentConfiguration.Prefix;
                    return Task.FromResult(m.GetStringPrefixLength(p));
                },
                Services = this.Services
            };
            IReadOnlyDictionary<int, CommandsNextExtension> cnext = await this.Client.UseCommandsNextAsync(cfg);

            Log.Debug("Registering commands...");
            var assembly = Assembly.GetExecutingAssembly();
            foreach ((int shardId, CommandsNextExtension cne) in cnext) {
                cne.RegisterCommands(assembly);
                cne.RegisterConverters(assembly);
                cne.SetHelpFormatter<LocalizedHelpFormatter>();
            }

            Log.Debug("Checking command translations...");
            LocalizationService lcs = this.Services.GetRequiredService<LocalizationService>();
            CommandService cs = this.Services.GetRequiredService<CommandService>();
            foreach (Command cmd in cnext.Values.First().GetRegisteredCommands()) {
                try {
                    _ = lcs.GetCommandDescription(0, cmd.QualifiedName);
                    if (cmd is not CommandGroup group || group.IsExecutableWithoutSubcommands) {
                        _ = cs.GetCommandDescription(0, cmd.QualifiedName);
                        _ = cs.GetCommandUsageExamples(0, cmd.QualifiedName);
                        IEnumerable<CommandArgument> args = cmd.Overloads.SelectMany(o => o.Arguments).Distinct();
                        foreach (CommandArgument arg in args)
                            _ = lcs.GetString(null, arg.Description);
                    }
                } catch (LocalizationException e) {
                    Log.Warning(e, "Translation not found");
                }
            }
            Log.Debug("Found translations for all commands and arguments");

            return cnext;
        }

        private Task<IReadOnlyDictionary<int, InteractivityExtension>> SetupInteractivityAsync()
        {
            return this.Client.UseInteractivityAsync(new InteractivityConfiguration {
                PaginationBehaviour = PaginationBehaviour.WrapAround,
                PaginationDeletion = PaginationDeletion.DeleteEmojis,
                PaginationEmojis = new PaginationEmojis(),
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromMinutes(1)
            });
        }
    }
}