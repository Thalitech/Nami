using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nami
{
    internal class Events
    {
        internal static Task OnMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
        {
            Task.Run(async () =>
            {
                var channel = e.Guild.Channels.ToList().Find(x => x.Value.Name.ToLower().Contains("announce")).Value;
                if (channel == null) channel = e.Guild.SystemChannel;
                var embed = new DiscordEmbedBuilder();
                embed.Timestamp = new DateTimeOffset(DateTime.Now);
                embed.Title = "Member Join";
                embed.Color = DiscordColor.Green;
                embed.Description = $"{e.Member.DisplayName} has joined, lest welcome them!";
                embed.Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = $"Thank you for your time. {e.Guild.Name} Team",
                };
                await channel.SendMessageAsync(embed: embed.Build());
            });
            return Task.CompletedTask;
        }
        internal static Task OnMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
        {
            Task.Run(async () =>
            {
                var channel = e.Guild.Channels.ToList().Find(x => x.Value.Name.ToLower().Contains("announce")).Value;
                if (channel == null) channel = e.Guild.SystemChannel;
                var embed = new DiscordEmbedBuilder();
                embed.Timestamp = new DateTimeOffset(DateTime.Now);
                embed.Title = "Member Left";
                embed.Color = DiscordColor.Red;
                embed.Description = $"{e.Member.DisplayName} has left, so sad to see you go";
                embed.Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = $"Thank you for your time. {e.Guild.Name} Team",
                };
                await channel.SendMessageAsync(embed: embed.Build());
            });
            return Task.CompletedTask;
        }

        internal static Task OnGuildsAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            Task.Run(async () => 
            {
                DiscordServer.servers.Add(new DiscordServer(e.Guild));
                Console.WriteLine($"{e.Guild.Name} is now registered ● {DateTime.Now}");
                await Task.CompletedTask;
            });
            return Task.CompletedTask;
        }
    }
}