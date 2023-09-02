using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FilterPresetsQuickLauncher
{
    public class FilterPresetsSearchContext : SearchContext
    {
        private readonly FilterPresetsQuickLauncherSettings settings;
        private static readonly string defaultSearchIconName = "FPQL_FilterIcon";
        private readonly IPlayniteAPI playniteApi;
        private readonly string imagesDirectory;

        public FilterPresetsSearchContext(FilterPresetsQuickLauncherSettings settings, IPlayniteAPI playniteApi, string imagesDirectory)
        {
            Description = ResourceProvider.GetString("LOCFiltersPresetsQL_FilterPresetSearchDescription");
            Label = ResourceProvider.GetString("LOCFiltersPresetsQL_FilterPresetSearchLabel");
            Hint = ResourceProvider.GetString("LOCFiltersPresetsQL_FilterPresetSearchHint");
            Delay = 80;
            this.settings = settings;
            this.playniteApi = playniteApi;
            this.imagesDirectory = imagesDirectory;
            PlayniteUtilities.AddTextIcoFontResource(defaultSearchIconName, "\xEF29");
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
                GetSearchItemIcon(filterPreset)
            );
        }

        private string GetSearchItemIcon(FilterPreset filterPreset)
        {
            var displaySettings = settings.FilterPresetsDisplaySettings.FirstOrDefault(x => x.Id == filterPreset.Id);
            if (displaySettings != null && !displaySettings.Image.IsNullOrEmpty())
            {
                var imagePath = Path.Combine(imagesDirectory, displaySettings.Image);
                if (FileSystem.FileExists(imagePath))
                {
                    return imagePath;
                }
            }

            return defaultSearchIconName;
        }

        private void ApplyFilterPreset(FilterPreset filterPreset)
        {
            PlayniteUtilities.ApplyFilterPreset(playniteApi, filterPreset);
            WindowHelper.BringMainWindowToForeground();
        }
    }
}