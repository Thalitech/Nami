using System;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Reddit;

namespace Nami
{

    class RedditManager : IDisposable
    {
        private RedditClient reddit;
        private CommandContext context;

        public RedditManager(CommandContext ctx = default)
        {
            reddit = new RedditClient(AssetDatabase.Get<string>(ValueType.RedditAppID, true).GetAwaiter().GetResult(),
                appSecret: AssetDatabase.Get<string>(ValueType.RedditSecret, true).GetAwaiter().GetResult(), 
                userAgent: "Nami");
            if(ctx != null)
            {
                this.context = ctx;
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        internal DiscordEmbed RequestNSFW()
        {
            var p = reddit.Subreddit("HENTAI_GIF").Posts.Top[0];

            return default;
        }
    }
}
