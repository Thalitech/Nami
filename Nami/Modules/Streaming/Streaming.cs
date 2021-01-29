using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Nami.Attributes;
using Nami.Common;
using Nami.Database;
using Nami.Database.Models;
using Nami.Extensions;
using Nami.Modules.Administration.Extensions;
using Nami.Modules.Administration.Services;
using Nami.Modules.Streaming.Services;
using Nami.Services;

namespace Nami.Modules.Streaming
{
    [Group("stream"), Module(ModuleType.Administration), NotBlocked]
    [Aliases("str")]
    [RequireGuild]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public sealed class StreamingModule : NamiServiceModule<StreamingService>
    {
        #region stream
        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.StreamingInfoAsync(ctx);
        #endregion


        [Command("setstreamplayerrole")]
        [Description("desc-streamer-badge")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task SetStreamerBadge(CommandContext ctx,
            [Description("desc-streamer-badge-role")] DiscordRole role)
        {
            var gcfg = await this.Service.GetConfigAsync(ctx.Guild.Id);
            await this.Service.ModifyConfigAsync(ctx.Guild.Id, cfg =>
            {
                cfg.StreamerBadge = role.Id;
            });

            await ctx.GuildLogAsync(emb => 
            {
                emb.WithLocalizedTitle("evt-cfg-upd");
                emb.WithColor(this.ModuleColor);
                emb.AddField($"Stream Player Badge", $"{role.Name}", inline: true);
            });

            await ctx.InfoAsync(this.ModuleColor, "desc-role", role.Name);
        }


        [Command("+")]
        [Aliases("add", "plus")]
        [Description("desc-streamer-guest-add")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task AddCounter(CommandContext ctx, 
            [Description("desc-member")]DiscordMember member)
        {
            
            await this.Service.IncreaseStreamAsync(ctx.Guild.Id, member.Id, 1);
            string joined = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.MoneyBag, "fmt-stream-joined", member.Mention, "1+", joined);
            var cur = await this.Service.GetAsync(ctx.Guild.Id, ctx.Member.Id);
        }

        [Command("-")]
        [Aliases("remove", "subtract")]
        [Description("desc-streamer-guest-remove")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task RemoveCounter(CommandContext ctx,
            [Description("desc-member")] DiscordMember member)
        {
            await this.Service.TryDecreaseStreamAsync(ctx.Guild.Id, member.Id, 1);
            string joined = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.MoneyBag, "fmt-stream-leave", member.Mention, "1-", joined);
            var cur = await this.Service.GetAsync(ctx.Guild.Id, ctx.Member.Id);
        }

        [Command("info")]
        [Aliases("i", "information")]
        public Task StreamingInfoAsync(CommandContext ctx)
        {
            return ctx.RespondWithLocalizedEmbedAsync(emb => 
            {
                emb.WithLocalizedTitle("str-stream-info-title");
                emb.WithColor(this.ModuleColor);
                emb.WithThumbnail(ctx.Guild.IconUrl);
                emb.WithLocalizedDescription("str-stream");
                
                var cfg = BotConfigService.LoadConfigAsync().Result;
                var role = cfg.CurrentConfiguration.StreamerGuessRole != null ? cfg.CurrentConfiguration.StreamerGuessRole.ToLower() : string.Empty; 

                emb.AddLocalizedTitleField("str-members", string.IsNullOrEmpty(role) ? "N/A" : ctx.Guild.Members.ToList().FindAll(x => x.Value.Roles.ToList().Find(x => x.Name.ToLower() == role) != null).Count, inline: true);
                
            });
        }
    }
}
