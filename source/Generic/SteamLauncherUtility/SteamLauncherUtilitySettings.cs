using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamLauncherUtility
{
    public class SteamLauncherUtilitySettings
    {
        public int LaunchMode { get; set; } = 0;
        public bool DisableSteamWebBrowserOnDesktopMode { get; set; } = true;
        public bool LaunchSteamBpmOnDesktopMode { get; set; } = false;
        public bool DisableSteamWebBrowserOnFullscreenMode { get; set; } = true;
        public bool LaunchSteamBpmOnFullscreenMode { get; set; } = false;
        public bool CloseSteamIfRunning { get; set; } = false;
    }

    public class SteamLauncherUtilitySettingsViewModel : ObservableObject, ISettings
    {
        private readonly SteamLauncherUtility plugin;
        private SteamLauncherUtilitySettings editingClone { get; set; }

        private SteamLauncherUtilitySettings settings;
        public SteamLauncherUtilitySettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public SteamLauncherUtilitySettingsViewModel(SteamLauncherUtility plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<SteamLauncherUtilitySettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new SteamLauncherUtilitySettings();
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