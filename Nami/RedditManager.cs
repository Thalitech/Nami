using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Reddit;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;
using Reddit.Things;

namespace Nami
{

    class RedditManager : IDisposable
    {
        private RedditClient reddit;
        private CommandContext context;

        public RedditManager(CommandContext ctx = default)
        {
            var appID = new ResourceFile<string>()["reddit_appid", encrypt: true];
            var appSecret = new ResourceFile<string>()["reddit_secret", encrypt: true];
            reddit = new RedditClient(appID, appSecret: appSecret, userAgent: "Nami");
            reddit.Account.Me.Over18 = true;
            reddit.Account.UpdatePrefsAsync(new AccountPrefsSubmit(reddit.Account.Prefs(), null, false, null)).GetAwaiter().GetResult();
            Console.WriteLine($"Reddit Nami is {(reddit.Account.Me.Over18 ? "18+" : "Under age")}");
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
