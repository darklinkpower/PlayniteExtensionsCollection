using MetacriticMetadata.Domain.Entities;
using MetacriticMetadata.Domain.Interfaces;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetacriticMetadata
{
    public class MetacriticSearchContext : SearchContext
    {
        private readonly IMetacriticService _metacriticService;
        private readonly MetacriticMetadataSettingsViewModel _settingsViewModel;

        public MetacriticSearchContext(IMetacriticService metacriticService, MetacriticMetadataSettingsViewModel settingsViewModel)
        {
            Description = "Enter search term";
            Label = "Metacritic";
            Hint = "Searches games on Metacritic";
            Delay = 600;
            _metacriticService = metacriticService;
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

            var searchResults = Task.Run(
                () => _metacriticService.GetGameSearchResultsAsync(searchTerm, _settingsViewModel.Settings.ApiKey, args.CancelToken))
                .GetAwaiter().GetResult();
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

        private string GetSearchItemDescription(MetacriticSearchResult searchResult)
        {
            var description = $"{string.Join(", ", searchResult.Platforms)} - {searchResult.ReleaseDate}";
            if (searchResult.MetacriticScore.HasValue)
            {
                description += $" - {searchResult.MetacriticScore}";
            }

            return description;
        }
    }
}
