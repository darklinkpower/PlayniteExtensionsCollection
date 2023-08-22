using Playnite.SDK;
using Playnite.SDK.Plugins;
using PluginsCommon;
using SearchCollection.Interfaces;
using SearchCollection.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace SearchCollection
{
    internal class SearchCollectionSearchContext : SearchContext
    {
        private SearchCollectionSettings settings;
        private List<ISearchDefinition> defaultSearches;
        private string iconsDirectory;
        private string userIconsDirectory;
        private readonly IPlayniteAPI playniteApi;

        public SearchCollectionSearchContext(IPlayniteAPI playniteApi, SearchCollectionSettings settings, List<ISearchDefinition> defaultSearches, string iconsDirectory, string userIconsDirectory)
        {
            this.settings = settings;
            this.defaultSearches = defaultSearches;
            this.iconsDirectory = iconsDirectory;
            this.userIconsDirectory = userIconsDirectory;
            this.playniteApi = playniteApi;

            Description = ResourceProvider.GetString("LOCSearch_Collection_SearchContextDescription");
            Label = "Search Collection";
            Hint = ResourceProvider.GetString("LOCSearch_Collection_SearchContextHint");
            Delay = 50;
        }

        public override IEnumerable<SearchItem> GetSearchResults(GetSearchResultsArgs args)
        {
            var searchItems = new List<SearchItem>();
            if (args.SearchTerm.IsNullOrEmpty())
            {
                return searchItems;
            }

            foreach (var searchDefinition in defaultSearches)
            {
                if (settings.DefaultSearchesSettings.TryGetValue(searchDefinition.Name, out bool isEnabled) && isEnabled)
                {
                    searchItems.Add(
                        new SearchItem(
                            searchDefinition.Name,
                            new SearchItemAction(ResourceProvider.GetString("LOCSearch_Collection_SearchActionLabel"),
                            () => { ProcessStarter.StartUrl(searchDefinition.GetSearchUrl(args.SearchTerm)); }),
                            Path.Combine(iconsDirectory, searchDefinition.Icon)
                        )
                        {
                            Description = string.Format(ResourceProvider.GetString("LOCSearch_Collection_SearchOnSiteActionLabel"), searchDefinition.Name),
                            SecondaryAction = GetSecondaryAction(searchDefinition, args)
                        }
                    );
                }
            }

            foreach (var searchDefinition in settings.SearchDefinitions)
            {
                if (!searchDefinition.IsEnabled)
                {
                    continue;
                }

                searchItems.Add(
                    new SearchItem(
                        searchDefinition.Name,
                        new SearchItemAction(ResourceProvider.GetString("LOCSearch_Collection_SearchActionLabel"),
                        () => { ProcessStarter.StartUrl(searchDefinition.GetSearchUrl(args.SearchTerm)); }),
                        Path.Combine(userIconsDirectory, searchDefinition.Icon)
                    )
                    {
                        Description = string.Format(ResourceProvider.GetString("LOCSearch_Collection_SearchOnSiteActionLabel"), searchDefinition.Name),
                        SecondaryAction = GetSecondaryAction(searchDefinition, args)
                    }
                );
            }

            searchItems.Sort((x, y) => x.Name.CompareTo(y.Name));
            return searchItems;
        }

        private SearchItemAction GetSecondaryAction(ISearchDefinition searchDefinition, GetSearchResultsArgs args)
        {
            return new SearchItemAction(ResourceProvider.GetString("LOCSearch_Collection_SearchOnPlayniteActionLabel"),
            () =>
            {
                OpenUrlOnWebview(searchDefinition.GetSearchUrl(args.SearchTerm));
            });
        }

        private SearchItemAction GetSecondaryAction(CustomSearchDefinition searchDefinition, GetSearchResultsArgs args)
        {
            return new SearchItemAction(ResourceProvider.GetString("LOCSearch_Collection_SearchOnPlayniteActionLabel"),
            () =>
            {
                OpenUrlOnWebview(searchDefinition.GetSearchUrl(args.SearchTerm));
            });
        }

        private void OpenUrlOnWebview(string url)
        {
            var scalingFactor = 1.25;
            using (var webView = playniteApi.WebViews.CreateView(Convert.ToInt32(Math.Floor(1900 / scalingFactor)), Convert.ToInt32(Math.Floor(1000 / scalingFactor))))
            {
                webView.Navigate(url);
                webView.OpenDialog();
            }
        }
    }
}