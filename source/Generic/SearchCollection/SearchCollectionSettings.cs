using Playnite.SDK;
using Playnite.SDK.Data;
using PluginsCommon;
using WebCommon;
using SearchCollection.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchCollection.Interfaces;
using System.Collections.ObjectModel;

namespace SearchCollection
{
    public class SearchCollectionSettings : ObservableObject
    {
        [DontSerialize]
        private Dictionary<string, bool> defaultSearchesSettings = new Dictionary<string, bool>();
        public Dictionary<string, bool> DefaultSearchesSettings { get => defaultSearchesSettings; set => SetValue(ref defaultSearchesSettings, value); }
        private List<CustomSearchDefinition> searchDefinitions { get; set; } = new List<CustomSearchDefinition>();

        public List<CustomSearchDefinition> SearchDefinitions
        {
            get => searchDefinitions;
            set
            {
                searchDefinitions = value;
                OnPropertyChanged();
            }
        }

        public bool FirstStartCompleted = false;
    }

    public class NameValueObject : ObservableObject
    {
        public string Name { get; }

        private bool _value;
        public bool Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }

        public NameValueObject(string name, bool value)
        {
            Name = name;
            Value = value;
        }
    }

    public class SearchCollectionSettingsViewModel : ObservableObject, ISettings
    {
        private readonly SearchCollection plugin;
        private SearchCollectionSettings editingClone { get; set; }
        public ObservableCollection<NameValueObject> EditingDefaultSearchesSettings { get; private set; }

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

        public SearchCollectionSettingsViewModel(SearchCollection plugin, IPlayniteAPI playniteApi, string userIconsDirectory, string pluginInstallPath, List<ISearchDefinition> defaultSearches)
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

            foreach (var search in defaultSearches)
            {
                if (!settings.DefaultSearchesSettings.ContainsKey(search.Name))
                {
                    settings.DefaultSearchesSettings.Add(search.Name, true);
                }
            }

            if (!settings.FirstStartCompleted)
            {
                foreach (var iconFile in new string[] { "Default.png", "Google.png" })
                {
                    var iconPath = Path.Combine(plugin.iconsDirectory, iconFile);
                    var targetDefaultIcon = Path.Combine(userIconsDirectory, iconFile);
                    FileSystem.CopyFile(iconPath, targetDefaultIcon);
                }

                settings.SearchDefinitions.Add(new CustomSearchDefinition
                {
                    Name = "Google",
                    IsEnabled = true,
                    Icon = "Google.png",
                    SearchTemplate = @"https://www.google.com/search?q=%s"
                });

                settings.FirstStartCompleted = true;
                plugin.SavePluginSettings(Settings);
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
            NewDefinitionName = string.Empty;
            NewDefinitionSearchTemplate = string.Empty;
            EditingDefaultSearchesSettings = settings.DefaultSearchesSettings
                .OrderBy(x => x.Key).Select(x => new NameValueObject(x.Key, x.Value)).ToObservable();
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
            Settings.DefaultSearchesSettings = EditingDefaultSearchesSettings.ToDictionary(kv => kv.Name, kv => kv.Value);
            Settings.SearchDefinitions.Sort((x, y) => x.Name.CompareTo(y.Name));
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

                var newDefinition = new CustomSearchDefinition
                {
                    IsEnabled = true,
                    Name = NewDefinitionName,
                    SearchTemplate = NewDefinitionSearchTemplate
                };

                newDefinition.Icon = GetNewDefinitionIcon(newDefinition);
                Settings.SearchDefinitions.Add(newDefinition);

                Settings.SearchDefinitions = Settings.SearchDefinitions.OrderBy(x => x.Name).ToList();
            }, () => AddButtonEnabled);
        }

        private string GetNewDefinitionIcon(CustomSearchDefinition newDefinition)
        {
            var domain = newDefinition.SearchTemplate.Replace($"%s", "Braid");
            var iconDownloadUrl = string.Format(@"http://www.google.com/s2/favicons?domain={0}", domain);
            var iconName = Guid.NewGuid().ToString() + ".png";
            var iconPath = Path.Combine(userIconsDirectory, iconName);
            HttpDownloader.GetRequestBuilder().WithUrl(iconDownloadUrl).WithDownloadTo(iconPath).DownloadFile();
            if (FileSystem.FileExists(iconPath))
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
                foreach (CustomSearchDefinition searchDefinition in items.ToList())
                {
                    if (searchDefinition.Icon != "Default.png")
                    {
                        var iconPath = Path.Combine(userIconsDirectory, searchDefinition.Icon);
                        FileSystem.DeleteFileSafe(iconPath);
                    }

                    Settings.SearchDefinitions.Remove(searchDefinition);
                }

                Settings.SearchDefinitions = Settings.SearchDefinitions.OrderBy(x => x.Name).ToList();
            }, (items) => items != null && items.Count > 0);
        }
    }
}