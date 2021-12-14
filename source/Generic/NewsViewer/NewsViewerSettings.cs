using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsViewer
{
    public class NewsViewerSettings : ObservableObject
    {
        [DontSerialize]
        private bool reviewsAvailable = false;
        [DontSerialize]
        public bool ReviewsAvailable { get => reviewsAvailable; set => SetValue(ref reviewsAvailable, value); }
        [DontSerialize]
        private bool useCompactWebNewsViewer = true;
        public bool UseCompactWebNewsViewer { get => useCompactWebNewsViewer; set => SetValue(ref useCompactWebNewsViewer, value); }
        [DontSerialize]
        private bool showSteamNewsNonSteam = false;
        public bool ShowSteamNewsNonSteam { get => showSteamNewsNonSteam; set => SetValue(ref showSteamNewsNonSteam, value); }
        [DontSerialize]
        private double descriptionMaxHeight { get; set; } = 220;
        public double DescriptionMaxHeight
        {
            get => descriptionMaxHeight;
            set
            {
                descriptionMaxHeight = value;
                OnPropertyChanged();
            }
        }
    }

    public class NewsViewerSettingsViewModel : ObservableObject, ISettings
    {
        private readonly NewsViewer plugin;
        private NewsViewerSettings editingClone { get; set; }

        private NewsViewerSettings settings;
        public NewsViewerSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public NewsViewerSettingsViewModel(NewsViewer plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<NewsViewerSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new NewsViewerSettings();
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