using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Nami.Attributes;
using Nami.Common;
using Nami.Exceptions;
using Nami.Extensions;
using Nami.Modules.Misc.Services;

namespace Nami.Modules.Misc
{
    [Group("random"), Module(ModuleType.Misc), NotBlocked]
    [Description("Random gibberish.")]
    [Aliases("rnd", "rand")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class RandomModule : NamiServiceModule<RandomService>
    {
        #region random choose
        [Command("choice")]
        [Aliases("select", "choose")]
        public Task ChooseAsync(CommandContext ctx,
                               [RemainingText, Description("desc-choice-list")] string list)
        {
            if (string.IsNullOrWhiteSpace(list))
                throw new InvalidCommandUsageException(ctx, "cmd-err-choice");

            string choice = this.Service.Choice(list);
            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{Emojis.Dice} {Formatter.Strip(choice)}",
                Color = this.ModuleColor,
            });
        }
        #endregion

        #region random raffle
        [Command("raffle")]
        [Aliases("chooseuser")]
        public Task RaffleAsync(CommandContext ctx,
                               [Description("desc-role")] DiscordRole? role = null)
        {
            IEnumerable<DiscordMember> members = ctx.Guild.Members.Values;
            if (role is { })
                members = members.Where(m => m.Roles.Contains(role));

            if (!members.Any())
                throw new InvalidCommandUsageException(ctx, "cmd-err-choice-none");

            DiscordMember raffled = new SecureRandom().ChooseRandomElement(members);
            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedDescription("fmt-raffle", raffled.Mention);
            });
        }
        #endregion
    }
}
