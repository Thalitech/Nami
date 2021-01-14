using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nami
{
    public class CommandHub : BaseCommandModule
    {
        [Command("announce")]
        public async Task GreetCommand(CommandContext ctx, [RemainingText] string message)
        {
            var permissinGranted = await PermissionManager.CheckPermissions(ctx, PermissionManager.PermissionType.Admin);
            if (!permissinGranted) return;

            await Task.Run(async () =>
            {
                var embed = new DiscordEmbedBuilder();
                embed.Title = "Announcements";
                embed.Timestamp = new System.DateTimeOffset(DateTime.Now);
                embed.Description = $"@everyone {message}";
                embed.Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = $"Thank you for your time. {ctx.Guild.Name} Team",
                    IconUrl = ctx.Member.AvatarUrl,
                };
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                {
                    Url = ctx.Guild.IconUrl,
                };
                embed.Color = DiscordColor.Orange;

                var channel = ctx.Guild.Channels.ToList().Find(x => x.Value.Name.ToLower().Contains("announce")).Value ?? ctx.Guild.SystemChannel;
                await channel.SendMessageAsync(embed: embed.Build());
            }).ConfigureAwait(false);
            await ctx.Message.DeleteAsync();
            await ctx.RespondAsync("Your message has been sent to the server.");
            return;
        }




    }
}
