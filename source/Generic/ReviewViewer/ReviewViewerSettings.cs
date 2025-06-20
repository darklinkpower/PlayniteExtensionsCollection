using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReviewViewer.Domain;

namespace ReviewViewer
{
    public class ReviewViewerSettings : ObservableObject
    {
        public QueryOptions LastUsedQuery { get; set; } = new QueryOptions();
        public int DatabaseVersion { get; set; } = 1;

        private bool _downloadDataIfOlderThanDays = true;
        public bool DownloadDataIfOlderThanDays
        {
            get => _downloadDataIfOlderThanDays;
            set
            {
                _downloadDataIfOlderThanDays = value;
                OnPropertyChanged();
            }
        }

        private int _downloadIfOlderThanValue = 7;
        public int DownloadIfOlderThanValue
        {
            get => _downloadIfOlderThanValue;
            set
            {
                _downloadIfOlderThanValue = value;
                OnPropertyChanged();
            }
        }

        private double _descriptionHeight = 180;
        public double DescriptionHeight
        {
            get => _descriptionHeight;
            set
            {
                _descriptionHeight = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool isControlVisible = false;
        [DontSerialize]
        public bool IsControlVisible { get => isControlVisible; set => SetValue(ref isControlVisible, value); }
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