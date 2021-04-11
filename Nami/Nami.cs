using System;
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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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


        internal static async Task Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();

            NamiBot.logger.LogInformation("Powering off");
            Log.CloseAndFlush();
            Environment.Exit(Environment.ExitCode);
        }

        public static Task Stop(int exitCode = 0, TimeSpan? after = null)
        {
            Environment.ExitCode = exitCode;
            Bot?.Services.GetRequiredService<BotActivityService>().MainLoopCts.CancelAfter(after ?? TimeSpan.Zero);
            return Task.CompletedTask;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) => {
                    services.AddLogging();
                    services.AddHostedService<NamiBot>();
                });
    }
}
