using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThemesDetailsViewToGridViewConverter
{
    public class ThemesDetailsViewToGridViewConverterSettings : ObservableObject
    {
        private bool convertHelium = false;
        public bool ConvertHelium { get => convertHelium; set => SetValue(ref convertHelium, value); }
        private bool convertStardust = false;
        public bool ConvertStardust { get => convertStardust; set => SetValue(ref convertStardust, value); }
        private bool convertMythic = false;
        public bool ConvertMythic { get => convertMythic; set => SetValue(ref convertMythic, value); }
        private bool convertHarmony = false;
        public bool ConvertHarmony { get => convertHarmony; set => SetValue(ref convertHarmony, value); }
    }

    public class ThemesDetailsViewToGridViewConverterSettingsViewModel : ObservableObject, ISettings
    {
        private readonly ThemesDetailsViewToGridViewConverter plugin;
        private ThemesDetailsViewToGridViewConverterSettings editingClone { get; set; }

        private ThemesDetailsViewToGridViewConverterSettings settings;
        public ThemesDetailsViewToGridViewConverterSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public ThemesDetailsViewToGridViewConverterSettingsViewModel(ThemesDetailsViewToGridViewConverter plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<ThemesDetailsViewToGridViewConverterSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new ThemesDetailsViewToGridViewConverterSettings();
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
            if (Serialization.ToJson(editingClone) != Serialization.ToJson(Settings))
            {
                plugin.ProcessAllSupportedThemes();
            }
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