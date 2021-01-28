using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Nami.Common;
using Nami.EventListeners.Attributes;
using Nami.EventListeners.Common;
using Nami.Extensions;
using Nami.Modules.Administration.Services;
using Nami.Modules.Owner.Services;
using Nami.Services;

namespace Nami.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.ClientErrored)]
        public static Task ClientErrorEventHandlerAsync(NamiBot _, ClientErrorEventArgs e)
        {
            Exception ex = e.Exception;
            while (ex is AggregateException)
                ex = ex.InnerException ?? ex;

            Log.Error(ex, "Client errored: {EventName}", e.EventName);
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.GuildAvailable)]
        public static Task GuildAvailableEventHandlerAsync(NamiBot bot, GuildCreateEventArgs e)
        {
            LogExt.Information(bot.GetId(e.Guild.Id), "Available: {AvailableGuild}", e.Guild);
            GuildConfigService gcs = bot.Services.GetRequiredService<GuildConfigService>();
            return gcs.IsGuildRegistered(e.Guild.Id) ? Task.CompletedTask : gcs.RegisterGuildAsync(e.Guild.Id);
        }

        [AsyncEventListener(DiscordEventType.GuildUnavailable)]
        public static Task GuildUnvailableEventHandlerAsync(NamiBot bot, GuildDeleteEventArgs e)
        {
            LogExt.Warning(bot.GetId(e.Guild.Id), "Unvailable: {UnvailableGuild}", e.Guild);
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.GuildDownloadCompleted)]
        public static Task GuildDownloadCompletedEventHandlerAsync(NamiBot bot, GuildDownloadCompletedEventArgs e)
        {
            Log.Information("All guilds are now downloaded ({Count} total)", e.Guilds.Count);
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.GuildCreated)]
        public static async Task GuildCreateEventHandlerAsync(NamiBot bot, GuildCreateEventArgs e)
        {
            LogExt.Information(bot.GetId(e.Guild.Id), "Joined {NewGuild}", e.Guild);

            if (bot.Services.GetRequiredService<BlockingService>().IsGuildBlocked(e.Guild.Id)) {
                LogExt.Information(bot.GetId(e.Guild.Id), "{Guild} is blocked. Leaving...", e.Guild);
                await e.Guild.LeaveAsync();
                return;
            }

            IReadOnlyCollection<DiscordMember> members = await e.Guild.GetAllMembersAsync();
            int botCount = members.Where(m => m.IsBot).Count();
            if (botCount > 25 || (members.Count - botCount < 0)) {
                LogExt.Information(bot.GetId(e.Guild.Id), "{Guild} is most likely a bot farm. Leaving and blocking...", e.Guild);
                await e.Guild.LeaveAsync();
                await bot.Services.GetRequiredService<BlockingService>().BlockGuildAsync(e.Guild.Id, "Bot farm");
                return;
            }

            await bot.Services.GetRequiredService<GuildConfigService>().RegisterGuildAsync(e.Guild.Id);

            DiscordChannel defChannel = e.Guild.GetDefaultChannel();
            if (!defChannel.PermissionsFor(e.Guild.CurrentMember).HasPermission(Permissions.SendMessages))
                return;

            string prefix = bot.Services.GetRequiredService<BotConfigService>().CurrentConfiguration.Prefix;
            string owners = bot.Client.CurrentApplication.Owners.Select(o => o.ToDiscriminatorString()).Humanize(", ");
            await defChannel.EmbedAsync(
                $"{Formatter.Bold("Thank you for adding me!")}\n\n" +
                $"{Emojis.SmallBlueDiamond} The default prefix for commands is {Formatter.Bold(prefix)}, but it can be changed " +
                $"via {Formatter.Bold("prefix")} command.\n" +
                $"{Emojis.SmallBlueDiamond} I advise you to run the configuration wizard for this guild in order to quickly configure " +
                $"functions like logging, notifications etc. The wizard can be invoked using {Formatter.Bold("config setup")} command.\n" +
                $"{Emojis.SmallBlueDiamond} You can use the {Formatter.Bold("help")} command as a guide, though it is recommended to " +
                $"read the documentation @ https://github.com/thalitech/nami \n" +
                $"{Emojis.SmallBlueDiamond} If you have any questions or problems, feel free to use the {Formatter.Bold("report")} " +
                $"command in order to send a message to the bot owners ({owners}). Alternatively, you can create an issue on " +
                $"GitHub or join **Thalitech's Resort** Discord server for quick support (https://discord.gg/C77BjRvRJP)."
                , Emojis.Wave
            );
        }

        [AsyncEventListener(DiscordEventType.SocketOpened)]
        public static Task SocketOpenedEventHandlerAsync(NamiBot bot, SocketEventArgs _)
        {
            Log.Debug("Socket opened");
            bot.Services.GetRequiredService<BotActivityService>().UptimeInformation.SocketStartTime = DateTimeOffset.Now;
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.SocketClosed)]
        public static Task SocketClosedEventHandlerAsync(NamiBot bot, SocketCloseEventArgs e)
        {
            Log.Debug("Socket closed with code {Code}: {Message}", e.CloseCode, e.CloseMessage);
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.SocketErrored)]
        public static Task SocketErroredEventHandlerAsync(NamiBot shard, SocketErrorEventArgs e)
        {
            Log.Debug(e.Exception, "Socket errored");
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.UnknownEvent)]
        public static Task UnknownEventHandlerAsync(NamiBot shard, UnknownEventArgs e)
        {
            Log.Error("Unknown event ({UnknownEvent}) occured: {@UnknownEventJson}", e.EventName, e.Json);
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.UserUpdated)]
        public static Task UserUpdatedEventHandlerAsync(NamiBot bot, UserUpdateEventArgs e)
        {
            Log.Information("Bot updated");
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.UserSettingsUpdated)]
        public static Task UserSettingsUpdatedEventHandlerAsync(NamiBot bot, UserSettingsUpdateEventArgs e)
        {
            Log.Information("User settings updated");
            return Task.CompletedTask;
        }
    }
}
