using ComposableAsync;
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
using System.Threading;
using MetacriticMetadata.Domain.Entities;
using MetacriticMetadata.Domain.Interfaces;
using Playnite.SDK;

namespace MetacriticMetadata.Services
{
    public class MetacriticService : IMetacriticService
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private const string _searchGameWithPlatformTemplate = @"https://www.metacritic.com/search/game/{0}/results?search_type=advanced&plats[{1}]=1";
        private const string _searchGameApiTemplate = @"https://backend.metacritic.com/v1/xapi/finder/metacritic/search/{0}/web?apiKey={1}&offset=0&limit=30&mcoTypeId=13&componentName=search&componentDisplayName=Search&componentType=SearchResults&sortBy=";
        private readonly TimeLimiter _timeConstraint;
        private readonly Dictionary<string, string> _platformSpecIdToMetacriticId;
        private static readonly Dictionary<string, string> defaultApiHeaders = new Dictionary<string, string>
        {
            {"Referer", @"https://www.metacritic.com"},
            {"User-Agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64)"}
        };

        public MetacriticService()
        {
            _timeConstraint = TimeLimiter.GetFromMaxCountByInterval(1, TimeSpan.FromMilliseconds(600));
            _platformSpecIdToMetacriticId = new Dictionary<string, string>
            {
                { "sony_playstation4", "72496" },
                { "sony_playstation3", "1" },
                { "xbox_one", "80000" },
                { "xbox360", "2" },
                { "pc_windows", "3" },
                { "nintendo_ds", "4" },
                { "nintendo_dsi", "4" },
                { "nintendo_3ds", "16" },
                { "sony_vita", "67365" },
                { "sony_psp", "7" },
                { "nintendo_wii", "8" },
                { "nintendo_wiiu", "1" },
                { "nintendo_switch", "268409" },
                { "sony_playstation2", "6" },
                { "sony_playstation", "10" },
                { "nintendo_gameboyadvance", "11" },
                { "xbox", "12" },
                { "nintendo_gamecube", "13" },
                { "nintendo_64", "14" },
                { "sega_dreamcast", "15" }
            };
        }

        private async Task<HttpContentResult<string>> ExecuteRequestAsync(string requestUrl, CancellationToken cancelToken)
        {
            await _timeConstraint;
            return await HttpRequestFactory.GetHttpRequest()
                .WithUrl(requestUrl)
                .WithHeaders(defaultApiHeaders)
                .DownloadStringAsync(cancelToken);
        }

        public async Task<List<MetacriticSearchResult>> GetGameSearchResultsAsync(Game game, string apiKey, CancellationToken cancelToken = default)
        {
            var metacriticPlatformId = GetGamePlatformMetacriticId(game);
            if (true || metacriticPlatformId.IsNullOrEmpty())
            {
                return await GetGameSearchResultsAsync(game.Name, apiKey, cancelToken); // Fallback to search by name only
            }
            else
            {
                var requestUrl = string.Format(_searchGameWithPlatformTemplate, game.Name.EscapeDataString(), metacriticPlatformId);
                return await GetSearchResultsAsync(requestUrl, apiKey, cancelToken);
            }
        }

        public async Task<List<MetacriticSearchResult>> GetGameSearchResultsAsync(string gameName, string apiKey, CancellationToken cancelToken = default)
        {
            var requestUrl = string.Format(_searchGameApiTemplate, gameName.EscapeDataString(), apiKey);
            return await GetSearchResultsAsync(requestUrl, apiKey, cancelToken);
        }

        private async Task<List<MetacriticSearchResult>> GetSearchResultsAsync(string requestUrl, string apiKey, CancellationToken cancelToken = default)
        {
            var results = new List<MetacriticSearchResult>();
            var searchRequest = await ExecuteRequestAsync(requestUrl, cancelToken);
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
                    Description = !searchResult.Description.IsNullOrEmpty() ? searchResult.Description : string.Empty,
                    MetacriticScore = searchResult.CriticScoreSummary.Score > 0 ? searchResult.CriticScoreSummary.Score.Value : (int?)null,
                };

                results.Add(result);
            }

            return results.Distinct().ToList(); // There are cases of duplicate games being returned in searches
        }

        private string GetGamePlatformMetacriticId(Game game)
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

                if (_platformSpecIdToMetacriticId.TryGetValue(platform.SpecificationId, out var metacriticId))
                {
                    return metacriticId;
                }
            }

            return null;
        }
    }

}