using Playnite.SDK;
using Playnite.SDK.Data;
using SteamShortcuts.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamShortcuts
{
    public class SteamShortcutsSettings : ObservableObject
    {
        private bool _launchUrlsInSteamClient = true;
        public bool LaunchUrlsInSteamClient { get => _launchUrlsInSteamClient; set => SetValue(ref _launchUrlsInSteamClient, value); }
    }

    public class SteamShortcutsSettingsViewModel : ObservableObject, ISettings
    {
        private readonly SteamShortcuts _plugin;
        private readonly SteamUriLauncherService _steamUriLauncherService;
        private SteamShortcutsSettings _editingClone;
        private SteamShortcutsSettings _settings;
        public SteamShortcutsSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public SteamShortcutsSettingsViewModel(SteamShortcuts plugin, SteamUriLauncherService teamUriLauncherService)
        {
            _plugin = plugin;
            _steamUriLauncherService = teamUriLauncherService;
            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<SteamShortcutsSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new SteamShortcutsSettings();
            }
        }

        public void BeginEdit()
        {
            _editingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            Settings = _editingClone;
        }

        public void EndEdit()
        {
            _plugin.SavePluginSettings(Settings);
            _steamUriLauncherService.LaunchUrlsInSteamClient = _settings.LaunchUrlsInSteamClient;
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}