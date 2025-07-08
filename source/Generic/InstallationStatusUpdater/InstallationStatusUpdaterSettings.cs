using InstallationStatusUpdater.Domain.ValueObjects;
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
        public bool ScanGamesHandledByLibPlugins { get; set; } = false;
        public bool OnlyUsePlayActionsForDetection { get; set; } = true;
        public bool UpdateOnStartup { get; set; } = true;
        public bool UpdateOnLibraryUpdate { get; set; } = true;
        public bool UpdateLocTagsOnLibUpdate { get; set; } = false;
        public bool UpdateStatusOnUsbChanges { get; set; } = true;
        public bool UpdateStatusOnDirChanges { get; set; } = false;
        public bool DetectFilesFromLaunchArguments { get; set; } = true;

        private List<WatchedDirectory> _detectionDirectories = new List<WatchedDirectory>();
        public List<WatchedDirectory> DetectionDirectories
        {
            get => _detectionDirectories;
            set
            {
                _detectionDirectories = value;
                OnPropertyChanged();
            }
        }
    }

    public class InstallationStatusUpdaterSettingsViewModel : ObservableObject, ISettings
    {
        private readonly IPlayniteAPI _playniteApi;
        private readonly InstallationStatusUpdater _plugin;
        private InstallationStatusUpdaterSettings _editingClone;

        private InstallationStatusUpdaterSettings _settings;
        public InstallationStatusUpdaterSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand AddDetectionDirectoryCommand { get; }
        public RelayCommand<IList<object>> RemoveDetectionDirectoriesCommand { get; }

        public InstallationStatusUpdaterSettingsViewModel(InstallationStatusUpdater plugin, IPlayniteAPI playniteApi)
        {
            _playniteApi = playniteApi ?? throw new ArgumentNullException(nameof(plugin));
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
            var savedSettings = _plugin.LoadPluginSettings<InstallationStatusUpdaterSettings>();
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new InstallationStatusUpdaterSettings();
            }

            AddDetectionDirectoryCommand = new RelayCommand(() => AddNewDetectionDirectory());
            RemoveDetectionDirectoriesCommand = new RelayCommand<IList<object>>(
                items => RemoveDetectionDirectories(items),
                (items) => items != null && items.Count > 0);
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            _editingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = _editingClone;
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            _plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }

        private void AddNewDetectionDirectory()
        {
            var newDir = _playniteApi.Dialogs.SelectFolder();
            if (newDir.IsNullOrEmpty())
            {
                return;
            }

            if (Settings.DetectionDirectories
                .FirstOrDefault(s => s.DirectoryPath.Equals(newDir, StringComparison.OrdinalIgnoreCase)) != null)
            {
                return;
            }

            var newDetectionDirectory = new WatchedDirectory
            {
                DirectoryPath = newDir,
                Enabled = true,
                ScanSubDirs = false
            };

            Settings.DetectionDirectories.Add(newDetectionDirectory);
            Settings.DetectionDirectories = Serialization.GetClone(Settings.DetectionDirectories);
        }

        private void RemoveDetectionDirectories(IList<object> items)
        {
            var directories = items.OfType<WatchedDirectory>().ToList();
            foreach (var dir in directories)
            {
                Settings.DetectionDirectories.Remove(dir);
            }

            Settings.DetectionDirectories = Serialization.GetClone(Settings.DetectionDirectories);
        }
    }
}