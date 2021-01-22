using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nami
{
    internal class Events
    {
        internal static Task OnMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
        {
            new Thread(async () =>
            {
                var channel = e.Guild.Channels.ToList().Find(x => x.Value.Name.ToLower().Contains("announce")).Value;
                if (channel == null) channel = e.Guild.SystemChannel;
                var embed = new DiscordEmbedBuilder();
                embed.Timestamp = new DateTimeOffset(DateTime.Now);
                embed.Title = "Member Join";
                embed.Color = DiscordColor.Green;
                embed.Description = $"@everyone {e.Member.Mention} has joined, lest welcome them! :smile:";
                embed.Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = $"Thank you for your time. {e.Guild.Name} Team",
                };
                await channel.SendMessageAsync(embed: embed.Build());
            }).Start();
            return Task.CompletedTask;
        }
        internal static Task OnMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
        {
            new Thread(async () =>
            {
                var channel = e.Guild.Channels.ToList().Find(x => x.Value.Name.ToLower().Contains("announce")).Value;
                if (channel == null) channel = e.Guild.SystemChannel;
                var embed = new DiscordEmbedBuilder();
                embed.Timestamp = new DateTimeOffset(DateTime.Now);
                embed.Title = "Member Left";
                embed.Color = DiscordColor.Red;
                embed.Description = $"{e.Member.DisplayName} has left, so sad to see you go :cry:";
                embed.Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = $"Thank you for your time. {e.Guild.Name} Team",
                };
                await channel.SendMessageAsync(embed: embed.Build());
            }).Start();
            return Task.CompletedTask;
        }

        internal static Task OnGuildsAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            new Thread(async () => 
            {
                DiscordServer.servers.Add(new DiscordServer(e.Guild));
                Console.WriteLine($"{e.Guild.Name} is now registered ● {DateTime.Now}");
                await GuildUpdater(sender, e.Guild);
                await Task.CompletedTask;
            }).Start();
            return Task.CompletedTask;
        }

        internal static Task GuildUpdater(DiscordClient sender, DiscordGuild e)
        {
            new Thread(async () => 
            {
            update:
                var _members = await e.GetAllMembersAsync();
                var _channels = await e.GetChannelsAsync();
                var membercount = _members.Count;
                var mc = _members.ToList().FindAll(x => x.Presence?.Status == UserStatus.Online);
                var onlinecount = mc.Count; 
                var memberchIndex = -1;
                var onlinechIndex = -1;
                for (int i = 0; i < _channels.Count; i++)
                {
                    if (_channels[i].Name.ToLower().StartsWith("members:")) memberchIndex = i;
                    if (_channels[i].Name.ToLower().StartsWith("online:")) onlinechIndex = i;                    
                }
                if (memberchIndex > -1)
                    await _channels[memberchIndex].ModifyAsync(a => { a.Name = $"Members: {membercount}"; });
                if (onlinechIndex > -1)
                    await _channels[onlinechIndex].ModifyAsync(a => { a.Name = $"Online: {onlinecount}"; });

                await Task.Delay(10000);
                goto update;
            }).Start();
            return Task.CompletedTask;
        }
    }
}