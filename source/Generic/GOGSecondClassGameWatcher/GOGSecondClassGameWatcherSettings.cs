using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOGSecondClassGameWatcher
{
    public class GOGSecondClassGameWatcherSettings : ObservableObject
    {
        private bool _isControlVisible = true;
        private bool _notifyMissingUpdates = true;
        private bool _notifyMissingLanguages = true;
        private bool _notifyMissingFreeDlc = true;
        private bool _notifyMissingPaidDlc = true;
        private bool _notifyMissingFeatures = true;
        private bool _notifyMissingSoundtrack = true;
        private bool _notifyOtherIssues = true;
        private bool _notifyMissingBuilds = true;
        private bool _notifyRegionLocking = true;

        private bool _notifyMissingAllAchievements = true;
        private bool _notifyMissingSomeAchievements = true;
        private bool _notifyBrokenAchievements = true;

        private bool _notifyOnGameInstalling = true;
        private bool _addStatusTagOnLibraryUpdate = true;

        public bool NotifyMissingUpdates { get => _notifyMissingUpdates; set => SetValue(ref _notifyMissingUpdates, value); }
        public bool NotifyMissingLanguages { get => _notifyMissingLanguages; set => SetValue(ref _notifyMissingLanguages, value); }
        public bool NotifyMissingFreeDlc { get => _notifyMissingFreeDlc; set => SetValue(ref _notifyMissingFreeDlc, value); }
        public bool NotifyMissingPaidDlc { get => _notifyMissingPaidDlc; set => SetValue(ref _notifyMissingPaidDlc, value); }
        public bool NotifyMissingFeatures { get => _notifyMissingFeatures; set => SetValue(ref _notifyMissingFeatures, value); }
        public bool NotifyMissingSoundtrack { get => _notifyMissingSoundtrack; set => SetValue(ref _notifyMissingSoundtrack, value); }
        public bool NotifyOtherIssues { get => _notifyOtherIssues; set => SetValue(ref _notifyOtherIssues, value); }
        public bool NotifyMissingBuilds { get => _notifyMissingBuilds; set => SetValue(ref _notifyMissingBuilds, value); }
        public bool NotifyRegionLocking { get => _notifyRegionLocking; set => SetValue(ref _notifyRegionLocking, value); }

        public bool NotifyMissingAllAchievements { get => _notifyMissingAllAchievements; set => SetValue(ref _notifyMissingAllAchievements, value); }
        public bool NotifyMissingSomeAchievements { get => _notifyMissingSomeAchievements; set => SetValue(ref _notifyMissingSomeAchievements, value); }
        public bool NotifyBrokenAchievements { get => _notifyBrokenAchievements; set => SetValue(ref _notifyBrokenAchievements, value); }

        public bool NotifyOnGameInstalling { get => _notifyOnGameInstalling; set => SetValue(ref _notifyOnGameInstalling, value); }
        public bool AddStatusTagOnLibraryUpdate { get => _addStatusTagOnLibraryUpdate; set => SetValue(ref _addStatusTagOnLibraryUpdate, value); }

        [DontSerialize]
        public bool IsControlVisible { get => _isControlVisible; set => SetValue(ref _isControlVisible, value); }
    }

    public class GOGSecondClassGameWatcherSettingsViewModel : ObservableObject, ISettings
    {
        private readonly GOGSecondClassGameWatcher plugin;
        private GOGSecondClassGameWatcherSettings editingClone { get; set; }

        private GOGSecondClassGameWatcherSettings settings;
        public GOGSecondClassGameWatcherSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public GOGSecondClassGameWatcherSettingsViewModel(GOGSecondClassGameWatcher plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<GOGSecondClassGameWatcherSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new GOGSecondClassGameWatcherSettings();
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
    }
}