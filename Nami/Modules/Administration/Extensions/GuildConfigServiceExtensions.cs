using DSharpPlus.Entities;
using Nami.Modules.Administration.Services;
using Nami.Services.Common;

namespace Nami.Modules.Administration.Extensions
{
    public static class GuildConfigServiceExtensions
    {
        public static DiscordChannel? GetLogChannelForGuild(this GuildConfigService service, DiscordGuild? guild)
        {
            CachedGuildConfig? gcfg = service.GetCachedConfig(guild?.Id ?? 0) ?? new CachedGuildConfig();
            return guild is { } && gcfg.LoggingEnabled ? guild.GetChannel(gcfg.LogChannelId) : null;
        }
    }
}
