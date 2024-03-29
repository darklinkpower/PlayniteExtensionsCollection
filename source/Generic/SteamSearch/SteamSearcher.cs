﻿using Playnite.SDK;
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
        private readonly SteamSearchSettingsViewModel _settings;
        private const string _steamUriOpenUrlMask = @"steam://openurl/{0}";

        public SteamSearcher(SteamSearchSettingsViewModel settings)
        {
            Description = ResourceProvider.GetString("LOCSteam_Search_SearcherDescriptionLabel");
            Label = ResourceProvider.GetString("LOCSteam_Search_SteamStoreLabel");
            Hint = ResourceProvider.GetString("LOCSteam_Search_SearcherHint");
            Delay = 600;
            this._settings = settings;
        }

        public override IEnumerable<SearchItem> GetSearchResults(GetSearchResultsArgs args)
        {
            var searchItems = new List<SearchItem>();
            if (args.SearchTerm.IsNullOrEmpty())
            {
                return searchItems;
            }

            var searchResults = GetStoreSearchResults(args.SearchTerm);
            if (args.CancelToken.IsCancellationRequested)
            {
                return searchItems;
            }

            searchItems.AddRange(searchResults.Select(searchResult => GetSearchItemFromSearchResult(searchResult)));
            return searchItems;
        }

        private SearchItem GetSearchItemFromSearchResult(StoreSearchResult searchResult)
        {
            var searchItem = new SearchItem(GetSearchItemName(searchResult),
                new SearchItemAction(ResourceProvider.GetString("LOCSteam_Search_ItemActionLabelOpenOnWeb"),
                () => { ProcessStarter.StartUrl(searchResult.StoreUrl); }))
            {
                Description = GetSearchItemDescription(searchResult),
                Icon = searchResult.BannerImageUrl
            };

            searchItem.SecondaryAction = new SearchItemAction(
                ResourceProvider.GetString("LOCSteam_Search_ItemActionLabelOpenOnSteam"),
                () => { ProcessStarter.StartUrl(string.Format(_steamUriOpenUrlMask, searchResult.StoreUrl));});

            return searchItem;
        }

        private string GetSearchItemName(StoreSearchResult searchResult)
        {
            if (_settings.Settings.IndicateIfGameIsInLibrary && _settings.Settings.SteamIdsInLibrary.Contains(searchResult.GameId))
            {
                return $"{searchResult.Name} - {ResourceProvider.GetString("LOCSteam_Search_ItemActionIndicateGameInLibrary")}";
            }
            else
            {
                return searchResult.Name;
            }
        }

        private static string GetSearchItemDescription(StoreSearchResult searchResult)
        {
            if (!searchResult.IsReleased)
            {
                return ResourceProvider.GetString("LOCSteam_Search_ItemActionDescriptionNotReleasedGame");
            }

            if (searchResult.IsFree)
            {
                return ResourceProvider.GetString("LOCSteam_Search_ItemActionDescriptionFreeGame");
            }

            if (searchResult.IsDiscounted)
            {
                return string.Join(" - ", new string[3]
                {
                    string.Format(ResourceProvider.GetString("LOCSteam_Search_ItemActionDiscountPercentDescription"), searchResult.DiscountPercentage),
                    string.Format(ResourceProvider.GetString("LOCSteam_Search_ItemActionCurrentPriceDescription"), searchResult.Currency, searchResult.PriceFinal),
                    string.Format(ResourceProvider.GetString("LOCSteam_Search_ItemActionOriginalPriceDescription"), searchResult.Currency, searchResult.PriceOriginal)
                });
            }

            return string.Format(ResourceProvider.GetString("LOCSteam_Search_ItemActionCurrentPriceDescription"), searchResult.Currency, searchResult.PriceFinal);
        }

        private List<StoreSearchResult> GetStoreSearchResults(string searchTerm)
        {
            if (_settings.Settings.UseCountryStore)
            {
                return SteamWeb.GetSteamSearchResults(searchTerm, _settings.Settings.SelectedManualCountry);
            }
            else
            {
                return SteamWeb.GetSteamSearchResults(searchTerm, null);
            }
        }
    }
}