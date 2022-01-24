using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CooperativeModesImporter
{
    public class CooperativeModesImporterSettings : ObservableObject
    {
        [DontSerialize]
        private string featuresPrefix = string.Empty;
        [DontSerialize]
        private bool addPrefix = false;
        public string FeaturesPrefix { get => featuresPrefix; set => SetValue(ref featuresPrefix, value); }
        public bool AddPrefix { get => addPrefix; set => SetValue(ref addPrefix, value); }
    }

    public class CooperativeModesImporterSettingsViewModel : ObservableObject, ISettings
    {
        private readonly CooperativeModesImporter plugin;
        private CooperativeModesImporterSettings editingClone { get; set; }

        private CooperativeModesImporterSettings settings;
        public CooperativeModesImporterSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public CooperativeModesImporterSettingsViewModel(CooperativeModesImporter plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<CooperativeModesImporterSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new CooperativeModesImporterSettings();
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