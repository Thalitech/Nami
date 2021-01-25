using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Nami.Database.Models;
using Nami.Modules.Search.Services;
using Nami.Services.Common;

namespace Nami.Services.Extensions
{
    public static class PeriodicTasksServiceExtensions
    {
        public static async Task<bool> SendFeedUpdateAsync(NamiBot shard, RssSubscription sub, SyndicationItem latest)
        {
            DiscordChannel? chn;
            try {
                chn = await shard.Client.GetShard(sub.GuildId).GetChannelAsync(sub.ChannelId);
            } catch (NotFoundException) {
                return false;
            }

            if (chn is null)
                return false;

            var emb = new LocalizedEmbedBuilder(shard.Services.GetRequiredService<LocalizationService>(), sub.GuildId);
            emb.WithTitle(latest.Title.Text);
            emb.WithUrl(sub.Feed.LastPostUrl);
            emb.WithColor(DiscordColor.Gold);
            emb.WithLocalizedTimestamp(latest.LastUpdatedTime > latest.PublishDate ? latest.LastUpdatedTime : latest.PublishDate);

            if (latest.Content is TextSyndicationContent content) {
                string? imageUrl = RedditService.GetImageUrl(content);
                if (imageUrl is { })
                    emb.WithImageUrl(imageUrl);
            }

            if (!string.IsNullOrWhiteSpace(sub.Name))
                emb.AddLocalizedTitleField("str-from", sub.Name);
            emb.AddLocalizedTitleField("str-content", sub.Feed.LastPostUrl);

            await chn.SendMessageAsync(embed: emb.Build());
            return true;
        }
    }
}
