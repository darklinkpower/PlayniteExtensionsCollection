using AngleSharp.Parser.Html;
using ComposableAsync;
using MetacriticMetadata.Models;
using Playnite.SDK.Models;
using RateLimiter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using WebCommon;

namespace MetacriticMetadata.Services
{
    public class MetacriticService
    {
        private static readonly Playnite.SDK.ILogger logger = Playnite.SDK.LogManager.GetLogger();
        private const string searchGameWithPlatformTemplate = @"https://www.metacritic.com/search/game/{0}/results?search_type=advanced&plats[{1}]=1";
        private const string searchGameTemplate = @"https://www.metacritic.com/search/game/{0}/results";
        private readonly TimeLimiter timeConstraint;

        public MetacriticService()
        {
            timeConstraint = TimeLimiter.GetFromMaxCountByInterval(1, TimeSpan.FromMilliseconds(600));
        }

        public async Task<DownloadStringResult> ExecuteRequestAsync(string requestUrl)
        {
            await timeConstraint;
            return HttpDownloader.DownloadStringWithHeaders(requestUrl, GetSearchHeaders());
        }

        public List<MetacriticSearchResult> GetGameSearchResults(Game game)
        {
            var metacriticPlatformId = GetGamePlatformMetacriticId(game);
            if (metacriticPlatformId.IsNullOrEmpty())
            {
                return GetGameSearchResults(game.Name); //Fallback to search by name only
            }
            else
            {
                var requestUrl = string.Format(searchGameWithPlatformTemplate, game.Name.UrlEncode(), metacriticPlatformId);
                return ParseGameSearchPage(requestUrl);
            }
        }

        public List<MetacriticSearchResult> GetGameSearchResults(string gameName)
        {
            var requestUrl = string.Format(searchGameTemplate, gameName.UrlEncode());
            return ParseGameSearchPage(requestUrl);
        }

        private List<MetacriticSearchResult> ParseGameSearchPage(string requestUrl)
        {
            var results = new List<MetacriticSearchResult>();
            var searchRequest = Task.Run(async () => await ExecuteRequestAsync(requestUrl)).Result;
            if (!searchRequest.Success)
            {
                return results;
            }

            var parser = new HtmlParser();
            var searchPage = parser.Parse(searchRequest.Result);
            var elements = searchPage.QuerySelectorAll("ul.search_results li");
            if (elements == null || !elements.HasItems())
            {
                return results;
            }

            foreach (var resultElem in elements)
            {
                var dataName = resultElem.QuerySelector("a");
                var releaseInfo = resultElem.QuerySelector("p");
                var platformElem = releaseInfo.QuerySelector("span.platform");

                int? metacriticScore = null;
                var metacriticScoreElem = resultElem.QuerySelector(".metascore_w");
                if (int.TryParse(metacriticScoreElem.InnerHtml, out var metacriticScoreP))
                {
                    metacriticScore = metacriticScoreP;
                }

                var result = new MetacriticSearchResult
                {
                    Name = dataName.InnerHtml.HtmlDecode().Trim(),
                    Url = @"https://www.metacritic.com" + dataName.GetAttribute("href"),
                    Platform = platformElem.InnerHtml,
                    ReleaseInfo = releaseInfo.InnerHtml
                        .Trim()
                        .Replace(platformElem.OuterHtml, string.Empty)
                        .Trim(),
                    Description = resultElem.QuerySelector(".deck")?.InnerHtml.HtmlDecode() ?? string.Empty,
                    MetacriticScore = metacriticScore
                };

                results.Add(result);

            }

            return results.Distinct().ToList(); // There are cases of duplicate games being returned in searches, e.g. Marco & The Galaxy Dragon on PC
        }

        private static string GetGamePlatformMetacriticId(Game game)
        {
            if (!game.Platforms.HasItems())
            {
                return null;
            }

            foreach (var platform in game.Platforms)
            {
                if (platform.SpecificationId.IsNullOrEmpty())
                {
                    continue;
                }

                switch (platform.SpecificationId)
                {
                    case "sony_playstation4":
                        return "72496";
                    case "sony_playstation3":
                        return "1";
                    case "xbox_one":
                        return "80000";
                    case "xbox360":
                        return "2";
                    case "pc_windows":
                        return "3";
                    case "nintendo_ds":
                        return "4";
                    case "nintendo_dsi":
                        return "4";
                    case "nintendo_3ds":
                        return "16";
                    case "sony_vita":
                        return "67365";
                    case "sony_psp":
                        return "7";
                    case "nintendo_wii":
                        return "8";
                    case "nintendo_wiiu":
                        return "1";
                    case "nintendo_switch":
                        return "268409";
                    case "sony_playstation2":
                        return "6";
                    case "sony_playstation":
                        return "10";
                    case "nintendo_gameboyadvance":
                        return "11";
                    //case "Iphone/Ipad":
                    //    return "0";
                    case "xbox":
                        return "12";
                    case "nintendo_gamecube":
                        return "13";
                    case "nintendo_64":
                        return "14";
                    case "sega_dreamcast":
                        return "15";
                    default:
                        continue;
                }
            }

            return null;
        }

        private static Dictionary<string, string> GetSearchHeaders()
        {
            return new Dictionary<string, string>
            {
                {"Referer", @"https://www.metacritic.com"},
                {"User-Agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64)"}
            };
        }
    }
}