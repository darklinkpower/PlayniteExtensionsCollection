using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallationStatusUpdater
{
    public class InstallationStatusUpdaterSettings
    {
        public bool UrlActionIsInstalled { get; set; } = true;
        public bool ScriptActionIsInstalled { get; set; } = true;
        public bool UseOnlyFirstRomDetection { get; set; } = false;
        public bool SkipHandledByPlugin { get; set; } = true;
        public bool OnlyUsePlayActionGameActions { get; set; } = false;
        public bool UpdateOnStartup { get; set; } = true;
        public bool UpdateOnLibraryUpdate { get; set; } = true;
        // Playnite serializes settings object to a JSON object and saves it as text file.
        // If you want to exclude some property from being saved then use `JsonDontSerialize` ignore attribute.
        [DontSerialize]
        public bool OptionThatWontBeSaved { get; set; } = false;
    }

    public class InstallationStatusUpdaterSettingsViewModel : ObservableObject, ISettings
    {
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

        public InstallationStatusUpdaterSettingsViewModel(InstallationStatusUpdater plugin)
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