using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using GScraper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nami
{
    public class CommandHub : BaseCommandModule
    {
        [RequirePrefixes("?")]
        [Command("announce")]
        [Description("This command is use to send announcments to the current server.")]
        public async Task AnnounceAsync(CommandContext ctx, [RemainingText][Description("The message that will be sent.")] string message)
        {
            var permissinGranted = await PermissionManager.CheckPermissions(ctx, PermissionManager.PermissionType.Admin);
            if (!permissinGranted) return;

            await Task.Run(async () =>
            {
                var embed = new DiscordEmbedBuilder();
                embed.Title = "Announcements";
                embed.Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    IconUrl = ctx.Member.AvatarUrl,
                    Name = ctx.User.Username
                };
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
                foreach (var item in message.Split(' '))
                {
                    if (Uri.IsWellFormedUriString(item, UriKind.Absolute))
                    {
                        var gs = new GoogleScraper();
                        var image = await gs.GetImagesAsync(item, 1, false);
                        var field = embed.AddField("** **", "** **");
                        field.ImageUrl = image[0].ThumbnailLink;
                    }
                }
                var channel = ctx.Guild.Channels.ToList().Find(x => x.Value.Name.ToLower().Contains("announce")).Value ?? ctx.Guild.SystemChannel;
                await channel.SendMessageAsync(embed: embed.Build());
            }).ConfigureAwait(false);
            await ctx.Message.DeleteAsync();
            var msg = await ctx.RespondAsync("Your message has been sent to the server.");
            await Task.Delay(5000);
            await msg.DeleteAsync();
        }

        [RequirePrefixes("?")]
        [Command("announce-all")]
        [Description("This command is use to send announcments to all server that the bot is linked too")]
        public async Task AnnounceAllAsync(CommandContext ctx, [RemainingText][Description("The message that will be sent.")] string message)
        {
            var permissinGranted = await PermissionManager.CheckPermissions(ctx, PermissionManager.PermissionType.Admin);
            if (!permissinGranted) return;
            foreach (var server in ctx.Client.Guilds.ToList().Select(x => x.Value))
            {
                await Task.Run(async () =>
                {
                    var embed = new DiscordEmbedBuilder();
                    embed.Title = "Global Announcements";
                    embed.Author = new DiscordEmbedBuilder.EmbedAuthor()
                    {
                        IconUrl = ctx.Member.AvatarUrl,
                        Name = ctx.User.Username
                    };
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
                    embed.Color = DiscordColor.IndianRed;
                    foreach (var item in message.Split(' '))
                    {
                        if (Uri.IsWellFormedUriString(item, UriKind.Absolute))
                        {
                            var gs = new GoogleScraper();
                            var image = await gs.GetImagesAsync(item, 1, false);
                            var field = embed.AddField("** **", "** **");
                            field.ImageUrl = image[0].ThumbnailLink;
                        }
                    }
                    var channel = server.Channels.ToList().Find(x => x.Value.Name.ToLower().Contains("announce")).Value ?? server.SystemChannel;
                    await channel.SendMessageAsync(embed: embed.Build());
                }).ConfigureAwait(false);
            }
            await ctx.Message.DeleteAsync();
            var msg = await ctx.RespondAsync("Your message has been sent to the server.");
            await Task.Delay(5000);
            await msg.DeleteAsync();
        }


        [RequirePrefixes("?")]
        [Command("purge")]
        [Description("This command will remove all messages that are in the text channel that the command was ran in.")]
        public async Task Purge(CommandContext ctx)
        {
            var permissinGranted = await PermissionManager.CheckPermissions(ctx, PermissionManager.PermissionType.Admin);
            if (!permissinGranted) return;

            await Task.Run(async () =>
            {
                var messages = ctx.Channel.GetMessagesAsync(1000).Result.ToList();
                for(int i = 0; i < messages.Count; i++)
                {
                    await messages[i].DeleteAsync();
                }
            }).ConfigureAwait(false);
        }
    }
}
