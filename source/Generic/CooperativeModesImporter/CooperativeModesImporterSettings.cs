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
        private int databaseVersion = 1;
        [DontSerialize]
        private bool addLinkOnImport = false;
        [DontSerialize]
        private string featuresPrefix = string.Empty;
        [DontSerialize]
        private bool addPrefix = false;
        [DontSerialize]
        private bool importBasicModes = true;
        [DontSerialize]
        private bool importDetailedModes = false;
        [DontSerialize]
        private bool addDetailedPrefix = false;
        [DontSerialize]
        private string featuresDetailedPrefix = string.Empty;
        [DontSerialize]
        private bool importDetailedModeLocal = false;
        [DontSerialize]
        private bool importDetailedModeOnline = false;
        [DontSerialize]
        private bool importDetailedModeCombo = false;
        [DontSerialize]
        private bool importDetailedModeLan = false;
        [DontSerialize]
        private bool importDetailedModeExtras = false;

        public int DatabaseVersion { get => databaseVersion; set => SetValue(ref databaseVersion, value); }
        public bool AddLinkOnImport { get => addLinkOnImport; set => SetValue(ref addLinkOnImport, value); }
        public string FeaturesPrefix { get => featuresPrefix; set => SetValue(ref featuresPrefix, value); }
        public bool AddPrefix { get => addPrefix; set => SetValue(ref addPrefix, value); }
        public bool ImportBasicModes { get => importBasicModes; set => SetValue(ref importBasicModes, value); }
        public bool ImportDetailedModes { get => importDetailedModes; set => SetValue(ref importDetailedModes, value); }
        public bool AddDetailedPrefix { get => addDetailedPrefix; set => SetValue(ref addDetailedPrefix, value); }
        public string FeaturesDetailedPrefix { get => featuresDetailedPrefix; set => SetValue(ref featuresDetailedPrefix, value); }
        public bool ImportDetailedModeLocal { get => importDetailedModeLocal; set => SetValue(ref importDetailedModeLocal, value); }
        public bool ImportDetailedModeOnline { get => importDetailedModeOnline; set => SetValue(ref importDetailedModeOnline, value); }
        public bool ImportDetailedModeCombo { get => importDetailedModeCombo; set => SetValue(ref importDetailedModeCombo, value); }
        public bool ImportDetailedModeLan { get => importDetailedModeLan; set => SetValue(ref importDetailedModeLan, value); }
        public bool ImportDetailedModeExtras { get => importDetailedModeExtras; set => SetValue(ref importDetailedModeExtras, value); }
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