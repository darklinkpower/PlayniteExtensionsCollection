using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamLauncherUtility
{
    public class SteamLauncherUtilitySettings : ISettings
    {
        private readonly SteamLauncherUtility plugin;

        public int LaunchMode { get; set; } = 0;
        public bool DisableSteamWebBrowserOnDesktopMode { get; set; } = true;
        public bool LaunchSteamBpmOnDesktopMode { get; set; } = false;
        public bool DisableSteamWebBrowserOnFullscreenMode { get; set; } = true;
        public bool LaunchSteamBpmOnFullscreenMode { get; set; } = false;
        public bool CloseSteamIfRunning { get; set; } = false;

        // Playnite serializes settings object to a JSON object and saves it as text file.
        // If you want to exclude some property from being saved then use `JsonIgnore` ignore attribute.
        [JsonIgnore]
        public bool OptionThatWontBeSaved { get; set; } = false;

        // Parameterless constructor must exist if you want to use LoadPluginSettings method.
        public SteamLauncherUtilitySettings()
        {
        }

        public SteamLauncherUtilitySettings(SteamLauncherUtility plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<SteamLauncherUtilitySettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                LaunchMode = savedSettings.LaunchMode;
                DisableSteamWebBrowserOnDesktopMode = savedSettings.DisableSteamWebBrowserOnDesktopMode;
                LaunchSteamBpmOnDesktopMode = savedSettings.LaunchSteamBpmOnDesktopMode;
                DisableSteamWebBrowserOnFullscreenMode = savedSettings.DisableSteamWebBrowserOnFullscreenMode;
                LaunchSteamBpmOnFullscreenMode = savedSettings.LaunchSteamBpmOnFullscreenMode;
                CloseSteamIfRunning = savedSettings.CloseSteamIfRunning;
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            plugin.SavePluginSettings(this);
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