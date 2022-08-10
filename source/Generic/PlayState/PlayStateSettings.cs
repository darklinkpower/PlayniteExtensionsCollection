using Playnite.SDK;
using Playnite.SDK.Data;
using PlayState.Models;
using PlayState.XInputDotNetPure;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlayState
{
    public class PlayStateSettings : ObservableObject
    {
        [DontSerialize]
        private string hotkeyText = string.Empty;
        [DontSerialize]
        public string HotkeyText { get => hotkeyText; set => SetValue(ref hotkeyText, value); }

        [DontSerialize]
        private string informationHotkeyText = string.Empty;
        [DontSerialize]
        public string InformationHotkeyText { get => informationHotkeyText; set => SetValue(ref informationHotkeyText, value); }

        // Keyboard Hotkeys
        public Hotkey SavedHotkeyGesture { get; set; } = new Hotkey(Key.A, (ModifierKeys)5);
        public Hotkey SavedInformationHotkeyGesture { get; set; } = new Hotkey(Key.I, (ModifierKeys)5);



        // GamePad Hotkeys
        [DontSerialize]
        private GamePadStateHotkey gamePadSuspendHotkey;
        public GamePadStateHotkey GamePadSuspendHotkey { get => gamePadSuspendHotkey; set => SetValue(ref gamePadSuspendHotkey, value); }
        private bool gamePadSuspendHotkeyEnable = true;
        public bool GamePadSuspendHotkeyEnable { get => gamePadSuspendHotkeyEnable; set => SetValue(ref gamePadSuspendHotkeyEnable, value); }

        [DontSerialize]
        private GamePadStateHotkey gamePadInformationHotkey;
        public GamePadStateHotkey GamePadInformationHotkey { get => gamePadInformationHotkey; set => SetValue(ref gamePadInformationHotkey, value); }

        private bool gamePadInformationHotkeyEnable = true;
        public bool GamePadInformationHotkeyEnable { get => gamePadInformationHotkeyEnable; set => SetValue(ref gamePadInformationHotkeyEnable, value); }

        [DontSerialize]
        private GamePadStateHotkey gamePadCloseHotkey;
        public GamePadStateHotkey GamePadCloseHotkey { get => gamePadCloseHotkey; set => SetValue(ref gamePadCloseHotkey, value); }

        private bool gamePadCloseHotkeyEnable = true;
        public bool GamePadCloseHotkeyEnable { get => gamePadCloseHotkeyEnable; set => SetValue(ref gamePadCloseHotkeyEnable, value); }

        private bool gamePadHotkeysEnableAllControllers = false;
        public bool GamePadHotkeysEnableAllControllers { get => gamePadHotkeysEnableAllControllers; set => SetValue(ref gamePadHotkeysEnableAllControllers, value); }





        [DontSerialize]
        private bool substractSuspendedPlaytimeOnStopped = false;
        public bool SubstractSuspendedPlaytimeOnStopped { get => substractSuspendedPlaytimeOnStopped; set => SetValue(ref substractSuspendedPlaytimeOnStopped, value); }
        [DontSerialize]
        private bool substractOnlyNonLibraryGames = true;
        public bool SubstractOnlyNonLibraryGames { get => substractOnlyNonLibraryGames; set => SetValue(ref substractOnlyNonLibraryGames, value); }
        [DontSerialize]
        private bool globalOnlySuspendPlaytime = false;
        public bool GlobalOnlySuspendPlaytime { get => globalOnlySuspendPlaytime; set => SetValue(ref globalOnlySuspendPlaytime, value); }
        [DontSerialize]
        private bool globalShowWindowsNotificationsStyle = true;
        public bool GlobalShowWindowsNotificationsStyle { get => globalShowWindowsNotificationsStyle; set => SetValue(ref globalShowWindowsNotificationsStyle, value); }
        [DontSerialize]
        private bool notificationShowSessionPlaytime = true;
        public bool NotificationShowSessionPlaytime { get => notificationShowSessionPlaytime; set => SetValue(ref notificationShowSessionPlaytime, value); }
        [DontSerialize]
        private bool notificationShowTotalPlaytime = true;
        public bool NotificationShowTotalPlaytime { get => notificationShowTotalPlaytime; set => SetValue(ref notificationShowTotalPlaytime, value); }
        public bool WindowsNotificationStyleFirstSetupDone = false;

        [DontSerialize]
        private bool showManagerSidebarItem = true;
        public bool ShowManagerSidebarItem { get => showManagerSidebarItem; set => SetValue(ref showManagerSidebarItem, value); }
        [DontSerialize]
        private bool useForegroundAutomaticSuspend = false;
        public bool UseForegroundAutomaticSuspend { get => useForegroundAutomaticSuspend; set => SetValue(ref useForegroundAutomaticSuspend, value); }
        [DontSerialize]
        private bool bringResumedToForeground = false;
        public bool BringResumedToForeground { get => bringResumedToForeground; set => SetValue(ref bringResumedToForeground, value); }
        [DontSerialize]
        private bool enableNotificationMessages = true;
        public bool EnableNotificationMessages { get => enableNotificationMessages; set => SetValue(ref enableNotificationMessages, value); }

        private bool enableGameStateSwitchControl = true;
        public bool EnableGameStateSwitchControl { get => enableGameStateSwitchControl; set => SetValue(ref enableGameStateSwitchControl, value); }

        [DontSerialize]
        private bool isControlVisible = false;
        [DontSerialize]
        public bool IsControlVisible { get => isControlVisible; set => SetValue(ref isControlVisible, value); }
    }

    public class PlayStateSettingsViewModel : ObservableObject, ISettings
    {
        private readonly PlayState plugin;
        private PlayStateSettings editingClone { get; set; }

        private int countDownSeconds = 3;
        private bool isCountDownRunning = false;

        private string gamePadSuspendHotkeyCloneText = string.Empty;
        public string GamePadSuspendHotkeyCloneText { get => gamePadSuspendHotkeyCloneText; set => SetValue(ref gamePadSuspendHotkeyCloneText, value); }

        private string gamePadInformationHotkeyCloneText = string.Empty;
        public string GamePadInformationHotkeyCloneText { get => gamePadInformationHotkeyCloneText; set => SetValue(ref gamePadInformationHotkeyCloneText, value); }

        private string gamePadCloseHotkeyCloneText = string.Empty;
        public string GamePadCloseHotkeyCloneText { get => gamePadCloseHotkeyCloneText; set => SetValue(ref gamePadCloseHotkeyCloneText, value); }

        private string hotkeySaveCountDownText = string.Empty;
        public string HotkeySaveCountDownText { get => hotkeySaveCountDownText; set => SetValue(ref hotkeySaveCountDownText, value); }

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

        public bool IsWindows10Or11 { get; }

        // GamePad Hotkeys clones. They are cloned to prevent triggering them while saving them
        private GamePadStateHotkey gamePadSuspendHotkeyClone;
        public GamePadStateHotkey GamePadSuspendHotkeyClone
        {
            get => gamePadSuspendHotkeyClone;
            set
            {
                gamePadSuspendHotkeyClone = value;
                GamePadSuspendHotkeyCloneText = GetGamepadHotkeyText(gamePadSuspendHotkeyClone);
                OnPropertyChanged();
            }
        }

        private System.Timers.Timer countDownTimer = new System.Timers.Timer(2000) { AutoReset = true, Enabled = false };
        private GamePadStateHotkey gamePadInformationHotkeyClone;
        public GamePadStateHotkey GamePadInformationHotkeyClone
        {
            get => gamePadInformationHotkeyClone;
            set
            {
                gamePadInformationHotkeyClone = value;
                GamePadInformationHotkeyCloneText = GetGamepadHotkeyText(gamePadInformationHotkeyClone);
                OnPropertyChanged();
            }
        }

        private GamePadStateHotkey gamePadCloseHotkeyClone;
        private int gamepadHotKeyToUpdate = -1;

        public GamePadStateHotkey GamePadCloseHotkeyClone
        {
            get => gamePadCloseHotkeyClone;
            set
            {
                gamePadCloseHotkeyClone = value;
                GamePadCloseHotkeyCloneText = GetGamepadHotkeyText(gamePadCloseHotkeyClone);
                OnPropertyChanged();
            }
        }


        public PlayStateSettingsViewModel(PlayState plugin, bool isWindows10Or11)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<PlayStateSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
                if (Settings.SavedHotkeyGesture.Modifiers == ModifierKeys.None)
                {
                    // Due to a bug in previous version that allowed 
                    // to save gestures without modifiers, this
                    // should be done to restore the default ModifierKeys
                    Settings.SavedHotkeyGesture.Modifiers = (ModifierKeys)5;
                    plugin.SavePluginSettings(Settings);
                }
            }
            else
            {
                Settings = new PlayStateSettings();
            }

            IsWindows10Or11 = isWindows10Or11;
            countDownTimer.Elapsed += CountDownTimer_Elapsed;
            countDownSeconds = 3;
        }

        private void CountDownTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            countDownSeconds -= 1;
            UpdateCountdownText(countDownSeconds);
            if (countDownSeconds <= 0)
            {
                countDownTimer.Stop();
                PlayerIndex playerIndex = PlayerIndex.One;
                GamePadState gamePadState = GamePad.GetState(playerIndex);
                if (gamePadState.IsConnected && (gamePadState.Buttons.IsAnyPressed() || gamePadState.DPad.IsAnyPressed()))
                {
                    switch (gamepadHotKeyToUpdate)
                    {
                        case 0:
                            GamePadCloseHotkeyClone = new GamePadStateHotkey(gamePadState);
                            break;
                        case 1:
                            GamePadInformationHotkeyClone = new GamePadStateHotkey(gamePadState);
                            break;
                        case 2:
                            GamePadSuspendHotkeyClone = new GamePadStateHotkey(gamePadState);
                            break;
                        default:
                            break;
                    }
                }

                isCountDownRunning = false;
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
            Settings.HotkeyText = $"{Settings.SavedHotkeyGesture.Modifiers} + {Settings.SavedHotkeyGesture.Key}";
            Settings.InformationHotkeyText = $"{Settings.SavedInformationHotkeyGesture.Modifiers} + {Settings.SavedInformationHotkeyGesture.Key}";

            GamePadCloseHotkeyClone = Settings.GamePadCloseHotkey;
            GamePadInformationHotkeyClone = Settings.GamePadInformationHotkey;
            GamePadSuspendHotkeyClone = Settings.GamePadSuspendHotkey;
            HotkeySaveCountDownText = string.Empty;
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
            Settings.GamePadCloseHotkey = GamePadCloseHotkeyClone;
            Settings.GamePadInformationHotkey = GamePadInformationHotkeyClone;
            Settings.GamePadSuspendHotkey = GamePadSuspendHotkeyClone;
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

        private string GetGamepadHotkeyText(GamePadStateHotkey gamePadStateHotkey)
        {
            if (gamePadStateHotkey == null)
            {
                return string.Empty;
            }

            var lines = new List<string>();
            var pressedButtons = gamePadStateHotkey.Buttons.GetPressedButtonsList();
            var pressedDpadButtons = gamePadStateHotkey.DPad.GetPressedButtonsList();
            if (pressedButtons.HasItems())
            {
                lines.Add(string.Format(ResourceProvider.GetString("LOCPlayState_SettingsGamePadHotkeyButtonsLabel"), string.Join(", ", pressedButtons)));
            }

            if (pressedDpadButtons.HasItems())
            {
                lines.Add(string.Format(ResourceProvider.GetString("LOCPlayState_SettingsGamePadHotkeyDpadLabel"), string.Join(", ", pressedDpadButtons)));
            }

            return string.Join("\n", lines);
        }

        public RelayCommand<string> OpenLinkCommand
        {
            get => new RelayCommand<string>((a) =>
            {
                ProcessStarter.StartUrl(a);
            });
        }

        public RelayCommand SaveGamepadCloseHotkeyCommand
        {
            get => new RelayCommand(() =>
            {
                if (isCountDownRunning)
                {
                    return;
                }

                gamepadHotKeyToUpdate = 0;
                StartCountdownTimer();
            }, () => !isCountDownRunning);
        }

        public RelayCommand SaveGamepadInformationHotkeyCommand
        {
            get => new RelayCommand(() =>
            {
                if (isCountDownRunning)
                {
                    return;
                }

                gamepadHotKeyToUpdate = 1;
                StartCountdownTimer();
            }, () => !isCountDownRunning);
        }

        public RelayCommand SaveGamepadSuspendHotkeyCommand
        {
            get => new RelayCommand(() =>
            {
                if (isCountDownRunning)
                {
                    return;
                }

                gamepadHotKeyToUpdate = 2;
                StartCountdownTimer();
            }, () => !isCountDownRunning);
        }

        private void StartCountdownTimer()
        {
            isCountDownRunning = true;
            countDownSeconds = 3;
            UpdateCountdownText(countDownSeconds);
            countDownTimer.Stop();
            countDownTimer.Start();
        }
        
        private void UpdateCountdownText(int secondsLeft)
        {
            if (secondsLeft > 0)
            {
                HotkeySaveCountDownText = string.Format(ResourceProvider.GetString("LOCPlayState_SettingsGamePadHotkeyUpdateCountdown"), secondsLeft);
            }
            else
            {
                HotkeySaveCountDownText = string.Empty;
            }
        }

    }
}