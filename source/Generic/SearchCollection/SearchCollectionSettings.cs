using Playnite.SDK;
using Playnite.SDK.Data;
using PluginsCommon;
using PluginsCommon.Web;
using SearchCollection.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchCollection
{
    public class SearchCollectionSettings : ObservableObject
    {

        [DontSerialize]
        private bool searchIsEnabledMetacritic = true;
        public bool SearchIsEnabledMetacritic { get => searchIsEnabledMetacritic; set => SetValue(ref searchIsEnabledMetacritic, value); }

        [DontSerialize]
        private bool searchIsEnabledPcgw = true;
        public bool SearchIsEnabledPcgw { get => searchIsEnabledPcgw; set => SetValue(ref searchIsEnabledPcgw, value); }

        [DontSerialize]
        private bool searchIsEnabledTwitch = true;
        public bool SearchIsEnabledTwitch { get => searchIsEnabledTwitch; set => SetValue(ref searchIsEnabledTwitch, value); }
        [DontSerialize]
        private bool searchIsEnabledSteam = true;
        public bool SearchIsEnabledSteam { get => searchIsEnabledSteam; set => SetValue(ref searchIsEnabledSteam, value); }
        [DontSerialize]
        private bool searchIsEnabledSteamDb = true;
        public bool SearchIsEnabledSteamDb { get => searchIsEnabledSteamDb; set => SetValue(ref searchIsEnabledSteamDb, value); }
        [DontSerialize]
        private bool searchIsEnabledSteamGridDB = true;
        public bool SearchIsEnabledSteamGridDB { get => searchIsEnabledSteamGridDB; set => SetValue(ref searchIsEnabledSteamGridDB, value); }
        [DontSerialize]
        private bool searchIsEnabledVndb = true;
        public bool SearchIsEnabledVndb { get => searchIsEnabledVndb; set => SetValue(ref searchIsEnabledVndb, value); }
        [DontSerialize]
        private bool searchIsEnabledYoutube = true;
        public bool SearchIsEnabledYoutube { get => searchIsEnabledYoutube; set => SetValue(ref searchIsEnabledYoutube, value); }
        private List<SearchDefinition> searchDefinitions { get; set; } = new List<SearchDefinition>();
        public List<SearchDefinition> SearchDefinitions
        {
            get => searchDefinitions;
            set
            {
                searchDefinitions = value;
                OnPropertyChanged();
            }
        }


    }

    public class SearchCollectionSettingsViewModel : ObservableObject, ISettings
    {
        private readonly SearchCollection plugin;
        private SearchCollectionSettings editingClone { get; set; }

        private SearchCollectionSettings settings;
        public SearchCollectionSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        private bool AddButtonEnabled = false;
        private string newDefinitionName = string.Empty;
        public string NewDefinitionName
        {
            get => newDefinitionName;
            set
            {
                newDefinitionName = value;
                SetIsAddButtonEnabled();
                OnPropertyChanged();
            }
        }

        private void SetIsAddButtonEnabled()
        {
            AddButtonEnabled = !newDefinitionName.IsNullOrEmpty() &&
                !newDefinitionSearchTemplate.IsNullOrEmpty();
        }

        private string newDefinitionSearchTemplate = string.Empty;
        private IPlayniteAPI playniteApi;
        private string userIconsDirectory;
        private readonly string pluginInstallPath;

        public string NewDefinitionSearchTemplate
        {
            get => newDefinitionSearchTemplate;
            set
            {
                newDefinitionSearchTemplate = value;
                SetIsAddButtonEnabled();
                OnPropertyChanged();
            }

        }
        public SearchCollectionSettingsViewModel(SearchCollection plugin, IPlayniteAPI playniteApi, string userIconsDirectory, string pluginInstallPath)
        {
            this.playniteApi = playniteApi;
            this.userIconsDirectory = userIconsDirectory;
            this.pluginInstallPath = pluginInstallPath;
            
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<SearchCollectionSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new SearchCollectionSettings();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = editingClone;
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }

        public RelayCommand AddSearchDefinitionCommand
        {
            get => new RelayCommand(() =>
            {
                if (NewDefinitionName.IsNullOrEmpty())
                {
                    return;
                }

                if (NewDefinitionSearchTemplate.IsNullOrEmpty())
                {
                    return;
                }

                if (settings.SearchDefinitions
                        .Any(x => x.SearchTemplate
                            .Equals(NewDefinitionSearchTemplate, StringComparison.OrdinalIgnoreCase)))
                {
                    playniteApi.Dialogs.ShowErrorMessage(ResourceProvider.GetString("LOCSearch_Collection_SettingsLabelNewDefinitionAlreadyExists"), "Search Collection");
                    return;
                }

                var newDefinition = new SearchDefinition
                {
                    IsEnabled = true,
                    Name = NewDefinitionName,
                    SearchTemplate = NewDefinitionSearchTemplate
                };

                newDefinition.Icon = GetNewDefinitionIcon(newDefinition);
                Settings.SearchDefinitions.Add(newDefinition);
                Settings.SearchDefinitions = Serialization.GetClone(Settings.SearchDefinitions);
            }, () => AddButtonEnabled);
        }

        private string GetNewDefinitionIcon(SearchDefinition newDefinition)
        {
            var domain = newDefinition.SearchTemplate.Replace($"%s", "Braid");
            var iconDownloadUrl = string.Format(@"http://www.google.com/s2/favicons?domain={0}", domain);
            var iconName = Guid.NewGuid().ToString() + ".png";
            var iconPath = Path.Combine(userIconsDirectory, iconName);
            var dlSuccess = HttpDownloader.DownloadFileAsync(iconDownloadUrl, iconPath).GetAwaiter().GetResult();
            if (dlSuccess)
            {
                return iconName;
            }
            else
            {
                return "Default.png";
            }
        }

        public RelayCommand<IList<object>> RemoveSearchDefinitionsCommand
        {
            get => new RelayCommand<IList<object>>((items) =>
            {
                foreach (SearchDefinition searchDefinition in items.ToList())
                {
                    if (searchDefinition.Icon != "Default.png")
                    {
                        var iconPath = Path.Combine(userIconsDirectory, searchDefinition.Icon);
                        FileSystem.DeleteFileSafe(iconPath);
                    }

                    Settings.SearchDefinitions.Remove(searchDefinition);
                }

                Settings.SearchDefinitions = Serialization.GetClone(Settings.SearchDefinitions);
            }, (items) => items != null && items.Count > 0);
        }
    }
}