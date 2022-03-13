using AngleSharp.Parser.Html;
using ExtraMetadataLoader.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PluginsCommon.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ExtraMetadataLoader.Common
{
    public class SteamCommon
    {
        private static ILogger logger = LogManager.GetLogger();
        private readonly Guid steamPluginId = Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab");
        private static readonly Regex steamLinkRegex = new Regex(@"^https?:\/\/store\.steampowered\.com\/app\/(\d+)", RegexOptions.Compiled);
        private const string steamGameSearchUrl = @"https://store.steampowered.com/search/?term={0}&ignore_preferences=1&category1=998";
        
        public static string GetSteamId(Game game, bool useLinksDetection = false)
        {
            if (game.PluginId == Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab"))
            {
                logger.Debug("Steam id found for Steam game by pluginId");
                return game.GameId;
            }
            else if (useLinksDetection && game.Links != null)
            {
                foreach (Link gameLink in game.Links)
                {
                    var linkMatch = steamLinkRegex.Match(gameLink.Url);
                    if (linkMatch.Success)
                    {
                        logger.Debug("Steam id found found in store link");
                        return linkMatch.Groups[1].Value;
                    }
                }
            }

            return null;
        }

        public static bool IsGameSteamGame(Game game)
        {
            if (game.PluginId == Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

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