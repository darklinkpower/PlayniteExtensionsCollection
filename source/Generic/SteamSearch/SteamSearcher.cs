using Playnite.SDK;
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
        private readonly SteamSearchSettingsViewModel settings;
        private const string steamUriOpenUrlMask = @"steam://openurl/{0}";

        public SteamSearcher(SteamSearchSettingsViewModel settings)
        {
            Description = ResourceProvider.GetString("LOCSteam_Search_SearcherDescriptionLabel");
            Label = ResourceProvider.GetString("LOCSteam_Search_SteamStoreLabel");
            Hint = ResourceProvider.GetString("LOCSteam_Search_SearcherHint");
            Delay = 600;
            this.settings = settings;
        }

        public override IEnumerable<SearchItem> GetSearchResults(GetSearchResultsArgs args)
        {
            if (args.SearchTerm.IsNullOrEmpty())
            {
                return null;
            }

            var searchResults = GetStoreSearchResults(args.SearchTerm);
            if (args.CancelToken.IsCancellationRequested)
            {
                return null;
            }

            var searchItems = new List<SearchItem>();
            foreach (var searchResult in searchResults)
            {
                searchItems.Add(GetSearchItemFromSearchResult(searchResult));
            }

            return searchItems;
        }

        private static SearchItem GetSearchItemFromSearchResult(StoreSearchResult searchResult)
        {
            var searchItem = new SearchItem($"{searchResult.Name}",
                new SearchItemAction(ResourceProvider.GetString("LOCSteam_Search_ItemActionLabelOpenOnWeb"),
                () => { ProcessStarter.StartUrl(searchResult.StoreUrl); }))
            {
                Description = GetSearchItemDescription(searchResult),
                Icon = searchResult.BannerImageUrl,
            };

            searchItem.SecondaryAction = new SearchItemAction(
                ResourceProvider.GetString("LOCSteam_Search_ItemActionLabelOpenOnSteam"),
                () => { ProcessStarter.StartUrl(string.Format(steamUriOpenUrlMask, searchResult.StoreUrl));});

            return searchItem;
        }

        private static string GetSearchItemDescription(StoreSearchResult searchResult)
        {
            if (searchResult.IsDiscounted)
            {
                return string.Join(" - ", new string[3]
                {
                    string.Format(ResourceProvider.GetString("LOCSteam_Search_ItemActionDiscountPercentDescription"), searchResult.DiscountPercentage),
                    string.Format(ResourceProvider.GetString("LOCSteam_Search_ItemActionCurrentPriceDescription"), searchResult.PriceFinal),
                    string.Format(ResourceProvider.GetString("LOCSteam_Search_ItemActionOriginalPriceDescription"), searchResult.PriceOriginal)
                });
            }

            return string.Format(ResourceProvider.GetString("LOCSteam_Search_ItemActionCurrentPriceDescription"), searchResult.PriceFinal);
        }

        private List<StoreSearchResult> GetStoreSearchResults(string searchTerm)
        {
            if (settings.Settings.UseCountryStore)
            {
                return SteamWeb.GetSteamSearchResults(searchTerm, settings.Settings.SelectedManualCountry);
            }
            else
            {
                return SteamWeb.GetSteamSearchResults(searchTerm, null);
            }
        }
    }
}