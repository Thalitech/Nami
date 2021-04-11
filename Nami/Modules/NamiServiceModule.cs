using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using Nami.Exceptions;
using Nami.Services;

namespace Nami.Modules
{
    public abstract class NamiServiceModule<TService> : NamiModule where TService : INamiService
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TService Service { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


        public override Task BeforeExecutionAsync(CommandContext ctx)
            => this.Service.IsDisabled ? throw new ServiceDisabledException(ctx) : base.BeforeExecutionAsync(ctx);

        #endregion


        #region internals
        private Task PrintImagesAsync(CommandContext ctx, IEnumerable<IGalleryItem>? res)
        {
            if (res is null || !res.Any()) 
                return ctx.FailAsync("cmd-err-res-none");

            return ctx.PaginateAsync(res.Take(20), (emb, r) => {
                emb.WithColor(this.ModuleColor);
                if (r is GalleryImage img) {
                    if ((img.Nsfw ?? false) && !ctx.Channel.IsNsfwOrNsfwName())
                        throw new CommandFailedException(ctx, "cmd-err-nsfw");
                    emb.WithTitle(img.Title);
                    emb.WithDescription(img.Description, unknown: false);
                    emb.WithImageUrl(img.Link);
                    emb.WithLocalizedTimestamp(img.DateTime);
                    emb.AddLocalizedTitleField("str-views", img.Views, inline: true);
                    emb.AddLocalizedTitleField("str-score", img.Score, inline: true, unknown: false);
                    emb.AddLocalizedTitleField("str-votes", $"{img.Points ?? 0} pts | {img.Ups ?? 0} ⬆️ {img.Downs ?? 0} ⬇️", inline: true);
                    emb.AddLocalizedTitleField("str-comments", img.CommentCount, inline: true, unknown: false);
                } else if (r is GalleryAlbum album) {
                    if ((album.Nsfw ?? false) & !ctx.Channel.IsNsfwOrNsfwName())
                        throw new CommandFailedException(ctx, "cmd-err-nsfw");
                    emb.WithTitle(album.Title);
                    emb.WithDescription(album.Description, unknown: false);
                    emb.WithImageUrl(album.Link);
                    emb.WithLocalizedTimestamp(album.DateTime);
                    emb.AddLocalizedTitleField("str-views", album.Views, inline: true);
                    emb.AddLocalizedTitleField("str-score", album.Score, inline: true, unknown: false);
                    emb.AddLocalizedTitleField("str-votes", $"{album.Points ?? 0} pts | {album.Ups ?? 0} ⬆️ {album.Downs ?? 0} ⬇️", inline: true);
                    emb.AddLocalizedTitleField("str-comments", album.CommentCount, inline: true, unknown: false);
                } else {
                    throw new CommandFailedException(ctx, "cmd-err-imgur");
                }
                return emb;
            });
        }
    }
}
