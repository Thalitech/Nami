﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Nami.Database;
using Nami.Extensions;
using Nami.Services;

namespace Nami
{
    internal static class Nami
    {
        public static string ApplicationName { get; }
        public static string ApplicationVersion { get; }

        internal static NamiBot? Bot { get; set; }
        private static PeriodicTasksService? PeriodicService { get; set; }


        static Nami()
        {
            AssemblyName info = Assembly.GetExecutingAssembly().GetName();
            ApplicationName = info.Name ?? "Nami";
            ApplicationVersion = $"v{info.Version?.ToString() ?? "<unknown>"}";
        }


        internal static async Task Main(string[] _)
        {
            PrintBuildInformation();

            try {
                BotConfigService cfg = await LoadBotConfigAsync();
                Log.Logger = LogExt.CreateLogger(cfg.CurrentConfiguration);
                Log.Information("Logger created.");

                DbContextBuilder dbb = await InitializeDatabaseAsync(cfg);
                
                await StartAsync(cfg, dbb);
                Log.Information("Booting complete!");

                CancellationToken token = Bot?.Services.GetRequiredService<BotActivityService>().MainLoopCts.Token 
                    ?? throw new InvalidOperationException("Bot not initialized");
                await Task.Delay(Timeout.Infinite, token);
            } catch (TaskCanceledException ex) {
                Log.Information(ex, "Shutdown signal received!");
                Console.ReadLine();
            } catch (Exception e) {
                Log.Fatal(e, "Critical exception occurred");
                Console.ReadLine();
                Environment.ExitCode = 1;
            } finally {
                await DisposeAsync();
            }

            Log.Information("Powering off");
            Log.CloseAndFlush();
            Environment.Exit(Environment.ExitCode);
        }

        public static Task Stop(int exitCode = 0, TimeSpan? after = null)
        {
            Environment.ExitCode = exitCode;
            Bot?.Services.GetRequiredService<BotActivityService>().MainLoopCts.CancelAfter(after ?? TimeSpan.Zero);
            return Task.CompletedTask;
        }


        #region Setup
        private static void PrintBuildInformation()
        {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
#if (DEBUG)
            Console.Title = $"{Assembly.GetExecutingAssembly().GetName().Name} {fileVersionInfo.FileVersion} (DEBUG)";
#elif (RELEASE)
            Console.Title = $"{Assembly.GetExecutingAssembly().GetName().Name} {fileVersionInfo.FileVersion} (RELEASE)";
#endif
            Console.WriteLine($"{ApplicationName} {ApplicationVersion} ({fileVersionInfo.FileVersion})");
            Console.WriteLine();
        }

        private static async Task<BotConfigService> LoadBotConfigAsync()
        {
            Console.Write("Loading configuration... ");

            var cfg = new BotConfigService();
            await cfg.LoadConfigAsync();

            Console.Write("\r");
            return cfg;
        }

        private static async Task<DbContextBuilder> InitializeDatabaseAsync(BotConfigService cfg)
        {
            Log.Information("Establishing database connection");
            var dbb = new DbContextBuilder(cfg.CurrentConfiguration.DatabaseConfig);

            Log.Information("Testing database context creation");
            using (NamiDbContext db = dbb.CreateContext()) 
            {
                IEnumerable<string> pendingMigrations = await db.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any()) 
                {
                    Log.Information("Applying pending database migrations: {PendingDbMigrations}", pendingMigrations);
                    await db.Database.MigrateAsync();
                }
            }

            return dbb;
        }

        private static Task StartAsync(BotConfigService cfg, DbContextBuilder dbb)
        {
            Bot = new NamiBot(cfg, dbb);
            PeriodicService = new PeriodicTasksService(Bot, cfg.CurrentConfiguration);
            return Bot.StartAsync();
        }

        private static async Task DisposeAsync()
        {
            Log.Information("Cleaning up ...");

            PeriodicService?.Dispose();
            if (Bot is { })
                await Bot.DisposeAsync();
    
            Log.Information("Cleanup complete! Powering off");
        }
#endregion
    }
}
