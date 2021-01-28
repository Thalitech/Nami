using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Nami.Extensions
{
    internal static class DiscordUserExtensions
    {
        public static async Task<bool> IsMemberOfAsync(this DiscordUser user, DiscordGuild guild)
            => await guild.GetMemberSilentAsync(user.Id) is { };

        public static string ToDiscriminatorString(this DiscordUser user)
            => $"{user.Username}#{user.Discriminator}";
    }
}
