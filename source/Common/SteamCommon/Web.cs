using AngleSharp.Parser.Html;
using Playnite.SDK;
using Playnite.SDK.Data;
using PluginsCommon.Web;
using SteamCommon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SteamCommon
{
    class SteamWeb
    {
        private static ILogger logger = LogManager.GetLogger();
        private const string steamGameSearchUrl = @"https://store.steampowered.com/search/?term={0}&ignore_preferences=1&category1=998";
        public static List<GenericItemOption> GetSteamSearchGenericItemOptions(string searchTerm)
        {
            return GetSteamSearchResults(searchTerm).Select(x => new GenericItemOption(x.Name, x.GameId)).ToList();
        }

        public static List<StoreSearchResult> GetSteamSearchResults(string searchTerm)
        {
            var results = new List<StoreSearchResult>();
            var searchPageSrc = HttpDownloader.DownloadStringAsync(string.Format(steamGameSearchUrl, searchTerm)).GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(searchPageSrc))
            {
                var parser = new HtmlParser();
                var searchPage = parser.Parse(searchPageSrc);
                foreach (var gameElem in searchPage.QuerySelectorAll(".search_result_row"))
                {
                    var title = gameElem.QuerySelector(".title").InnerHtml;
                    var releaseDate = gameElem.QuerySelector(".search_released").InnerHtml;
                    if (gameElem.HasAttribute("data-ds-packageid"))
                    {
                        continue;
                    }

                    var gameId = gameElem.GetAttribute("data-ds-appid");
                    results.Add(new StoreSearchResult
                    {
                        Name = HttpUtility.HtmlDecode(title),
                        Description = HttpUtility.HtmlDecode(releaseDate),
                        GameId = gameId
                    });
                }
            }

            logger.Debug($"Obtained {results.Count} games from Steam search term {searchTerm}");
            return results;
        }

        public static SteamAppDetails GetSteamAppDetails(string steamId)
        {
            var url = string.Format(@"https://store.steampowered.com/api/appdetails?appids={0}", steamId);
            var downloadedString = HttpDownloader.DownloadStringAsync(url).GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(downloadedString))
            {
                var parsedData = Serialization.FromJson<Dictionary<string, SteamAppDetails>>(downloadedString);
                if (parsedData.Keys?.Any() == true)
                {
                    var response = parsedData[parsedData.Keys.First()];
                    if (response.success == true && response.data != null)
                    {
                        return response;
                    }
                }
            }

            return null;
        }
    }
}
