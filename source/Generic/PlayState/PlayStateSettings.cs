using Playnite.SDK;
using Playnite.SDK.Data;
using PlayState.Enums;
using PlayState.Models;
using PlayState.XInputDotNetPure;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        // GamePad to Keyboard hotkey
        private ObservableCollection<GamePadHotkeyCombo> gamePadToHotkeyCollection = new ObservableCollection<GamePadHotkeyCombo>();
        public ObservableCollection<GamePadHotkeyCombo> GamePadToHotkeyCollection { get => gamePadToHotkeyCollection; set => SetValue(ref gamePadToHotkeyCollection, value); }

        // Keyboard Hotkeys
        private HotKey suspendHotKey = new HotKey(Key.A, ModifierKeys.Shift | ModifierKeys.Alt);
        public HotKey SuspendHotKey { get => suspendHotKey; set => SetValue(ref suspendHotKey, value); }
        private HotKey informationHotkey = new HotKey(Key.I, ModifierKeys.Shift | ModifierKeys.Alt);
        public HotKey InformationHotkey { get => informationHotkey; set => SetValue(ref informationHotkey, value); }

        // GamePad Hotkeys
        private GamePadStateHotkey gamePadSuspendHotkey;
        public GamePadStateHotkey GamePadSuspendHotkey { get => gamePadSuspendHotkey; set => SetValue(ref gamePadSuspendHotkey, value); }
        private bool gamePadSuspendHotkeyEnable = true;
        public bool GamePadSuspendHotkeyEnable { get => gamePadSuspendHotkeyEnable; set => SetValue(ref gamePadSuspendHotkeyEnable, value); }
        private GamePadStateHotkey gamePadInformationHotkey;
        public GamePadStateHotkey GamePadInformationHotkey { get => gamePadInformationHotkey; set => SetValue(ref gamePadInformationHotkey, value); }
        private bool gamePadInformationHotkeyEnable = true;
        public bool GamePadInformationHotkeyEnable { get => gamePadInformationHotkeyEnable; set => SetValue(ref gamePadInformationHotkeyEnable, value); }

        // GamePad Hotkey Options
        private bool gamePadHotkeysEnableAllControllers = false;
        public bool GamePadHotkeysEnableAllControllers { get => gamePadHotkeysEnableAllControllers; set => SetValue(ref gamePadHotkeysEnableAllControllers, value); }

        // Other Settings
        private bool substractSuspendedPlaytimeOnStopped = false;
        public bool SubstractSuspendedPlaytimeOnStopped { get => substractSuspendedPlaytimeOnStopped; set => SetValue(ref substractSuspendedPlaytimeOnStopped, value); }
        private bool substractOnlyNonLibraryGames = true;
        public bool SubstractOnlyNonLibraryGames { get => substractOnlyNonLibraryGames; set => SetValue(ref substractOnlyNonLibraryGames, value); }

        private bool notificationShowSessionPlaytime = true;
        public bool NotificationShowSessionPlaytime { get => notificationShowSessionPlaytime; set => SetValue(ref notificationShowSessionPlaytime, value); }
        private bool notificationShowTotalPlaytime = true;
        public bool NotificationShowTotalPlaytime { get => notificationShowTotalPlaytime; set => SetValue(ref notificationShowTotalPlaytime, value); }

        private bool windowsNotificationStyleFirstSetupDone = false;
        public bool WindowsNotificationStyleFirstSetupDone { get => windowsNotificationStyleFirstSetupDone; set => SetValue(ref windowsNotificationStyleFirstSetupDone, value); }

        private bool showManagerSidebarItem = true;
        public bool ShowManagerSidebarItem { get => showManagerSidebarItem; set => SetValue(ref showManagerSidebarItem, value); }
        private bool useForegroundAutomaticSuspend = false;
        public bool UseForegroundAutomaticSuspend { get => useForegroundAutomaticSuspend; set => SetValue(ref useForegroundAutomaticSuspend, value); }
        private bool useForegroundAutomaticSuspendPlaytimeMode = false;
        public bool UseForegroundAutomaticSuspendPlaytimeMode { get => useForegroundAutomaticSuspendPlaytimeMode; set => SetValue(ref useForegroundAutomaticSuspendPlaytimeMode, value); }
        private bool bringResumedToForeground = false;
        public bool BringResumedToForeground { get => bringResumedToForeground; set => SetValue(ref bringResumedToForeground, value); }

        private bool showNotificationOnGameStatusChange = true;
        public bool ShowNotificationOnGameStatusChange { get => showNotificationOnGameStatusChange; set => SetValue(ref showNotificationOnGameStatusChange, value); }
        private bool showNotificationOnGameAdded = true;
        public bool ShowNotificationOnGameAdded { get => showNotificationOnGameAdded; set => SetValue(ref showNotificationOnGameAdded, value); }

        private bool enableGameStateSwitchControl = true;
        public bool EnableGameStateSwitchControl { get => enableGameStateSwitchControl; set => SetValue(ref enableGameStateSwitchControl, value); }

        private bool isControlVisible = false;
        [DontSerialize]
        public bool IsControlVisible { get => isControlVisible; set => SetValue(ref isControlVisible, value); }

        private SuspendModes globalSuspendMode = SuspendModes.Processes;
        public SuspendModes GlobalSuspendMode { get => globalSuspendMode; set => SetValue(ref globalSuspendMode, value); }
        private NotificationStyles notificationStyle = NotificationStyles.Toast;
        public NotificationStyles NotificationStyle { get => notificationStyle; set => SetValue(ref notificationStyle, value); }

        private bool switchToDesktopModeOnControllerStatus = false;
        public bool SwitchToDesktopModeOnControllerStatus { get => switchToDesktopModeOnControllerStatus; set => SetValue(ref switchToDesktopModeOnControllerStatus, value); }
        private bool switchToFullscreenModeOnControllerStatus = false;
        public bool SwitchToFullscreenModeOnControllerStatus { get => switchToFullscreenModeOnControllerStatus; set => SetValue(ref switchToFullscreenModeOnControllerStatus, value); }
        private bool switchModesOnlyIfNoRunningGames = true;
        public bool SwitchModesOnlyIfNoRunningGames { get => switchModesOnlyIfNoRunningGames; set => SetValue(ref switchModesOnlyIfNoRunningGames, value); }
        private int switchModeIgnoreCtrlStateOnStartupSeconds = 20;
        public int SwitchModeIgnoreCtrlStateOnStartupSeconds { get => switchModeIgnoreCtrlStateOnStartupSeconds; set => SetValue(ref switchModeIgnoreCtrlStateOnStartupSeconds, value); }

        private bool enableControllersHotkeys = true;
        public bool EnableControllersHotkeys { get => enableControllersHotkeys; set => SetValue(ref enableControllersHotkeys, value); }

        private bool excludeShortPlaytimeSessions = false;
        public bool ExcludeShortPlaytimeSessions { get => excludeShortPlaytimeSessions; set => SetValue(ref excludeShortPlaytimeSessions, value); }
        private uint minimumPlaytimeThreshold = 5;
        public uint MinimumPlaytimeThreshold { get => minimumPlaytimeThreshold; set => SetValue(ref minimumPlaytimeThreshold, value); }
        private List<string> executablesScanExclusionList;
        public List<string> ExecutablesScanExclusionList { get => executablesScanExclusionList; set => SetValue(ref executablesScanExclusionList, value); }
    }

    public class PlayStateSettingsViewModel : ObservableObject, ISettings
    {
        private readonly List<string> _defaultExclusionList = new List<string>
        {
            "7z.exe",
            "7za.exe",
            "archive.exe",
            "asset_.exe",
            "anetdrop.exe",
            "bat_to_exe_convertor.exe",
            "bssndrpt.exe",
            "bootboost.exe",
            "bootstrap.exe",
            "cabarc.exe",
            "cdkey.exe",
            "cheat engine.exe",
            "cheatengine",
            "civ2map.exe",
            "config",
            "closepw.exe",
            "crashdump",
            "crashreport",
            "crc32.exe",
            "creationkit.exe",
            "creatureupload.exe",
            "easyhook.exe",
            "dgvoodoocpl.exe",
            "dotnet",
            "doc.exe",
            "dxsetup",
            "dw.exe",
            "enbinjector.exe",
            "havokbehaviorpostprocess.exe",
            "help",
            "install",
            "launch_game.exe",
            "langselect.exe",
            "language.exe",
            "launch",
            "loader",
            "mapcreator.exe",
            "master_dat_fix_up.exe",
            "md5sum.exe",
            "mgexegui.exe",
            "modman.exe",
            "modorganizer.exe",
            "notepad++.exe",
            "notification_helper.exe",
            "oalinst.exe",
            "palettestealersuspender.exe",
            "pak",
            "patch",
            "planet_mapgen.exe",
            "papyrus",
            "radtools.exe",
            "readspr.exe",
            "register.exe",
            "sekirofpsunlocker",
            "settings",
            "setup",
            "scuex64.exe",
            "synchronicity.exe",
            "syscheck.exe",
            "systemsurvey.exe",
            "tes construction set.exe",
            "texmod.exe",
            "unins",
            "unitycrashhandler",
            "x360ce",
            "unpack",
            "unx_calibrate",
            "update",
            "unrealcefsubprocess.exe",
            "url.exe",
            "versioned_json.exe",
            "vcredist",
            "xtexconv.exe",
            "xwmaencode.exe",
            "website.exe",
            "wide_on.exe"
        };

        private readonly PlayState plugin;

        // Settings
        private PlayStateSettings editingClone { get; set; }
        private string _editingExclusionList = string.Empty;
        public string EditingExclusionList { get => _editingExclusionList; set => SetValue(ref _editingExclusionList, value); }

        // Countdown
        private int countDownSeconds = 3;
        private bool isCountDownRunning = false;
        public bool IsCountDownRunning { get => isCountDownRunning; set => SetValue(ref isCountDownRunning, value); }
        private System.Timers.Timer countDownTimer = new System.Timers.Timer(1000) { AutoReset = true, Enabled = false };

        // Hotkey
        private int gamepadHotKeyToUpdate = -1;
        private string hotkeySaveCountDownText = string.Empty;
        public string HotkeySaveCountDownText { get => hotkeySaveCountDownText; set => SetValue(ref hotkeySaveCountDownText, value); }

        // Keyboard Hotkey
        private HotKey comboHotkeyKeyboard;
        public HotKey ComboHotkeyKeyboard { get => comboHotkeyKeyboard; set => SetValue(ref comboHotkeyKeyboard, value); }
        private ObservableCollection<HotKey> defaultComboKeyboardHotkeys;
        public ObservableCollection<HotKey> DefaultComboKeyboardHotkeys { get => defaultComboKeyboardHotkeys; set => SetValue(ref defaultComboKeyboardHotkeys, value); }
        private HotKey selectedDefaultComboKeyboardHotkey;
        public HotKey SelectedDefaultComboKeyboardHotkey { get => selectedDefaultComboKeyboardHotkey; set => SetValue(ref selectedDefaultComboKeyboardHotkey, value); }

        // Gamepad Hotkey
        private GamePadHotkeyCombo selectedComboHotkey;
        public GamePadHotkeyCombo SelectedComboHotkey { get => selectedComboHotkey; set => SetValue(ref selectedComboHotkey, value); }
        private GamePadStateHotkey comboHotkeyGamePad;
        public GamePadStateHotkey ComboHotkeyGamePad { get => comboHotkeyGamePad; set => SetValue(ref comboHotkeyGamePad, value); }
        private GamePadToKeyboardHotkeyModes selectedGpdToKbHotkeyMode = GamePadToKeyboardHotkeyModes.Always;
        public GamePadToKeyboardHotkeyModes SelectedGpdToKbHotkeyMode { get => selectedGpdToKbHotkeyMode; set => SetValue(ref selectedGpdToKbHotkeyMode, value); }



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
                OnPropertyChanged();
            }
        }
        
        private GamePadStateHotkey gamePadInformationHotkeyClone;
        public GamePadStateHotkey GamePadInformationHotkeyClone
        {
            get => gamePadInformationHotkeyClone;
            set
            {
                gamePadInformationHotkeyClone = value;
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
            }
            else
            {
                Settings = new PlayStateSettings();
            }

            if (Settings.ExecutablesScanExclusionList is null)
            {
                Settings.ExecutablesScanExclusionList = _defaultExclusionList;
            }

            IsWindows10Or11 = isWindows10Or11;
            countDownTimer.Elapsed += CountDownTimer_Elapsed;
            countDownSeconds = 3;

            DefaultComboKeyboardHotkeys = new ObservableCollection<HotKey>
            {
                new HotKey(Key.F4, ModifierKeys.Alt),
                new HotKey(Key.Escape, ModifierKeys.Alt),
                new HotKey(Key.Tab, ModifierKeys.Alt),
                new HotKey(Key.Tab, ModifierKeys.Alt | ModifierKeys.Shift),
                new HotKey(Key.Tab, ModifierKeys.Control | ModifierKeys.Alt),
                new HotKey(Key.Tab, ModifierKeys.Windows),
                new HotKey(Key.D, ModifierKeys.Windows),
                new HotKey(Key.M, ModifierKeys.Windows),
                new HotKey(Key.MediaPlayPause, ModifierKeys.None),
                new HotKey(Key.MediaPreviousTrack, ModifierKeys.None),
                new HotKey(Key.MediaNextTrack, ModifierKeys.None),
                new HotKey(Key.VolumeUp, ModifierKeys.None),
                new HotKey(Key.VolumeDown, ModifierKeys.None),
                new HotKey(Key.VolumeMute, ModifierKeys.None),
                new HotKey(Key.PrintScreen, ModifierKeys.Windows | ModifierKeys.Alt)
            };

            SelectedDefaultComboKeyboardHotkey = DefaultComboKeyboardHotkeys.FirstOrDefault();
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);

            GamePadInformationHotkeyClone = Settings.GamePadInformationHotkey;
            GamePadSuspendHotkeyClone = Settings.GamePadSuspendHotkey;
            HotkeySaveCountDownText = string.Empty;
            EditingExclusionList = string.Join("\n", Settings.ExecutablesScanExclusionList);
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
            Settings.GamePadInformationHotkey = GamePadInformationHotkeyClone;
            Settings.GamePadSuspendHotkey = GamePadSuspendHotkeyClone;
            Settings.ExecutablesScanExclusionList = EditingExclusionList.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
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
                            ComboHotkeyGamePad = new GamePadStateHotkey(gamePadState);
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

                IsCountDownRunning = false;
                OnPropertyChanged(nameof(IsCountDownRunning));
            }
        }

        public static ObservableCollection<GamePadHotkeyCombo> OrderCollection(ObservableCollection<GamePadHotkeyCombo> collectionToSort)
        {
            var tempCollection = new ObservableCollection<GamePadHotkeyCombo>(collectionToSort.OrderBy(p => p.KeyboardHotkey.ToString()));
            collectionToSort.Clear();
            foreach (var gamepadHotkey in tempCollection)
            {
                collectionToSort.Add(gamepadHotkey);
            }

            return collectionToSort;
        }

        private void StartCountdownTimer()
        {
            IsCountDownRunning = true;
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

        internal void ResetExecutablesScanExclusionList()
        {
            EditingExclusionList = string.Join("\n", _defaultExclusionList);
        }

        public RelayCommand<string> OpenLinkCommand
        {
            get => new RelayCommand<string>((a) =>
            {
                ProcessStarter.StartUrl(a);
            });
        }

        public RelayCommand SaveGamepadToKeyboardHotkeyCommand
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

        public RelayCommand AddComboHotkeyCommand
        {
            get => new RelayCommand(() =>
            {
                if (comboHotkeyKeyboard is null || ComboHotkeyGamePad is null)
                {
                    return;
                }

                var newComboSer = Serialization.ToJson(ComboHotkeyGamePad);
                var isComboAlreadyInUse = Serialization.ToJson(gamePadSuspendHotkeyClone) == newComboSer ||
                        Serialization.ToJson(gamePadInformationHotkeyClone) == newComboSer ||
                        Settings.GamePadToHotkeyCollection.Any(x => SelectedGpdToKbHotkeyMode != x.Mode && Serialization.ToJson(x.GamePadHotKey) == newComboSer);
                if (isComboAlreadyInUse)
                {
                    plugin.PlayniteApi.Dialogs.ShowErrorMessage(ResourceProvider.GetString("LOCPlayState_SettingsDuplicateGamepadHotkeyMessage"), "PlayState");
                    return;
                }

                var newCombo = new GamePadHotkeyCombo(ComboHotkeyKeyboard, ComboHotkeyGamePad, SelectedGpdToKbHotkeyMode);
                Settings.GamePadToHotkeyCollection.Add(newCombo);
                Settings.GamePadToHotkeyCollection = OrderCollection(Settings.GamePadToHotkeyCollection);
            }, () => !isCountDownRunning);
        }

        public RelayCommand SetSelectedDefaultHotkeyCommand
        {
            get => new RelayCommand(() =>
            {
                if (SelectedDefaultComboKeyboardHotkey is null)
                {
                    return;
                }

                ComboHotkeyKeyboard = SelectedDefaultComboKeyboardHotkey;
            });
        }

        public RelayCommand RemoveSelectedComboHotkeyCommand
        {
            get => new RelayCommand(() =>
            {
                if (SelectedComboHotkey is null)
                {
                    return;
                }

                Settings.GamePadToHotkeyCollection.Remove(SelectedComboHotkey);
            });
        }

        public RelayCommand ResetExecutablesScanExclusionListCommand
        {
            get => new RelayCommand(() =>
            {
                ResetExecutablesScanExclusionList();
            });
        }
    }
}