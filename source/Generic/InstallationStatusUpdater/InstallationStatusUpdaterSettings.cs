using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallationStatusUpdater
{
    public class InstallationStatusUpdaterSettings : ObservableObject
    {
        public bool UrlActionIsInstalled { get; set; } = true;
        public bool ScriptActionIsInstalled { get; set; } = true;
        public bool UseOnlyFirstRomDetection { get; set; } = false;
        public bool SkipHandledByPlugin { get; set; } = true;
        public bool OnlyUsePlayActionGameActions { get; set; } = false;
        public bool UpdateOnStartup { get; set; } = true;
        public bool UpdateOnLibraryUpdate { get; set; } = true;
        public bool UpdateLocTagsOnLibUpdate { get; set; } = false;
        public bool UpdateStatusOnUsbChanges { get; set; } = true;
        public bool UpdateStatusOnDirChanges { get; set; } = false;
        public bool EnableInstallButtonAction { get; set; } = true;
        [DontSerialize]
        private List<SelectableDirectory> detectionDirectories { get; set; } = new List<SelectableDirectory>();
        public List<SelectableDirectory> DetectionDirectories
        {
            get => detectionDirectories;
            set
            {
                detectionDirectories = value;
                OnPropertyChanged();
            }
        }
    }

    public class InstallationStatusUpdaterSettingsViewModel : ObservableObject, ISettings
    {
        private IPlayniteAPI PlayniteApi;
        private readonly InstallationStatusUpdater plugin;
        private InstallationStatusUpdaterSettings editingClone { get; set; }

        private InstallationStatusUpdaterSettings settings;
        public InstallationStatusUpdaterSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public InstallationStatusUpdaterSettingsViewModel(InstallationStatusUpdater plugin, IPlayniteAPI playniteApi)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<InstallationStatusUpdaterSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new InstallationStatusUpdaterSettings();
            }

            PlayniteApi = playniteApi;
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

        public RelayCommand<object> AddDetectionDirectoryCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                var newDir = PlayniteApi.Dialogs.SelectFolder();
                if (string.IsNullOrEmpty(newDir))
                {
                    return;
                }
                if (Settings.DetectionDirectories.FirstOrDefault(s => s.DirectoryPath.Equals(newDir, StringComparison.OrdinalIgnoreCase)) != null)
                {
                    return;
                }

                var newDetectionDirectory = new SelectableDirectory
                {
                    DirectoryPath = newDir,
                    Enabled = true,
                    ScanSubDirs = false
                };
                Settings.DetectionDirectories.Add(newDetectionDirectory);
                Settings.DetectionDirectories = Serialization.GetClone(Settings.DetectionDirectories);
            });
        }

        public RelayCommand<IList<object>> RemoveDetectionDirectoryCommand
        {
            get => new RelayCommand<IList<object>>((items) =>
            {
                foreach (SelectableDirectory item in items.ToList())
                {
                    Settings.DetectionDirectories.Remove(item);
                }
                Settings.DetectionDirectories = Serialization.GetClone(Settings.DetectionDirectories);
            }, (items) => items != null && items.Count > 0);
        }
    }
}