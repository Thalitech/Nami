using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using GScraper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nami
{
    public sealed class AdminCommands : BaseCommandModule
    {
        private DiscordEmoji[] _pollEmojiCache;

        [Command("restart")]
        [Description("This command will reset the discord bot")]
        public async Task Restart(CommandContext ctx)
        {
            var permissinGranted = await PermissionManager.CheckPermissions(ctx, PermissionManager.PermissionType.Admin);
            if (!permissinGranted) return;

            await Task.Run(async () =>
            {
                await ctx.RespondAsync("Nami is restarting, please wait a few seconds before trying to run any bot commands");
                Console.Clear();
                Program.bot.commandCode = Bot.CommandCode.Reset;
            }).ConfigureAwait(false);
        }

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
                        Url = server.IconUrl,
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

        [Command("purge")]
        [Description("This command will remove all messages that are in the text channel that the command was ran in.")]
        public async Task Purge(CommandContext ctx, int amount = 0)
        {
            var permissinGranted = await PermissionManager.CheckPermissions(ctx, PermissionManager.PermissionType.Admin);
            if (!permissinGranted) return;

            await Task.Run(async () =>
            {
                var messages = ctx.Channel.GetMessagesAsync(amount > 0 ? amount : 1000).Result.ToList();
                if (amount <= 100)
                {
                    await ctx.Channel.DeleteMessagesAsync(await ctx.Channel.GetMessagesAsync(amount), "The purge command on this channel has been called by a admin.");
                }
                else
                {
                    for (int i = 0; i < messages.Count; i++)
                    {
                        await messages[i].DeleteAsync("The purge command on this channel has been called by a admin.");
                    }
                }
            }).ConfigureAwait(false);
        }

        [Command("respite")]
        [Description("This command will server mute a play for 1m for the first attempt, 3m for the second attempt, 5 for the sixed attempt and add two everytime the command is used for that member.")]
        public async Task Respite(CommandContext ctx, DiscordMember member, [RemainingText] string reason = default)
        {
            var permissinGranted = await PermissionManager.CheckPermissions(ctx, PermissionManager.PermissionType.Admin);
            if (!permissinGranted) return;

            await Task.Run(async () =>
            {
                await RespiteEngine.Add(ctx.Guild, member, reason);
            }).ConfigureAwait(false);
        }

        [Command("rm-respite")]
        [Description("This command will remove server mute on a muted member.")]
        public async Task RemoveRespite(CommandContext ctx, DiscordMember member, [RemainingText] string reason = default)
        {
            var permissinGranted = await PermissionManager.CheckPermissions(ctx, PermissionManager.PermissionType.Admin);
            if (!permissinGranted) return;

            await Task.Run(async () =>
            {
                await RespiteEngine.Remove(ctx.Guild, member, reason);

            }).ConfigureAwait(false);
        }

        [Command("s+")]
        [Description("This command will add one SRJ (Stream/Recording) Joinier to the member once they have reached 3 the person will receive a Thicc Stream Badge")]
        public async Task ThiccGamersGuestAddSRJ(CommandContext ctx, [Description("The member that command will be ran on.")] DiscordMember member)
        {
            var permissinGranted = await PermissionManager.CheckPermissions(ctx, PermissionManager.PermissionType.Admin);
            if (!permissinGranted) return;
            var file = Path.Combine(Directory.CreateDirectory("Resources/").FullName, $"MembersSRJ.json");
            var data = new Dictionary<ulong, int>();
            if (File.Exists(file))
                data = JsonConvert.DeserializeObject<Dictionary<ulong, int>>(File.ReadAllText(file));
            if (data.Count <= 0 || data[member.Id] < 3)
            {
                if (data.ContainsKey(member.Id)) data[member.Id]++;
                else data.Add(member.Id, 1);
                await ctx.RespondAsync($"An SRJ has beend added to **{member.Username}** they have **{data[member.Id]}** SRJ's");
                File.WriteAllText(file, JsonConvert.SerializeObject(data, Formatting.Indented));

                if (data[member.Id] >= 3)
                {
                    await ctx.RespondAsync($"The member {member.Username}'s rank has pass the threshold but thre hasn't been a role for use to add.");
                    // Add role to user and keep track just incase we want to remove a time.
                    await member.GrantRoleAsync(ctx.Guild.Roles.ToList().Find(x => x.Value.Name == "Thicc Players").Value,
                        $"The member {member.Username}'s rank has pass the threshold but thre hasn't been a role for use to add.");
                }
                return;
            }
        }
        
        [Command("s-")]
        [Description("This command will add one SRJ (Stream/Recording) Joinier to the member once they have reached 3 the person will receive a Thicc Stream Badge")]
        public async Task ThiccGamersGuestSubtractSRJ(CommandContext ctx, [Description("The member that command will be ran on.")] DiscordMember member)
        {
            var permissinGranted = await PermissionManager.CheckPermissions(ctx, PermissionManager.PermissionType.Admin);
            if (!permissinGranted) return;
            var file = Path.Combine(Directory.CreateDirectory("Resources/").FullName, $"MembersSRJ.json");
            var data = new Dictionary<ulong, int>();
            if (File.Exists(file))
                data = JsonConvert.DeserializeObject<Dictionary<ulong, int>>(File.ReadAllText(file));
            if (data.Count <= 0 || data[member.Id] > 0)
            {
                if (data.ContainsKey(member.Id)) data[member.Id]--;
                else data.Add(member.Id, 0);
                await ctx.RespondAsync($"An SRJ has beend removed to **{member.Username}** they have **{data[member.Id]}** SRJ's remaining...");
                File.WriteAllText(file, JsonConvert.SerializeObject(data, Formatting.Indented));

                if (data[member.Id] < 3 && member.Roles.ToList().Find(x => x.Name == "Thicc Players") != null)
                {
                    await ctx.RespondAsync($"The member {member.Username}'s rank has went below the threshold but thre hasn't been a role for use to remove.");
                    await member.RevokeRoleAsync(ctx.Guild.Roles.ToList().Find(x => x.Value.Name == "Thicc Players").Value,
                   $"The member {member.Username}'s rank has went below the threshold but thre hasn't been a role for use to remove.");
                }
                return;
            }
        }


        /// <summary>
        /// A simple emoji based yes/no poll.
        /// </summary>
        /// <param name="ctx">CommandContext of the message that has executed this command</param>
        /// <param name="duration">Amount of time how long the poll should last.</param>
        /// <param name="question">Polls question</param>
        /// <returns></returns>
        [Command("emojipoll"), Description("Start a simple emoji poll for a simple yes/no question"), Cooldown(2, 30, CooldownBucketType.Guild)]
        public async Task EmojiPollAsync(CommandContext ctx, [Description("How long should the poll last. (e.g. 1m = 1 minute)")] TimeSpan duration, [Description("Poll question"), RemainingText] string question)
        {
            var permissinGranted = await PermissionManager.CheckPermissions(ctx, PermissionManager.PermissionType.Admin);
            if (!permissinGranted) return;

            if (!string.IsNullOrEmpty(question))
            {
                var client = ctx.Client;
                var interactivity = client.GetInteractivity();
                if (_pollEmojiCache == null)
                {
                    _pollEmojiCache = new[] {
                        DiscordEmoji.FromName(client, ":white_check_mark:"),
                        DiscordEmoji.FromName(client, ":x:")
                    };
                }

                // Creating the poll message
                var pollStartText = new StringBuilder();
                pollStartText.Append("**").Append("Poll started for:").AppendLine("**");
                pollStartText.Append(question);
                var pollStartMessage = await ctx.RespondAsync(pollStartText.ToString());

                // DoPollAsync adds automatically emojis out from an emoji array to a special message and waits for the "duration" of time to calculate results.
                var pollResult = await interactivity.DoPollAsync(pollStartMessage, _pollEmojiCache, PollBehaviour.DeleteEmojis, duration);
                var yesVotes = pollResult[0].Total;
                var noVotes = pollResult[1].Total;

                // Printing out the result
                var pollResultText = new StringBuilder();
                pollResultText.AppendLine(question);
                pollResultText.Append("Poll result: ");
                pollResultText.Append("**");
                if (yesVotes > noVotes)
                {
                    pollResultText.Append("Yes");
                }
                else if (yesVotes == noVotes)
                {
                    pollResultText.Append("Undecided");
                }
                else
                {
                    pollResultText.Append("No");
                }
                pollResultText.Append("**");
                await ctx.RespondAsync(pollResultText.ToString());
            }
            else
            {
                await ctx.RespondAsync("Error: the question can't be empty");
            }
        }

        [Command("sendtochannel"), Description("Send a message to a special channel")]
        public async Task SendToChannelAsync(CommandContext ctx, [Description("Target discord channel")] DiscordChannel targetChannel, [Description("Message to send"), RemainingText] string message)
        {
            var permissinGranted = await PermissionManager.CheckPermissions(ctx, PermissionManager.PermissionType.Admin);
            if (!permissinGranted) return;

            await targetChannel.SendMessageAsync(message);
        }
    }
}
