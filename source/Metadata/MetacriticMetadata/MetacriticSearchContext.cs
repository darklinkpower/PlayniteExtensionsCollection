using MetacriticMetadata.Models;
using MetacriticMetadata.Services;
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
        private readonly MetacriticService metacriticService;

        public MetacriticSearchContext(MetacriticService metacriticService)
        {
            Description = "Enter search term";
            Label = "Metacritic";
            Hint = "Searches games on Metacritic";
            Delay = 600;
            this.metacriticService = metacriticService;
        }

        public override IEnumerable<SearchItem> GetSearchResults(GetSearchResultsArgs args)
        {
            var searchItems = new List<SearchItem>();
            var searchTerm = args.SearchTerm;
            if (searchTerm.IsNullOrEmpty())
            {
                return searchItems;
            }

            var searchResults = metacriticService.GetGameSearchResults(searchTerm);
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
