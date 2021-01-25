using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.ServiceModel.Syndication;
using Nami.Services;

namespace Nami.Modules.Search.Services
{
    public class NewsService : NamiHttpService
    {
        private const string NewsUrl = "https://news.google.com/news/rss/headlines/section/topic/";

        public override bool IsDisabled => false;


        public static IReadOnlyList<SyndicationItem>? FetchNews(CultureInfo culture, string? topic = null)
        {
            if (string.IsNullOrWhiteSpace(topic))
                topic = "world";
            string lang = culture.TwoLetterISOLanguageName;
            string url = $"{NewsUrl}{WebUtility.UrlEncode(topic).ToUpperInvariant()}?ned={lang}&hl={lang}";
            return RssFeedsService.GetFeedResults(url);
        }
    }
}
