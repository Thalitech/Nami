using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Nami.Attributes;
using Nami.Database;
using Nami.Extensions;
using Nami.Modules.Administration.Extensions;
using Nami.Modules.Streaming.Services;
using Nami.Services;

namespace Nami.Modules.Streaming
{
    [Group("stream"), Module(ModuleType.Administration), NotBlocked]
    [Aliases("str")]
    [RequireGuild]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public sealed class StreamingModule : NamiServiceModule<StreamingConfigService>
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
            var cfg = await BotConfigService.LoadConfigAsync();
            var db = await DbContextBuilder.LoadConfigAsync(cfg);
            var dbctx = db.CreateContext();

            var da = dbctx.Streams;


            /*
            var file = Path.Combine(Directory.CreateDirectory("Resources/").FullName, $"MembersSRJ.json");
            var data = new Dictionary<ulong, int>();
            if (File.Exists(file))
                data = JsonConvert.DeserializeObject<Dictionary<ulong, int>>(File.ReadAllText(file));
            if (data.Count <= 0 || data[member.Id] < 3) {
                if (data.ContainsKey(member.Id)) data[member.Id]++;
                else data.Add(member.Id, 1);
                await ctx.RespondAsync($"An SRJ has beend added to **{member.Username}** they have **{data[member.Id]}** SRJ's");
                File.WriteAllText(file, JsonConvert.SerializeObject(data, Formatting.Indented));

                if (data[member.Id] >= 3) {
                    await ctx.RespondAsync($"The member {member.Username}'s rank has pass the threshold but thre hasn't been a role for use to add.");
                    // Add role to user and keep track just incase we want to remove a time.
                    await member.GrantRoleAsync(ctx.Guild.Roles.ToList().Find(x => x.Value.Name == "Thicc Players").Value,
                        $"The member {member.Username}'s rank has pass the threshold but thre hasn't been a role for use to add.");
                }
                return;
            }
            */
        }

        [Command("-")]
        [Aliases("remove", "subtract")]
        [Description("desc-streamer-guest-remove")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task RemoveCounter(CommandContext ctx,
            [Description("desc-member")] DiscordMember member)
        {
            var cfg = await BotConfigService.LoadConfigAsync();
            var db = await DbContextBuilder.LoadConfigAsync(cfg);
            var dbctx = db.CreateContext();
            var role = cfg.CurrentConfiguration.StreamerGuessRole;
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
