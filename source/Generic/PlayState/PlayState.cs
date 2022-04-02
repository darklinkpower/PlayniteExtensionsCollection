using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayState.Enums;
using PlayState.Models;
using PlayState.ViewModels;
using PlayState.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using PluginsCommon;
using PlayniteUtilitiesCommon;
using System.Collections.ObjectModel;
using System.Reflection;

namespace PlayState
{
    public class PlayState : GenericPlugin
    {
        [DllImport("ntdll.dll", PreserveSig = false)]
        public static extern void NtSuspendProcess(IntPtr processHandle);
        [DllImport("ntdll.dll", PreserveSig = false)]
        public static extern void NtResumeProcess(IntPtr processHandle);
        private static readonly ILogger logger = LogManager.GetLogger();

        private Window mainWindow;
        private WindowInteropHelper windowInterop;
        private IntPtr mainWindowHandle;
        private HwndSource source = null;
        private bool globalHotkeyRegistered = false;

        private PlayStateSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("26375941-d460-4d32-925f-ad11e2facd8f");

        private PlayStateManagerViewModel playStateManager;
        private MessagesHandler messagesHandler;
        private readonly bool isWindows10Or11;

        public PlayState(IPlayniteAPI api) : base(api)
        {
            isWindows10Or11 = IsWindows10Or11();
            settings = new PlayStateSettingsViewModel(this, isWindows10Or11);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            playStateManager = new PlayStateManagerViewModel(PlayniteApi);
            messagesHandler = new MessagesHandler(PlayniteApi, settings, playStateManager, isWindows10Or11);
        }

        private bool IsWindows10Or11()
        {
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
            {
                var productName = key?.GetValue("ProductName")?.ToString() ?? string.Empty;
                return productName.Contains("Windows 10") || productName.Contains("Windows 11");
            }
        }

        public override IEnumerable<SidebarItem> GetSidebarItems()
        {
            yield return new SidebarItem
            {
                Title = "PlayState",
                Type = SiderbarItemType.View,
                Icon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "icon.png"),
                Opened = () => {
                    var view = new PlayStateManagerView();
                    view.DataContext = playStateManager;
                    return view;
                }
            };
        }

        // Hotkey implementation based on https://github.com/felixkmh/QuickSearch-for-Playnite
        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            mainWindow = Application.Current.MainWindow;
            if (mainWindow != null)
            {
                windowInterop = new WindowInteropHelper(mainWindow);
                mainWindowHandle = windowInterop.Handle;
                source = HwndSource.FromHwnd(mainWindowHandle);
                RegisterGlobalHotkey();
            }
            else
            {
                logger.Error("Could not find main window. Shortcuts could not be registered.");
            }

            if (!settings.Settings.WindowsNotificationStyleFirstSetupDone &&
                isWindows10Or11 &&
                settings.Settings.GlobalShowWindowsNotificationsStyle)
            {
                PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCPlayState_MessageWinStyleNotificationsFirstSetup"), "PlayState");
                settings.Settings.WindowsNotificationStyleFirstSetupDone = true;
                SavePluginSettings(settings.Settings);

                // A notification is shown so Playnite is added to the list
                // to add Playnite to the priority list
                new ToastContentBuilder()
                    .AddText("PlayState")
                    .Show();
                ProcessStarter.StartUrl(@"https://github.com/darklinkpower/PlayniteExtensionsCollection/wiki/PlayState#window-notification-style-configuration");
            }
        }

        internal bool RegisterGlobalHotkey()
        {
            if (!globalHotkeyRegistered)
            {
                var window = mainWindow;
                var handle = mainWindowHandle;
                source.AddHook(GlobalHotkeyCallback);
                globalHotkeyRegistered = true;

                // Pause/Resume Hotkey
                var registered = HotkeyHelper.RegisterHotKey(handle, HOTKEY_ID, settings.Settings.SavedHotkeyGesture.Modifiers.ToVK(), (uint)KeyInterop.VirtualKeyFromKey(settings.Settings.SavedHotkeyGesture.Key));

                if (registered)
                {
                    logger.Debug($"Pause/resume Hotkey registered with hotkey {settings.Settings.SavedHotkeyGesture}.");
                }
                else
                {
                    PlayniteApi.Notifications.Add(new NotificationMessage(Guid.NewGuid().ToString(),
                        "PlayState: " + string.Format(ResourceProvider.GetString("LOCPlayState_NotificationMessageHotkeyRegisterFailed"), settings.Settings.SavedHotkeyGesture),
                        NotificationType.Error));
                    logger.Error($"Failed to register configured pause/resume Hotkey {settings.Settings.SavedHotkeyGesture}.");
                }

                // Information Hotkey
                var registered2 = HotkeyHelper.RegisterHotKey(handle, HOTKEY_ID, settings.Settings.SavedInformationHotkeyGesture.Modifiers.ToVK(), (uint)KeyInterop.VirtualKeyFromKey(settings.Settings.SavedInformationHotkeyGesture.Key));
                
                if (registered2)
                {
                    logger.Debug($"Information Hotkey registered with hotkey {settings.Settings.SavedInformationHotkeyGesture}.");
                }
                else
                {
                    PlayniteApi.Notifications.Add(new NotificationMessage(Guid.NewGuid().ToString(),
                        "PlayState: " + string.Format(ResourceProvider.GetString("LOCPlayState_NotificationMessageHotkeyRegisterFailed"), settings.Settings.SavedInformationHotkeyGesture),
                        NotificationType.Error));
                    logger.Error($"Failed to register configured information Hotkey {settings.Settings.SavedInformationHotkeyGesture}.");
                }

                return registered && registered2;
            }

            return true;
        }

        private const int HOTKEY_ID = 3754;
        private const int WM_HOTKEY = 0x0312;
        private IntPtr GlobalHotkeyCallback(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                switch (wParam.ToInt32())
                {
                    case HOTKEY_ID:
                        uint vkey = ((uint)lParam >> 16) & 0xFFFF;
                        if (vkey == (uint)KeyInterop.VirtualKeyFromKey(settings.Settings.SavedHotkeyGesture.Key))
                        {
                            var gameData = playStateManager.GetCurrentGameData();
                            if (gameData != null)
                            {
                                SwitchGameState(gameData);
                            }
                        }
                        else if (vkey == (uint)KeyInterop.VirtualKeyFromKey(settings.Settings.SavedInformationHotkeyGesture.Key))
                        {
                            messagesHandler.ShowNotification(NotificationTypes.Information, playStateManager.CurrentGame);
                        }
                        handled = true;
                        break;
                    default:
                        break;
                }
            }

            return IntPtr.Zero;
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            InitializePlaytimeInfoFile(); // Temporary workaround for sharing PlayState paused time until Playnite allows to share data among extensions
            var gameData = playStateManager.GetCurrentGameData();
            if (gameData != null && gameData.IsSuspended)
            {
                SwitchGameState(gameData);
            }

            var game = args.Game;
            if (game.Features != null && game.Features.Any(a => a.Name.Equals("[PlayState] Blacklist", StringComparison.OrdinalIgnoreCase)))
            {
                logger.Info($"{game.Name} is in PlayState blacklist. Extension execution stopped");
                return;
            }

            List<ProcessItem> gameProcesses;

            var suspendPlaytimeOnlyFeature = game.Features != null ? game.Features.Any(a => a.Name.Equals("[PlayState] Suspend Playtime only", StringComparison.OrdinalIgnoreCase)) : false;
            var suspendProcessesFeature = game.Features != null ? game.Features.Any(a => a.Name.Equals("[PlayState] Suspend Processes", StringComparison.OrdinalIgnoreCase)) : false;
            if (settings.Settings.SubstractSuspendedPlaytimeOnStopped &&
                (settings.Settings.GlobalOnlySuspendPlaytime && !suspendProcessesFeature ||
                !settings.Settings.GlobalOnlySuspendPlaytime && suspendPlaytimeOnlyFeature))
            {
                playStateManager.CurrentGame = game;
                gameProcesses = null;
                playStateManager.AddPlayStateData(game, gameProcesses, true);
                return;
            }

            Task.Run(async () =>
            {
                playStateManager.CurrentGame = game;
                gameProcesses = null;
                logger.Debug($"Changed game to {playStateManager.CurrentGame.Name} game processes");
                var sourceAction = args.SourceAction;
                if (sourceAction?.Type == GameActionType.Emulator)
                {
                    logger.Debug("Source action is emulator.");
                    var emulatorProfileId = sourceAction.EmulatorProfileId;
                    if (emulatorProfileId.StartsWith("#builtin_"))
                    {
                        //Currently it isn't possible to obtain the emulator path
                        //for emulators using Builtin profiles
                        logger.Debug("Source action was builtin emulator, which is not compatible. Execution stopped.");
                        return;
                    }

                    var emulator = PlayniteApi.Database.Emulators[sourceAction.EmulatorId];
                    var profile = emulator?.CustomProfiles.FirstOrDefault(p => p.Id == emulatorProfileId);
                    if (profile != null)
                    {
                        logger.Debug($"Custom emulator profile executable is {profile.Executable}");
                        gameProcesses = ProcessesHandler.GetProcessesWmiQuery(false, string.Empty, profile.Executable.ToLower());
                        if (gameProcesses.Count > 0)
                        {
                            playStateManager.AddPlayStateData(game, gameProcesses);
                        }
                    }

                    return;
                }

                if (game.InstallDirectory.IsNullOrEmpty())
                {
                    return;
                }

                var gameInstallDir = game.InstallDirectory.ToLower();

                // Fix for some games that take longer to start, even when already detected as running
                await Task.Delay(15000);
                if (playStateManager.GetIsCurrentGameDifferent(game))
                {
                    return;
                }

                gameProcesses = ProcessesHandler.GetProcessesWmiQuery(true, gameInstallDir);
                if (gameProcesses.Count > 0)
                {
                    logger.Debug($"Found {gameProcesses.Count} game processes in initial WMI query");
                    playStateManager.AddPlayStateData(game, gameProcesses);
                    return;
                }

                // Waiting is useful for games that use a startup launcher, since
                // it can take some time before the user launches the game from it
                await Task.Delay(40000);
                var filterPaths = true;
                for (int i = 0; i < 10; i++)
                {
                    // This is done to stop execution in case a new game was launched
                    // or the launched game was closed
                    if (playStateManager.GetIsCurrentGameDifferent(game))
                    {
                        logger.Debug($"Current game has changed. Execution of WMI Query task stopped.");
                        return;
                    }

                    // Try a few times with filters.
                    // If nothing is found, try without filters. This helps in cases
                    // where the active process is being filtered out by filters
                    logger.Debug($"Starting WMI loop number {i}");
                    if (i == 5)
                    {
                        logger.Debug("FilterPaths set to false for WMI Query");
                        filterPaths = false;
                    }

                    gameProcesses = ProcessesHandler.GetProcessesWmiQuery(filterPaths, gameInstallDir);
                    if (gameProcesses.Count > 0)
                    {
                        logger.Debug($"Found {gameProcesses.Count} game processes");
                        playStateManager.AddPlayStateData(game, gameProcesses);
                        return;
                    }
                    else
                    {
                        await Task.Delay(15000);
                    }
                }

                logger.Debug("Couldn't find any game process");
            });
        }

        private void SwitchGameState(PlayStateData gameData)
        {
            try
            {
                gameData.ProcessesSuspended = false;
                if (gameData.GameProcesses != null && gameData.GameProcesses.Count > 0)
                {
                    foreach (var gameProcess in gameData.GameProcesses)
                    {
                        if (gameProcess == null || gameProcess.Process.Handle == null || gameProcess.Process.Handle == IntPtr.Zero)
                        {
                            return;
                        }
                        if (gameData.IsSuspended)
                        {
                            NtResumeProcess(gameProcess.Process.Handle);
                        }
                        else
                        {
                            NtSuspendProcess(gameProcess.Process.Handle);
                        }
                    }
                    gameData.ProcessesSuspended = true;
                }

                if (gameData.ProcessesSuspended || gameData.SuspendPlaytimeOnly)
                {
                    if (gameData.IsSuspended)
                    {
                        gameData.IsSuspended = false;
                        if (gameData.ProcessesSuspended)
                        {
                            messagesHandler.ShowNotification(NotificationTypes.Resumed, gameData.Game);
                        }
                        else
                        {
                            messagesHandler.ShowNotification(NotificationTypes.PlaytimeResumed, gameData.Game);
                        }
                        gameData.Stopwatch.Stop();
                        logger.Debug($"Game {gameData.Game.Name} resumed");
                    }
                    else
                    {
                        gameData.IsSuspended = true;
                        if (gameData.ProcessesSuspended)
                        {
                            messagesHandler.ShowNotification(NotificationTypes.Suspended, gameData.Game);
                        }
                        else
                        {
                            messagesHandler.ShowNotification(NotificationTypes.PlaytimeSuspended, gameData.Game);
                        }

                        gameData.Stopwatch.Start();
                        logger.Debug($"Game {gameData.Game.Name} suspended");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Error while suspending or resuming game");
                gameData.GameProcesses = null;
                gameData.Stopwatch.Stop();
            }
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            var game = args.Game;
            messagesHandler.HideWindow();

            var gameData = playStateManager.GetDataOfGame(game);
            if (gameData == null)
            {
                logger.Debug($"PlayState data for {game.Name} was not found on game stopped");
                return;
            }

            gameData.GameProcesses = null;
            if (settings.Settings.SubstractSuspendedPlaytimeOnStopped)
            {
                SubstractPlaytimeFromPlayStateData(game, gameData);
            }

            playStateManager.RemovePlayStateData(gameData);
        }

        private void SubstractPlaytimeFromPlayStateData(Game game, PlayStateData gameData)
        {
            if (game.PluginId != Guid.Empty && settings.Settings.SubstractOnlyNonLibraryGames)
            {
                return;
            }

            var suspendedTime = gameData.Stopwatch.Elapsed;
            if (suspendedTime == null)
            {
                logger.Debug($"PlayState data for {game.Name} had null suspendedTime");
                return;
            }

            var elapsedSeconds = Convert.ToUInt64(suspendedTime.TotalSeconds);
            logger.Debug($"Suspend elapsed seconds for game {game.Name} was {elapsedSeconds}");
            ExportPausedTimeInfo(game, elapsedSeconds); // Temporary workaround for sharing PlayState paused time until Playnite allows to share data among extensions
            if (elapsedSeconds != 0)
            {
                var newPlaytime = game.Playtime > elapsedSeconds ? game.Playtime - elapsedSeconds : elapsedSeconds - game.Playtime;
                logger.Debug($"Old playtime {game.Playtime}, new playtime {newPlaytime}");
                game.Playtime = newPlaytime;
                PlayniteApi.Database.Games.Update(game);
            }
        }

        private void InitializePlaytimeInfoFile() // Temporary workaround for sharing PlayState paused time until Playnite allows to share data among extensions
        {
            // This method will remove the info of the txt file in order to avoid reusing the previous play information.
            string[] info = { " ", " " };

            File.WriteAllLines(Path.Combine(PlayniteApi.Paths.ExtensionsDataPath, "PlayState.txt"), info);
        }

        private void ExportPausedTimeInfo(Game game, ulong elapsedSeconds) // Temporary workaround for sharing PlayState paused time until Playnite allows to share data among extensions
        {
            // This method will write the Id and pausedTime to PlayState.txt file placed inside ExtensionsData Roaming Playnite folder
            string[] info = { game.Id.ToString(), elapsedSeconds.ToString() };

            File.WriteAllLines(Path.Combine(PlayniteApi.Paths.ExtensionsDataPath, "PlayState.txt"), info);
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCPlayState_MenuItemAddToBlacklistDescription"),
                    MenuSection = "@PlayState",
                    Action = a => {
                        var featureAddedCount = PlayniteUtilities.AddFeatureToGames(PlayniteApi, PlayniteApi.MainView.SelectedGames.Distinct(), "[PlayState] Blacklist");
                        PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCPlayState_BlacklistAddedResultsMessage"), featureAddedCount), "PlayState");
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCPlayState_MenuItemRemoveFromBlacklistDescription"),
                    MenuSection = "@PlayState",
                    Action = a => {
                        var featureRemovedCount = PlayniteUtilities.RemoveFeatureFromGames(PlayniteApi, PlayniteApi.MainView.SelectedGames.Distinct(), "[PlayState] Blacklist");
                        PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCPlayState_BlacklistRemovedResultsMessage"), featureRemovedCount), "PlayState");
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCPlayState_MenuItemAddToPlaytimeSuspendDescription"),
                    MenuSection = "@PlayState",
                    Action = a => {
                        var featureAddedCount = PlayniteUtilities.AddFeatureToGames(PlayniteApi, PlayniteApi.MainView.SelectedGames.Distinct(), "[PlayState] Suspend Playtime only");
                        PlayniteUtilities.RemoveFeatureFromGames(PlayniteApi, PlayniteApi.MainView.SelectedGames.Distinct(), "[PlayState] Suspend Processes");
                        PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCPlayState_PlaytimeSuspendAddedResultsMessage"), featureAddedCount), "PlayState");
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCPlayState_MenuItemRemoveFromPlaytimeSuspendDescription"),
                    MenuSection = "@PlayState",
                    Action = a => {
                        var featureRemovedCount = PlayniteUtilities.RemoveFeatureFromGames(PlayniteApi, PlayniteApi.MainView.SelectedGames.Distinct(), "[PlayState] Suspend Playtime only");
                        PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCPlayState_PlaytimeSuspendRemovedResultsMessage"), featureRemovedCount), "PlayState");
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCPlayState_MenuItemAddToProcessesSuspendDescription"),
                    MenuSection = "@PlayState",
                    Action = a => {
                        var featureAddedCount = PlayniteUtilities.AddFeatureToGames(PlayniteApi, PlayniteApi.MainView.SelectedGames.Distinct(), "[PlayState] Suspend Processes");
                        PlayniteUtilities.RemoveFeatureFromGames(PlayniteApi, PlayniteApi.MainView.SelectedGames.Distinct(), "[PlayState] Suspend Playtime only");
                        PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCPlayState_ProcessesSuspendAddedResultsMessage"), featureAddedCount), "PlayState");
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCPlayState_MenuItemRemoveFromProcessesSuspendDescription"),
                    MenuSection = "@PlayState",
                    Action = a => {
                        var featureRemovedCount = PlayniteUtilities.RemoveFeatureFromGames(PlayniteApi, PlayniteApi.MainView.SelectedGames.Distinct(), "[PlayState] Suspend Processes");
                        PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCPlayState_ProcessesSuspendRemovedResultsMessage"), featureRemovedCount), "PlayState");
                    }
                }
            };
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new PlayStateSettingsView();
        }
    }
}