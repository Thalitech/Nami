using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using GScraper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserCommands
{
    public class UserCommandHub : BaseCommandModule
    {
        [RequirePrefixes("*")]
        [Description("Returns the current System time of the server that the bot is running on")]
        [Command("stime")]
        public async Task sTime(CommandContext ctx)
        {
            await ctx.RespondAsync($"The system time is: {DateTime.Now.TimeOfDay}");
        }
    }
}