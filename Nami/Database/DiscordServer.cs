using DSharpPlus.Entities;
using System.Collections.Generic;

namespace Nami.Database
{
    internal class DiscordServer
    {
        public static List<DiscordServer> servers = new List<DiscordServer>();
        private DiscordGuild server;

        public DiscordServer(DiscordGuild server)
        {
            this.server = server;
        }
    }
}