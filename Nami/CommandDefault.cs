using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nami
{
    public class CommandDefault : BaseCommandModule
    {
        [Command("announce")]
        public Task GreetCommand(CommandContext ctx, [RemainingText] string message)
        {
            Task.Run(async () =>
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
            });
            return Task.CompletedTask;
        }
    }
}
