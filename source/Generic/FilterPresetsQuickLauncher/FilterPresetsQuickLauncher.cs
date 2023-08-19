using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using FilterPresetsQuickLauncher.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FilterPresetsQuickLauncher
{
    public class FilterPresetsQuickLauncher : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly FontFamily applicationFont;
        private readonly List<TopPanelItem> topPanelList;
        private readonly List<SidebarItem> sidebarItems;

        private FilterPresetsQuickLauncherSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("ef9df36c-24c2-418c-8468-eed95a09d950");

        public FilterPresetsQuickLauncher(IPlayniteAPI api) : base(api)
        {
            settings = new FilterPresetsQuickLauncherSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            Searches = new List<SearchSupport>
            {
                new SearchSupport("fp",
                    ResourceProvider.GetString("LOCFiltersPresetsQL_SearchHelperDescription"),
                    new FilterPresetsSearchContext(settings, api))
            };

            applicationFont = Fonts.SystemFontFamilies.FirstOrDefault(x => x.Source == PlayniteApi.ApplicationSettings.FontFamilyName);
            if (applicationFont is null)
            {
                applicationFont = Fonts.SystemFontFamilies.FirstOrDefault(x => x.Source == "Arial");
            }

            PlayniteApi.UriHandler.RegisterSource("FiltersPresetsQL", (args) =>
            {
                var argument = args.Arguments[0];
                if (argument.IsNullOrEmpty())
                {
                    return;
                }

                FilterPreset filterPreset = null;
                if (Guid.TryParse(argument, out var id))
                {
                    filterPreset = api.Database.FilterPresets.FirstOrDefault(x => x.Id == id);
                }
                else
                {
                    filterPreset = api.Database.FilterPresets.FirstOrDefault(x => x.Name.Equals(argument, StringComparison.InvariantCultureIgnoreCase));
                }

                if (filterPreset is null)
                {
                    api.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCFiltersPresetsQL_FilterPresetNotFoundLabel"), argument));
                }
                else
                {
                    PlayniteUtilities.ApplyFilterPreset(api, filterPreset);
                    WindowHelper.BringMainWindowToForeground();
                }
            });

            topPanelList = new List<TopPanelItem>();
            sidebarItems = new List<SidebarItem>();
        }

        public override IEnumerable<TopPanelItem> GetTopPanelItems()
        {
            // Unfortunately Playnite loads its database after requesting items to extensions
            // so it's not possible to use the items in the database in any way

            //var topPanelList = new List<TopPanelItem>();
            //if (!PlayniteApi.Database.FilterPresets.HasItems())
            //{
            //    return topPanelList;
            //}

            foreach (var displaySettings in settings.Settings.FilterPresetsDisplaySettings)
            {
                var filterPreset = PlayniteApi.Database.FilterPresets?.FirstOrDefault(x => x.Id == displaySettings.Id);
                //if (filterPreset is null)
                //{
                //    continue;
                //}

                if (!displaySettings.ShowInTopPanel)
                {
                    continue;
                }

                topPanelList.Add(GetFilterPresetTopPanelItem(filterPreset, displaySettings));
            }

            return topPanelList;
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            foreach (var topPanelItem in topPanelList)
            {
                var foundFilterPreset = PlayniteApi.Database.FilterPresets.FirstOrDefault(x => x.Id.ToString() == topPanelItem.Title);
                if (foundFilterPreset != null)
                {
                    topPanelItem.Title = string.Format(ResourceProvider.GetString("LOCFiltersPresetsQL_FilterPresetFormatLabel"), foundFilterPreset.Name);
                }
            }

            foreach (var sidebarItem in sidebarItems)
            {
                // In the comparison, the first character that is used as a prefix to sort is skipped
                var foundFilterPreset = PlayniteApi.Database.FilterPresets.FirstOrDefault(x => x.Id.ToString() == sidebarItem.Title.Substring(1));
                if (foundFilterPreset != null)
                {
                    sidebarItem.Title = string.Format(ResourceProvider.GetString("LOCFiltersPresetsQL_FilterPresetFormatLabel"), foundFilterPreset.Name);
                }
            }
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            CleanupFilterPresetsDisplaySettings();
        }

        private void CleanupFilterPresetsDisplaySettings()
        {
            var settingsUpdated = false;
            foreach (var displaySettings in settings.Settings.FilterPresetsDisplaySettings.ToList())
            {
                var foundFilterPreset = PlayniteApi.Database.FilterPresets.FirstOrDefault(x => x.Id == displaySettings.Id);
                if (foundFilterPreset is null)
                {
                    settings.Settings.FilterPresetsDisplaySettings.Remove(displaySettings);
                    settingsUpdated = true;
                }
            }

            if (settingsUpdated)
            {
                SavePluginSettings(settings.Settings);
            }
        }

        private TopPanelItem GetFilterPresetTopPanelItem(FilterPreset filterPreset, FilterPresetDisplaySettings displaySettings)
        {
            return new TopPanelItem
            {
                Icon = GetFilterPresetIcon(filterPreset, displaySettings),
                Title = filterPreset?.Name ?? displaySettings.Id.ToString(),
                Visible = displaySettings.ShowInTopPanel,
                Activated = () =>
                {
                    var foundFilterPreset = PlayniteApi.Database.FilterPresets.FirstOrDefault(x => x.Id == displaySettings.Id);
                    if (foundFilterPreset != null)
                    {
                        PlayniteUtilities.ApplyFilterPreset(PlayniteApi, foundFilterPreset);
                    }
                }
            };
        }

        private object GetFilterPresetIcon(FilterPreset filterPreset, FilterPresetDisplaySettings displaySettings)
        {
            if (!displaySettings.Image.IsNullOrEmpty())
            {
                var fullIconPath = Path.Combine(GetPluginUserDataPath(), displaySettings.Image);
                if (FileSystem.FileExists(fullIconPath))
                {
                    return fullIconPath;
                }
            }

            if (displaySettings.DisplayName.IsNullOrEmpty())
            {
                return GetGenericTextIcon(filterPreset?.Name ?? displaySettings.Name, true);
            }
            else
            {
                return GetGenericTextIcon(displaySettings.DisplayName, false);
            }
        }

        private TextBlock GetGenericTextIcon(string text, bool useAcronym)
        {
            return new TextBlock
            {
                FontFamily = applicationFont,
                Text = useAcronym ? MakeAcronym(text) : text,
                FontSize = 16
            };
        }

        private static string MakeAcronym(string str)
        {
            if (str.IsNullOrEmpty())
            {
                return string.Empty;
            }

            StringBuilder acronymBuilder = new StringBuilder();
            foreach (string word in str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!char.IsLetterOrDigit(word[0]) && !char.IsWhiteSpace(word[0]))
                {
                    acronymBuilder.Append(word[0]);
                }

                for (int i = 0; i < word.Length; i++)
                {
                    if (char.IsLetterOrDigit(word[i]))
                    {
                        acronymBuilder.Append(char.ToUpper(word[i]));
                        break;
                    }
                }

                if (!char.IsLetterOrDigit(word[word.Length - 1]) && !char.IsWhiteSpace(word[word.Length - 1]))
                {
                    acronymBuilder.Append(word[word.Length - 1]);
                }
            }

            return acronymBuilder.ToString();
        }

        public override IEnumerable<SidebarItem> GetSidebarItems()
        {
            // Unfortunately Playnite loads its database after requesting items to extensions
            // so it's not possible to use the items in the database in any way
            //var sidebarItems = new List<SidebarItem>();
            //if (!PlayniteApi.Database.FilterPresets.HasItems())
            //{
            //    return sidebarItems;
            //}

            // Playnite sorts the sidebar items by Title order. As a hack to sort, it's possible
            // to add a Unicode char prefix in increasing order to manually sort them
            int startCodePoint = 0x1F600; // Start with the code point of the first emoji in the Unicode range
            int numberOfCharacters = settings.Settings.FilterPresetsDisplaySettings.Count(); // Number of characters to generate

            for (int i = 0; i < numberOfCharacters; i++)
            {
                char unicodeCharPrefix = (char)(startCodePoint + i);
                var displaySettings = settings.Settings.FilterPresetsDisplaySettings[i];
                var filterPreset = PlayniteApi.Database.FilterPresets?.FirstOrDefault(x => x.Id == displaySettings.Id);
                //if (filterPreset is null)
                //{
                //    continue;
                //}

                if (!displaySettings.ShowInSidebar)
                {
                    continue;
                }

                sidebarItems.Add(GetFilterPresetSidebarItem(unicodeCharPrefix, filterPreset, displaySettings));
            }

            return sidebarItems;
        }

        private SidebarItem GetFilterPresetSidebarItem(char unicodeCharPrefix, FilterPreset filterPreset, FilterPresetDisplaySettings displaySettings)
        {
            return new SidebarItem
            {
                Title = $"{unicodeCharPrefix}{displaySettings.Id}",
                Type = SiderbarItemType.Button,
                Visible = displaySettings.ShowInSidebar,
                Icon = GetFilterPresetIcon(filterPreset, displaySettings),
                Activated = () =>
                {
                    var foundFilterPreset = PlayniteApi.Database.FilterPresets.FirstOrDefault(x => x.Id == displaySettings.Id);
                    if (foundFilterPreset != null)
                    {
                        PlayniteUtilities.ApplyFilterPreset(PlayniteApi, foundFilterPreset);
                    }
                }
            };
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new FilterPresetsQuickLauncherSettingsView();
        }

    }
}