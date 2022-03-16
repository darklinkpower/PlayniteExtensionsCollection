using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlayniteControlLocker
{
    public class PlayniteControlLockerSettings : ObservableObject
    {
        private bool desktopCheckCloseOnFail = true;
        public bool DesktopCheckCloseOnFail { get => desktopCheckCloseOnFail; set => SetValue(ref desktopCheckCloseOnFail, value); }
        private bool fullcreenCheckCloseOnFail = false;
        public bool FullcreenCheckCloseOnFail { get => fullcreenCheckCloseOnFail; set => SetValue(ref fullcreenCheckCloseOnFail, value); }

        private bool enableOnDesktopMode = true;
        public bool EnableOnDesktopMode { get => enableOnDesktopMode; set => SetValue(ref enableOnDesktopMode, value); }

        private bool enableOnFullscreenMode = true;
        public bool EnableOnFullscreenMode { get => enableOnFullscreenMode; set => SetValue(ref enableOnFullscreenMode, value); }

        private bool readModeAllowDeleteGames = false;
        public bool ReadModeAllowDeleteGames { get => readModeAllowDeleteGames; set => SetValue(ref readModeAllowDeleteGames, value); }

        private bool readModeAllowFavorites = true;
        public bool ReadModeAllowFavorites { get => readModeAllowFavorites; set => SetValue(ref readModeAllowFavorites, value); }
        
        private bool readModeAllowHiding = false;
        public bool ReadModeAllowHiding { get => readModeAllowHiding; set => SetValue(ref readModeAllowHiding, value); }

        private bool readModeOnlyUseFullscreenMode = false;

        public bool ReadModeOnlyUseFullscreenMode { get => readModeOnlyUseFullscreenMode; set => SetValue(ref readModeOnlyUseFullscreenMode, value); }
        
        public bool PassValue1 = true;
        public bool PassValue2 = true;
        public bool PassValue3 = true;
        public bool PassValue4 = true;
        public bool PassValue5 = true;
        public bool PassValue6 = true;

        public bool PasswordSet = false;

    }

    public class PlayniteControlLockerSettingsViewModel : ObservableObject, ISettings
    {
        private readonly IPlayniteAPI playniteApi;
        private bool settingsUnlocked = false;
        public bool SettingsUnlocked { get => settingsUnlocked; set => SetValue(ref settingsUnlocked, value); }
        private readonly PlayniteControlLocker plugin;
        private PlayniteControlLockerSettings editingClone { get; set; }

        private PlayniteControlLockerSettings settings;
        public PlayniteControlLockerSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public PlayniteControlLockerSettingsViewModel(PlayniteControlLocker plugin, IPlayniteAPI playniteApi)
        {
            this.playniteApi = playniteApi;
            
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<PlayniteControlLockerSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new PlayniteControlLockerSettings();
            }            
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
            if (!settings.PasswordSet)
            {
                SettingsUnlocked = true;
            }
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
            SettingsUnlocked = false;
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }

        public RelayCommand SetPasswordCommand
        {
            get => new RelayCommand(() =>
            {
                SetPassword();
            });
        }

        public RelayCommand UnlockSettings
        {
            get => new RelayCommand(() =>
            {
                var passwordIsCorrect = ValidatePassword();
                if (passwordIsCorrect)
                {
                    SettingsUnlocked = true;
                }
                else
                {
                    playniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOC_PlayniteControlLocker_DialogMessagePasswordIncorrect"));
                }
            }, () => !SettingsUnlocked);
        }

        public void SetPassword()
        {
            playniteApi.Dialogs.ShowMessage(
                ResourceProvider.GetString("LOC_PlayniteControlLocker_Settings_EnterPasswordMessage"),
                "Playnite Control Locker");

            var inputA1 = GetBoolFromYesNoDialog("1");
            var inputA2 = GetBoolFromYesNoDialog("2");
            var inputA3 = GetBoolFromYesNoDialog("3");
            var inputA4 = GetBoolFromYesNoDialog("4");
            var inputA5 = GetBoolFromYesNoDialog("5");
            var inputA6 = GetBoolFromYesNoDialog("6");

            playniteApi.Dialogs.ShowMessage(
                ResourceProvider.GetString("LOC_PlayniteControlLocker_Settings_RepeatPasswordMessage"),
                "Playnite Control Locker");

            var inputB1 = GetBoolFromYesNoDialog("1");
            var inputB2 = GetBoolFromYesNoDialog("2");
            var inputB3 = GetBoolFromYesNoDialog("3");
            var inputB4 = GetBoolFromYesNoDialog("4");
            var inputB5 = GetBoolFromYesNoDialog("5");
            var inputB6 = GetBoolFromYesNoDialog("6");

            if ((inputA1 == inputB1) &&
                (inputA2 == inputB2) &&
                (inputA3 == inputB3) &&
                (inputA4 == inputB4) &&
                (inputA5 == inputB5) &&
                (inputA6 == inputB6))
            {
                settings.PassValue1 = inputA1;
                settings.PassValue2 = inputA2;
                settings.PassValue3 = inputA3;
                settings.PassValue4 = inputA4;
                settings.PassValue5 = inputA5;
                settings.PassValue6 = inputA6;
                settings.PasswordSet = true;
                playniteApi.Dialogs.ShowMessage(
                    ResourceProvider.GetString("LOC_PlayniteControlLocker_Settings_PasswordSetSuccessMessage"),
                    "Playnite Control Locker");
            }
            else
            {
                playniteApi.Dialogs.ShowMessage(
                    ResourceProvider.GetString("LOC_PlayniteControlLocker_Settings_PasswordCombinationsNotSame"),
                    "Playnite Control Locker");
            }
        }

        public bool ValidatePassword()
        {
            var input1 = GetBoolFromYesNoDialog("1");
            var input2 = GetBoolFromYesNoDialog("2");
            var input3 = GetBoolFromYesNoDialog("3");
            var input4 = GetBoolFromYesNoDialog("4");
            var input5 = GetBoolFromYesNoDialog("5");
            var input6 = GetBoolFromYesNoDialog("6");

            return input1 == settings.PassValue1 &&
                input2 == settings.PassValue2 &&
                input3 == settings.PassValue3 &&
                input4 == settings.PassValue4 &&
                input5 == settings.PassValue5 &&
                input6 == settings.PassValue6;
        }

        private bool GetBoolFromYesNoDialog(string number)
        {
            var selection = playniteApi.Dialogs.ShowMessage(
                string.Format(ResourceProvider.GetString("LOC_PlayniteControlLocker_DialogMessageEnterPassword"), number),
                "Playnite Control Locker",
                MessageBoxButton.YesNo);
            return selection == MessageBoxResult.Yes;
        }
    }
}