using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImporterforAnilist
{
    public class ImporterForAnilistSettings : ObservableObject
    {
        public string AccountAccessCode { get; set; } = string.Empty;
        public string PropertiesPrefix { get; set; } = string.Empty;
        public bool ImportAnimeLibrary { get; set; } = true;
        public bool ImportMangaLibrary { get; set; } = true;
        public bool UpdateUserScoreOnLibUpdate { get; set; } = true;
        public bool UpdateCompletionStatusOnLibUpdate { get; set; } = true;
        public bool UpdateProgressOnLibUpdate { get; set; } = false;
        private string browserPath = string.Empty;
        public string BrowserPath { get => browserPath; set => SetValue(ref browserPath, value); }
        public Guid PlanWatchId { get; set; } = Guid.Empty;
        public Guid WatchingId { get; set; } = Guid.Empty;
        public Guid PausedId { get; set; } = Guid.Empty;
        public Guid DroppedId { get; set; } = Guid.Empty;
        public Guid CompletedId { get; set; } = Guid.Empty;
        public Guid RewatchingId { get; set; } = Guid.Empty;
    }

    public class ImporterForAnilistSettingsViewModel : ObservableObject, ISettings
    {
        private readonly ImporterForAnilist plugin;
        private ImporterForAnilistSettings editingClone { get; set; }
        private IPlayniteAPI playniteApi;
        private ImporterForAnilistSettings settings;
        public ImporterForAnilistSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }
        private List<CompletionStatus> completionStatuses;
        public List<CompletionStatus> CompletionStatuses
        {
            get => completionStatuses;
            set
            {
                completionStatuses = value;
                OnPropertyChanged();
            }
        }

        public ImporterForAnilistSettingsViewModel(ImporterForAnilist plugin, IPlayniteAPI playniteApi)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;
            this.playniteApi = playniteApi;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<ImporterForAnilistSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new ImporterForAnilistSettings();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
            CompletionStatuses = playniteApi.Database.CompletionStatuses.OrderBy(x => x.Name).ToList();
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

        public RelayCommand<object> LoginCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                Login();
            });
        }

        private void Login()
        {
            ProcessStarter.StartUrl(@"https://anilist.co/api/v2/oauth/authorize?client_id=5706&response_type=token");
        }

        public RelayCommand<object> SelectBrowserExecutableCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectBrowserExecutable();
            });
        }
        private void SelectBrowserExecutable()
        {
            var executablePath = playniteApi.Dialogs.SelectFile("Exe|*.exe");
            if (!string.IsNullOrEmpty(executablePath))
            {
                settings.BrowserPath = executablePath;
            }
        }

        public RelayCommand<object> RemoveBrowserCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                settings.BrowserPath = string.Empty;
            });
        }

    }
}