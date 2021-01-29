using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Nami.Attributes;
using Nami.Common;
using Nami.Extensions;
using Nami.Modules.Search.Services;

namespace Nami.Modules.Search
{
    [Group("joke"), Module(ModuleType.Searches), NotBlocked]
    [Aliases("jokes", "j")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class JokesModule : NamiModule
    {
        #region joke
        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            string? joke = await JokesService.GetRandomJokeAsync();
            if (joke is null)
                await ctx.FailAsync("cmd-err-res-none");
            else
                await ctx.Channel.EmbedAsync(joke, Emojis.Joy, this.ModuleColor);
        }
        #endregion

        #region joke search
        [Command("search")]
        [Aliases("s")]
        public async Task SearchAsync(CommandContext ctx,
                                     [RemainingText, Description("desc-query")] string query)
        {
            IReadOnlyList<string>? jokes = await JokesService.SearchForJokesAsync(query);
            if (jokes is null || !jokes.Any())
                await ctx.FailAsync("cmd-err-res-none");
            else
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Joy, "fmt-results", jokes.Take(5).JoinWith());
        }
        #endregion

        #region joke yourmom
        [Command("yourmom")]
        [Aliases("mama", "m", "yomomma", "yomom", "yomoma", "yomamma", "yomama")]
        public async Task YomamaAsync(CommandContext ctx)
        {
            string? joke = await JokesService.GetRandomYoMommaJokeAsync();
            if (joke is null)
                await ctx.FailAsync("cmd-err-res-none");
            else
                await ctx.Channel.EmbedAsync(joke, Emojis.Joy, this.ModuleColor);
        }
        #endregion
    }
}