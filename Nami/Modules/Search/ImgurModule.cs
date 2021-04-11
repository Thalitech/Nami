using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Azure.Management.Monitor.Fluent.Models;
using Nami.Attributes;
using Nami.Exceptions;
using Nami.Extensions;
using Nami.Modules.Search.Services;

namespace Nami.Modules.Search
{
    [Group("imgur"), Module(ModuleType.Searches), NotBlocked]
    [Aliases("img", "im", "i")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class ImgurModule : NamiServiceModule<ImgurService>
    {
        #region imgur
        [GroupCommand, Priority(1)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("desc-res-num")] int amount,
                                           [RemainingText, Description("desc-sub")] string sub)
        {
            if (string.IsNullOrWhiteSpace(sub)) {
                await ctx.FailAsync("cmd-err-sub");
                return;
            }

            IEnumerable<IGalleryItem>? res = await this.Service.GetItemsFromSubAsync(
                sub,
                amount,
                SubredditGallerySortOrder.Top,
                TimeWindow.Day
            );

            await this.PrintImagesAsync(ctx, res);
        }

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-sub")] string sub,
                                     [Description("desc-res-num")] int n = 1)
            => this.ExecuteGroupAsync(ctx, n, sub);
        #endregion

        #region imgur latest
        [Command("latest"), Priority(1)]
        [Aliases("l", "new", "newest")]
        public async Task LatestAsync(CommandContext ctx,
                                     [Description("desc-res-num")] int amount,
                                     [RemainingText, Description("desc-sub")] string sub)
        {
            if (string.IsNullOrWhiteSpace(sub)) {
                await ctx.FailAsync("cmd-err-sub");
                return;
            }

            IEnumerable<IGalleryItem>? res = await this.Service.GetItemsFromSubAsync(sub, amount, SubredditGallerySortOrder.Time, TimeWindow.Day);
            await this.PrintImagesAsync(ctx, res);
        }

        [Command("latest"), Priority(0)]
        public Task LatestAsync(CommandContext ctx,
                               [Description("desc-sub")] string sub,
                               [Description("desc-res-num")] int n)
            => this.LatestAsync(ctx, n, sub);
        #endregion

        #region imgur top
        [Command("top"), Priority(3)]
        [Aliases("t")]
        public async Task TopAsync(CommandContext ctx,
                                  [Description("desc-timewindow")] TimeWindow timespan,
                                  [Description("desc-res-num")] int amount,
                                  [RemainingText, Description("desc-sub")] string sub)
        {
            if (string.IsNullOrWhiteSpace(sub)) {
                await ctx.FailAsync("cmd-err-sub");
                return;
            }

            IEnumerable<IGalleryItem>? res = await this.Service.GetItemsFromSubAsync(sub, amount, SubredditGallerySortOrder.Time, timespan);
            await this.PrintImagesAsync(ctx, res);
        }

        [Command("top"), Priority(2)]
        public Task TopAsync(CommandContext ctx,
                            [Description("desc-timewindow")] TimeWindow timespan,
                            [Description("desc-sub")] string sub,
                            [Description("desc-res-num")] int amount = 1)
            => this.TopAsync(ctx, timespan, amount, sub);

        [Command("top"), Priority(1)]
        public Task TopAsync(CommandContext ctx,
                            [Description("desc-res-num")] int amount,
                            [Description("desc-timewindow")] TimeWindow timespan,
                            [RemainingText, Description("desc-sub")] string sub)
            => this.TopAsync(ctx, timespan, amount, sub);

        [Command("top"), Priority(0)]
        public Task TopAsync(CommandContext ctx,
                            [Description("desc-res-num")] int amount,
                            [RemainingText, Description("desc-sub")] string sub)
            => this.TopAsync(ctx, TimeWindow.Day, amount, sub);

        #endregion
        #region internals
        #endregion
    }
}
