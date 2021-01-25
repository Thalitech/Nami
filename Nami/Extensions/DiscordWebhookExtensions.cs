using DSharpPlus.Entities;

namespace Nami.Extensions
{
    internal static class DiscordWebhookExtensions
    {
        public static string BuildUrlString(this DiscordWebhook wh)
            => $"https://discordapp.com/api/webhooks/{ wh.ChannelId }/{ wh.Token }"; 
    }
}
