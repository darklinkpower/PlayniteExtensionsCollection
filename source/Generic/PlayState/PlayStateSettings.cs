using Playnite.SDK;
using Playnite.SDK.Data;
using PlayState.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlayState
{
    public class PlayStateSettings : ObservableObject
    {
        [DontSerialize]
        public string hotkeyText = string.Empty;
        [DontSerialize]
        public string HotkeyText { get => hotkeyText; set => SetValue(ref hotkeyText, value); }
        public Hotkey SavedHotkeyGesture { get; set; } = new Hotkey(Key.A, (ModifierKeys)5);
        [DontSerialize]
        public bool substractSuspendedPlaytimeOnStopped = false;
        public bool SubstractSuspendedPlaytimeOnStopped { get => substractSuspendedPlaytimeOnStopped; set => SetValue(ref substractSuspendedPlaytimeOnStopped, value); }
        [DontSerialize]
        public bool substractOnlyNonLibraryGames = true;
        public bool SubstractOnlyNonLibraryGames { get => substractOnlyNonLibraryGames; set => SetValue(ref substractOnlyNonLibraryGames, value); }
    }

    public class PlayStateSettingsViewModel : ObservableObject, ISettings
    {
        private readonly PlayState plugin;
        private PlayStateSettings editingClone { get; set; }

        private PlayStateSettings settings;
        public PlayStateSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public PlayStateSettingsViewModel(PlayState plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<PlayStateSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new PlayStateSettings();
            }
            
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
            Settings.HotkeyText = $"{Settings.SavedHotkeyGesture.Modifiers} + {Settings.SavedHotkeyGesture.Key}";
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