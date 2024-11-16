using Playnite.SDK;
using Playnite.SDK.Data;
using SteamViewer.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamViewer
{
    public class SteamViewerSettings : ObservableObject
    {
        private bool _launchUrlsInSteamClient = true;
        public bool LaunchUrlsInSteamClient { get => _launchUrlsInSteamClient; set => SetValue(ref _launchUrlsInSteamClient, value); }
    }

    public class SteamViewerSettingsViewModel : ObservableObject, ISettings
    {
        private readonly SteamViewer _plugin;
        private readonly SteamUriLauncherService _steamUriLauncherService;
        private SteamViewerSettings _editingClone;
        private SteamViewerSettings _settings;
        public SteamViewerSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public SteamViewerSettingsViewModel(SteamViewer plugin, SteamUriLauncherService teamUriLauncherService)
        {
            _plugin = plugin;
            _steamUriLauncherService = teamUriLauncherService;
            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<SteamViewerSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new SteamViewerSettings();
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