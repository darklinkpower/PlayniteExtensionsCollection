using Playnite.SDK.Plugins;
using PluginsCommon;
using SteamCommon;
using SteamCommon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamSearch
{
    public class SteamSearcher : SearchContext
    {
        public SteamSearcher()
        {
            Description = "Enter search term";
            Label = "Steam Store";
            Hint = "Some hint here";
            Delay = 450;
        }

        public override IEnumerable<SearchItem> GetSearchResults(GetSearchResultsArgs args)
        {
            if (args.SearchTerm.IsNullOrEmpty())
            {
                return null;
            }
            
            var searchResults = SteamWeb.GetSteamSearchResults(args.SearchTerm);
            if (args.CancelToken.IsCancellationRequested)
            {
                return null;
            }

            var searchItems = new List<SearchItem>();
            foreach (var searchResult in searchResults)
            {
                searchItems.Add(new SearchItem($"{searchResult.Name}", new SearchItemAction("Open on web", () => { ProcessStarter.StartUrl(searchResult.StoreUrl); }))
                {
                    Description = GetSearchItemDescription(searchResult),
                    //Icon = new Uri(searchResult.BannerImageUrl),
                });
            }

            return searchItems;
        }

        private static string GetSearchItemDescription(StoreSearchResult searchResult)
        {
            if (searchResult.IsDiscounted)
            {
                return $"{searchResult.DiscountPercentage}% off - Current Price: ${string.Format("{0:N2}", searchResult.PriceFinal)} - Original Price: ${string.Format("{0:N2}", searchResult.PriceOriginal)}";
            }

            return $"Current Price: ${string.Format("{0:N2}", searchResult.PriceFinal)}";
        }
    }
}