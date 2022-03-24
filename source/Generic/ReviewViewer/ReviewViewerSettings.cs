using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewViewer
{
    public class ReviewViewerSettings : ObservableObject
    {
        [DontSerialize]
        private bool useMatchingSteamApiLang { get; set; } = true;
        public bool UseMatchingSteamApiLang
        {
            get => useMatchingSteamApiLang;
            set
            {
                useMatchingSteamApiLang = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool downloadDataOnGameSelection { get; set; } = true;
        public bool DownloadDataOnGameSelection
        {
            get => downloadDataOnGameSelection;
            set
            {
                downloadDataOnGameSelection = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private double descriptionHeight { get; set; } = 180;
        public double DescriptionHeight
        {
            get => descriptionHeight;
            set
            {
                descriptionHeight = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool displayHelpfulnessData { get; set; } = true;
        public bool DisplayHelpfulnessData
        {
            get => displayHelpfulnessData;
            set
            {
                displayHelpfulnessData = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool displayReviewDate { get; set; } = true;
        public bool DisplayReviewDate
        {
            get => displayReviewDate;
            set
            {
                displayReviewDate = value;
                OnPropertyChanged();
            }
        }
    }

    public class ReviewViewerSettingsViewModel : ObservableObject, ISettings
    {
        private readonly ReviewViewer plugin;
        private ReviewViewerSettings editingClone { get; set; }

        private ReviewViewerSettings settings;
        public ReviewViewerSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public ReviewViewerSettingsViewModel(ReviewViewer plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<ReviewViewerSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new ReviewViewerSettings();
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