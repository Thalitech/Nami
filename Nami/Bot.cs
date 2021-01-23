using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Nami
{
    class Bot : IDisposable
    {
        public enum CommandCode { Running, stop, Reset, }
        public CommandCode commandCode;
        public async Task RunAsync()
        {
            if (commandCode == CommandCode.stop)
            {
                Console.ReadLine();
                return;
            }
        initialize:
            Console.Clear();
            if (await AssetDatabase.Get<bool>(ValueType.UseReddit))
                using (var reddit = new RedditManager()) { }
            // Configurations
            var discordConfig = new DiscordConfiguration()
            {
                Token = await AssetDatabase.Get<string>(ValueType.Token, true),
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Debug,
                LogTimestampFormat = "MMM dd yyyy - hh:mm:ss tt",
                Intents = DiscordIntents.DirectMessageReactions
                     | DiscordIntents.DirectMessages
                     | DiscordIntents.GuildBans
                     | DiscordIntents.GuildEmojis
                     | DiscordIntents.GuildInvites
                     | DiscordIntents.GuildMembers
                     | DiscordIntents.GuildMessages
                     | DiscordIntents.Guilds
                     | DiscordIntents.GuildVoiceStates
                     | DiscordIntents.GuildWebhooks
                     | DiscordIntents.GuildPresences,

            };
            var commandConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = await AssetDatabase.Get<string[]>(ValueType.Prefixes),
                DmHelp = true,
                EnableMentionPrefix = true,
            };
            var discord = new DiscordClient(discordConfig);
            discord.UseInteractivity(new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromMinutes(5)
            });

            var commands = discord.UseCommandsNext(commandConfig);
            discord.GuildAvailable += Events.OnGuildsAvailable;
            discord.GuildMemberAdded += Events.OnMemberAdded;
            discord.GuildMemberRemoved += Events.OnMemberRemoved;
            commands.RegisterCommands<AdminCommands>();
            commands.RegisterCommands<MusicCommands>();
            if (AssetDatabase.compiledAssembly != null)
                commands.RegisterCommands(AssetDatabase.compiledAssembly);

            await discord.ConnectAsync();
            if (await AssetDatabase.Get<bool>(ValueType.UseLavalink))
                await Dispatcher.ConnectLavalinkAsync(discord);
            commandCode = CommandCode.Running;
            while (commandCode != CommandCode.stop && commandCode != CommandCode.Reset) { };
            await discord.DisconnectAsync();
            if (commandCode == CommandCode.Reset) 
                goto initialize;

        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
