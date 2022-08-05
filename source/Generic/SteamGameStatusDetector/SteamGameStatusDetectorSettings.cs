using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamGameStatusDetector
{
    public class SteamGameStatusDetectorSettings : ObservableObject
    {
        private string appState = string.Empty;
        private long bytesDownloaded = 0;
        private long bytesToDownload = 0;
        private string downloadProgress = string.Empty;
        private bool hasData = false;

        [DontSerialize]
        public string AppState { get => appState; set => SetValue(ref appState, value); }
        [DontSerialize]
        public long BytesDownloaded { get => bytesDownloaded; set => SetValue(ref bytesDownloaded, value); }
        [DontSerialize]
        public long BytesToDownload { get => bytesToDownload; set => SetValue(ref bytesToDownload, value); }

        [DontSerialize]
        public string DownloadProgress { get => downloadProgress; set => SetValue(ref downloadProgress, value); }
        [DontSerialize]
        public bool HasData { get => hasData; set => SetValue(ref hasData, value); }
    }

    public class SteamGameStatusDetectorSettingsViewModel : ObservableObject, ISettings
    {
        private readonly SteamGameStatusDetector plugin;
        private SteamGameStatusDetectorSettings editingClone { get; set; }

        private SteamGameStatusDetectorSettings settings;
        public SteamGameStatusDetectorSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public SteamGameStatusDetectorSettingsViewModel(SteamGameStatusDetector plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<SteamGameStatusDetectorSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new SteamGameStatusDetectorSettings();
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