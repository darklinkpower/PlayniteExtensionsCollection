using AngleSharp.Parser.Html;
using ComposableAsync;
using MetacriticMetadata.Models;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using RateLimiter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using FlowHttp;
using FlowHttp.Results;

namespace MetacriticMetadata.Services
{
    public class MetacriticService
    {
        private static readonly Playnite.SDK.ILogger logger = Playnite.SDK.LogManager.GetLogger();
        private const string searchGameWithPlatformTemplate = @"https://www.metacritic.com/search/game/{0}/results?search_type=advanced&plats[{1}]=1";
        private const string searchGameTemplate = @"https://www.metacritic.com/search/{0}/?category=13&page=1";
        private const string searchGameApiTemplate = @"https://fandom-prod.apigee.net/v1/xapi/finder/metacritic/search/{0}/web?apiKey={1}&offset=0&limit=30&mcoTypeId=13&componentName=search&componentDisplayName=Search&componentType=SearchResults&sortBy=";
        private static readonly Dictionary<string, string> defaultApiHeaders = new Dictionary<string, string>
        {
            {"Referer", @"https://www.metacritic.com"},
            {"User-Agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64)"}
        };

        private readonly TimeLimiter timeConstraint;
        private readonly MetacriticMetadataSettings settings;

        public MetacriticService(MetacriticMetadataSettings settings)
        {
            timeConstraint = TimeLimiter.GetFromMaxCountByInterval(1, TimeSpan.FromMilliseconds(600));
            this.settings = settings;
        }

        public async Task<HttpContentResult<string>> ExecuteRequestAsync(string requestUrl)
        {
            await timeConstraint;
            return HttpRequestFactory.GetHttpRequest()
                .WithUrl(requestUrl)
                .WithHeaders(defaultApiHeaders)
                .DownloadString();
        }

        public List<MetacriticSearchResult> GetGameSearchResults(Game game)
        {
            var metacriticPlatformId = GetGamePlatformMetacriticId(game);
            if (metacriticPlatformId.IsNullOrEmpty() || true)
            {
                return GetGameSearchResults(game.Name); //Fallback to search by name only
            }
            else
            {
                var requestUrl = string.Format(searchGameWithPlatformTemplate, game.Name.EscapeDataString(), metacriticPlatformId);
                return GetSearchResults(requestUrl);
            }
        }

        public List<MetacriticSearchResult> GetGameSearchResults(string gameName)
        {
            var requestUrl = string.Format(searchGameApiTemplate, gameName.EscapeDataString(), settings.ApiKey);
            return GetSearchResults(requestUrl);
        }

        private List<MetacriticSearchResult> GetSearchResults(string requestUrl)
        {
            var results = new List<MetacriticSearchResult>();
            if (settings.ApiKey.IsNullOrEmpty())
            {
                logger.Info("API Key has not been configured.");
                return results;
            }

            var searchRequest = Task.Run(async () => await ExecuteRequestAsync(requestUrl)).Result;
            if (!searchRequest.IsSuccess)
            {
                return results;
            }

            var response = Serialization.FromJson<MetacriticSearchResponse>(searchRequest.Content);
            foreach (var searchResult in response.Data.Items)
            {
                if (searchResult.Type != "game-title")
                {
                    continue;
                }

                var result = new MetacriticSearchResult
                {
                    Name = searchResult.Title,
                    Url = @"https://www.metacritic.com" + searchResult.CriticScoreSummary.Url.Replace("critic-reviews/", string.Empty),
                    Platforms = searchResult.Platforms.Select(x => x.Name).ToList(),
                    ReleaseDate = searchResult.ReleaseDate,
                    Description = !searchResult.Description.IsNullOrEmpty() ? searchResult.Description: string.Empty,
                };

                if (searchResult.CriticScoreSummary.Score.HasValue && searchResult.CriticScoreSummary.Score > 0)
                {
                    result.MetacriticScore = searchResult.CriticScoreSummary.Score.Value;
                }

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
    }
}