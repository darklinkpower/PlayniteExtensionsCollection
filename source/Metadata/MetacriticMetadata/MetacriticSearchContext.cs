using MetacriticMetadata.Domain.Entities;
using MetacriticMetadata.Domain.Interfaces;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MetacriticMetadata
{
    public class MetacriticSearchContext : SearchContext
    {
        private readonly IMetacriticService _metacriticService;
        private readonly ILogger _logger;
        private readonly MetacriticMetadataSettingsViewModel _settingsViewModel;

        public MetacriticSearchContext(IMetacriticService metacriticService, ILogger logger, MetacriticMetadataSettingsViewModel settingsViewModel)
        {
            Description = "Enter search term";
            Label = "Metacritic";
            Hint = "Searches games on Metacritic";
            Delay = 600;
            _metacriticService = metacriticService;
            _logger = logger;
            _settingsViewModel = settingsViewModel;
        }

        public override IEnumerable<SearchItem> GetSearchResults(GetSearchResultsArgs args)
        {
            var searchItems = new List<SearchItem>();
            var searchTerm = args.SearchTerm;
            if (searchTerm.IsNullOrEmpty())
            {
                return searchItems;
            }

            var searchResults = GetMetacriticSearchOptions(searchTerm, _settingsViewModel.Settings.ApiKey, args.CancelToken);
            if (args.CancelToken.IsCancellationRequested)
            {
                return searchItems;
            }

            foreach (var searchResult in searchResults)
            {
                var url = searchResult.Url;
                var searchItem = new SearchItem(
                    searchResult.Name,
                    new SearchItemAction("Open on browser",
                    () => { ProcessStarter.StartUrl(url); })
                )
                {
                    Description = GetSearchItemDescription(searchResult),
                    SecondaryAction = new SearchItemAction("Open on Playnite",
                    () => { PlayniteUtilities.OpenUrlOnWebView(url); })
                };

                searchItems.Add(searchItem);
            }

            return searchItems;
        }

        private List<MetacriticSearchResult> GetMetacriticSearchOptions(string gameName, string apiKey, CancellationToken cancellationToken)
        {
            try
            {
                var results = _metacriticService.GetGameSearchResultsAsync(gameName, apiKey, cancellationToken).GetAwaiter().GetResult();
                return results;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get Metacritic search options.");
                return new List<MetacriticSearchResult>();
            }
        }

        private string GetSearchItemDescription(MetacriticSearchResult searchResult)
        {
            var platforms = string.Join(", ", searchResult.Platforms);
            var releaseDate = searchResult.ReleaseDate;
            var metacriticScore = searchResult.MetacriticScore.HasValue
                                  ? $" - {searchResult.MetacriticScore}"
                                  : string.Empty;

            return $"{platforms} - {releaseDate}{metacriticScore}";
        }
    }
}
