using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FilterPresetsQuickLauncher
{
    public class FilterPresetsSearchContext : SearchContext
    {
        private readonly FilterPresetsQuickLauncherSettingsViewModel settings;
        private readonly IPlayniteAPI playniteApi;

        public FilterPresetsSearchContext(FilterPresetsQuickLauncherSettingsViewModel settings, IPlayniteAPI playniteApi)
        {
            Description = ResourceProvider.GetString("LOCFiltersPresetsQL_FilterPresetSearchDescription");
            Label = ResourceProvider.GetString("LOCFiltersPresetsQL_FilterPresetSearchLabel");
            Hint = ResourceProvider.GetString("LOCFiltersPresetsQL_FilterPresetSearchHint");
            Delay = 80;
            this.settings = settings;
            this.playniteApi = playniteApi;
            PlayniteUtilities.AddTextIcoFontResource("FH_FilterIcon", "\xEF29");
        }

        public override IEnumerable<SearchItem> GetSearchResults(GetSearchResultsArgs args)
        {
            var clearFilterList = new List<SearchItem>
            {
                GetSearchItem(new FilterPreset(){
                    Name = ResourceProvider.GetString("LOCFiltersPresetsQL_ClearFiltersLabel"),
                    Settings = new FilterPresetSettings()
                })
            };

            if (!playniteApi.Database.FilterPresets.HasItems())
            {
                return clearFilterList;
            }

            var filterItems = !args.SearchTerm.IsNullOrEmpty();
            var searchItems = new List<SearchItem>();
            foreach (var filterPreset in playniteApi.Database.FilterPresets)
            {
                if (args.CancelToken.IsCancellationRequested)
                {
                    return new List<SearchItem>();
                }

                if (!filterItems || TextComparer.MatchTextFilter(filterPreset.Name, args.SearchTerm))
                {
                    searchItems.Add(GetSearchItem(filterPreset));
                }
            }

            searchItems.Sort((x, y) => x.Name.CompareTo(y.Name));
            if (!filterItems)
            {
                clearFilterList.AddRange(searchItems);
                return clearFilterList;
            }
            else
            {
                return searchItems;
            }
        }

        private SearchItem GetSearchItem(FilterPreset filterPreset)
        {
            return new SearchItem(
                filterPreset.Name,
                new SearchItemAction(ResourceProvider.GetString("LOCFiltersPresetsQL_ActivateActionLabel"),
                () => { ApplyFilterPreset(filterPreset); }),
                "FH_FilterIcon"
            );
        }

        private void ApplyFilterPreset(FilterPreset filterPreset)
        {
            PlayniteUtilities.ApplyFilterPreset(playniteApi, filterPreset);
            WindowHelper.BringMainWindowToForeground();
        }
    }
}