using FlowHttp;
using HtmlAgilityPack;
using NewsViewer.Domain.ValueObjects;
using NewsViewer.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TemporaryCache;

namespace NewsViewer.Infrastructure
{
    public class SteamNewsService
    {
        private readonly CacheManager<string, SteamNewsFeed> _newsCacheManager;
        private readonly Dictionary<string, string> headers = new Dictionary<string, string> { ["Accept"] = "text/xml", ["Accept-Encoding"] = "utf-8", ["User-Agent"] = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36 Vivaldi/4.3" };
        const string steamRssTemplate = @"https://store.steampowered.com/feeds/news/app/{0}/l={1}";
        private readonly List<SteamHtmlTransformDefinition> _descriptionTransformElems;
        private readonly ILogger _logger;
        private readonly string _steamLanguage;

        public SteamNewsService(ILogger logger, string steamLanguage, TimeSpan cacheAliveTime)
        {
            _logger = logger;
            _steamLanguage = steamLanguage;
            _newsCacheManager = new CacheManager<string, SteamNewsFeed>(cacheAliveTime);
            _descriptionTransformElems = new List<SteamHtmlTransformDefinition>()
            {
                new SteamHtmlTransformDefinition("span", "bb_strike", "strike"),
                new SteamHtmlTransformDefinition("div", "bb_h1", "h1"),
                new SteamHtmlTransformDefinition("div", "bb_h2", "h2"),
                new SteamHtmlTransformDefinition("div", "bb_h3", "h3"),
                new SteamHtmlTransformDefinition("div", "bb_h4", "h4"),
                new SteamHtmlTransformDefinition("div", "bb_h5", "h5")
            };
        }

        public async Task<SteamNewsFeed> GetNewsAsync(SteamNewsRequestOptions steamNewsRequestOptions)
        {
            if (steamNewsRequestOptions.ObtainFromCache && _newsCacheManager.TryGetValue(steamNewsRequestOptions.SteamId, out var cache))
            {
                return cache;
            }

            if (!steamNewsRequestOptions.ObtainFromHttpRequest)
            {
                return null;
            }

            var request = HttpRequestFactory.GetHttpRequest()
                .WithUrl(string.Format(steamRssTemplate, steamNewsRequestOptions.SteamId, _steamLanguage))
                .WithHeaders(headers);
            var result = await request.DownloadStringAsync();
            if (!result.IsSuccess)
            {
                return null;
            }

            var newsFeed = ParseRssResponse(result.Content);
            if (newsFeed is null)
            {
                return null;
            }

            var savedCache = _newsCacheManager.Add(steamNewsRequestOptions.SteamId, newsFeed);
            return savedCache;
        }

        private SteamNewsFeed ParseRssResponse(string xmlContent)
        {
            try
            {
                var document = new HtmlDocument();
                document.LoadHtml(xmlContent);

                var descriptionDocument = new HtmlDocument();
                var channelNode = document.DocumentNode.SelectSingleNode("//channel");
                var itemNodes = channelNode.SelectNodes(".//item");
                var channel = new SteamNewsFeed(
                    channelNode.SelectSingleNode("title")?.InnerText,
                    channelNode.SelectSingleNode("link")?.InnerText,
                    channelNode.SelectSingleNode("description")?.InnerText,
                    channelNode.SelectSingleNode("language")?.InnerText,
                    itemNodes?.Select(x => CreateRssItem(descriptionDocument, x)).ToList() ?? new List<SteamNewsArticle>()
                );

                return channel;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error while parsing rss feed");
                return null;
            }
        }

        private SteamNewsArticle CreateRssItem(HtmlDocument descriptionDocument, HtmlNode itemNode)
        {
            var title = itemNode.SelectSingleNode(".//title")?.InnerText.HtmlDecode();
            var link = itemNode.SelectSingleNode(".//guid")?.InnerText;
            var pubDate = DateTime.ParseExact(itemNode.SelectSingleNode(".//pubdate")?
                .InnerText, "ddd, dd MMM yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            var author = itemNode.SelectSingleNode(".//author")?.InnerText.HtmlDecode();

            var isPermaLink = itemNode.SelectSingleNode(".//guid")?.GetAttributeValue("isPermaLink", string.Empty) == "true";
            var guidValue = itemNode.SelectSingleNode(".//guid")?.InnerText;

            var newsGuid = new SteamNewsIdentifier(isPermaLink, guidValue);

            var descriptionNode = itemNode.SelectSingleNode(".//description");
            string description = null;

            if (descriptionNode != null)
            {
                descriptionDocument.LoadHtml(descriptionNode.InnerText.HtmlDecode());
                FixNewsDescriptionElements(descriptionDocument);
                description = descriptionDocument.DocumentNode.InnerHtml;
            }

            return new SteamNewsArticle(title, description, link, pubDate, author, newsGuid);
        }

        private void FixNewsDescriptionElements(HtmlDocument descriptionDocument)
        {
            if (!descriptionDocument.DocumentNode.HasChildNodes)
            {
                return;
            }

            foreach (var childNode in descriptionDocument.DocumentNode.ChildNodes)
            {
                foreach (var transformElem in _descriptionTransformElems)
                {
                    if (childNode.Name != transformElem.Name)
                    {
                        continue;
                    }

                    if (childNode.GetAttributeValue("class", string.Empty) != transformElem.ClassName)
                    {
                        continue;
                    }

                    childNode.Name = transformElem.NewName;
                    break;
                }
            }
        }


    }
}