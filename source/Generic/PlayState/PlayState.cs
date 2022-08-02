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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Win32;
using PluginsCommon;
using PlayniteUtilitiesCommon;
using System.Reflection;
using System.Windows.Media;
using StartPage.SDK;
using PlayState.Controls;

namespace PlayState
{
    public class PlayState : GenericPlugin, IStartPageExtension
    {

        private static readonly ILogger logger = LogManager.GetLogger();

        private Window mainWindow;
        private WindowInteropHelper windowInterop;
        private IntPtr mainWindowHandle;
        private HwndSource source = null;
        private bool globalHotkeyRegistered = false;

        private PlayStateSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("26375941-d460-4d32-925f-ad11e2facd8f");

        private PlayStateManagerViewModel playStateManager;
        private readonly string playstateIconImagePath;
        private MessagesHandler messagesHandler;
        private readonly bool isWindows10Or11;
        private const string featureBlacklist = "[PlayState] Blacklist";
        private const string featureSuspendPlaytime = "[PlayState] Suspend Playtime only";
        private const string featureSuspendProcesses = "[PlayState] Suspend Processes";

        public PlayState(IPlayniteAPI api) : base(api)
        {
            isWindows10Or11 = IsWindows10Or11();
            settings = new PlayStateSettingsViewModel(this, isWindows10Or11);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = "PlayState",
                SettingsRoot = $"{nameof(settings)}.{nameof(settings.Settings)}"
            });

            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                ElementList = new List<string> { "GameStateSwitchControl" },
                SourceName = "PlayState",
            });

            messagesHandler = new MessagesHandler(PlayniteApi, settings, isWindows10Or11);
            playStateManager = new PlayStateManagerViewModel(PlayniteApi, settings, messagesHandler);
            playstateIconImagePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "playstateIcon.png");
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == "GameStateSwitchControl")
            {
                return new GameStateSwitchControl(playStateManager, PlayniteApi, settings);
            }

            return null;
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
            if (settings.Settings.ShowManagerSidebarItem)
            {
                yield return new SidebarItem
                {
                    Title = ResourceProvider.GetString("LOCPlayState_PlayStateManagerViewHeaderLabel"),
                    Type = SiderbarItemType.View,
                    Icon = new TextBlock
                    {
                        Text = "\u0041",
                        FontFamily = new FontFamily(new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "playstateiconfont.ttf")), "./#playstateiconfont")
                    },
                    Opened = () => {
                        return new PlayStateManagerView { DataContext = playStateManager };
                    }
                };
            }
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
                messagesHandler.ShowGenericNotification("PlayState");
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
                                playStateManager.SwitchGameState(gameData);
                            }
                        }
                        else if (vkey == (uint)KeyInterop.VirtualKeyFromKey(settings.Settings.SavedInformationHotkeyGesture.Key))
                        {
                            var gameData = playStateManager.GetCurrentGameData();
                            if (gameData != null)
                            {
                                messagesHandler.ShowGameStatusNotification(NotificationTypes.Information, gameData);
                            }
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
            if (playStateManager.IsGameBeingDetected(args.Game))
            {
                return;
            }
            
            InitializePlaytimeInfoFile(); // Temporary workaround for sharing PlayState paused time until Playnite allows to share data among extensions
            var gameData = playStateManager.GetCurrentGameData();

            // Resume current game if manager is not enabled or Playnite is in Fullscreen Mode,
            // since otherwise it won't be possible to resume it
            if ((!settings.Settings.ShowManagerSidebarItem || PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen) && gameData != null && gameData.IsSuspended)
            {
                playStateManager.SwitchGameState(gameData);
            }

            var game = args.Game;
            if (PlayniteUtilities.GetGameHasFeature(game, featureBlacklist, true))
            {
                logger.Info($"{game.Name} is in PlayState blacklist. Extension execution stopped");
                return;
            }

            var suspendPlaytimeOnlyFeature = PlayniteUtilities.GetGameHasFeature(game, featureSuspendPlaytime, true);
            var suspendProcessesFeature = PlayniteUtilities.GetGameHasFeature(game, featureSuspendProcesses, true);
            if (!suspendProcessesFeature && settings.Settings.GlobalOnlySuspendPlaytime ||
                suspendPlaytimeOnlyFeature)
            {
                if (settings.Settings.UseForegroundAutomaticSuspend)
                {
                    InvokeGameProcessesDetection(args, SuspendModes.Playtime);
                }
                else
                {
                    playStateManager.AddPlayStateData(game, SuspendModes.Playtime, new List<ProcessItem> { });
                }

                return;
            }

            InvokeGameProcessesDetection(args, SuspendModes.Processes);
        }

        private async void InvokeGameProcessesDetection(OnGameStartedEventArgs args, SuspendModes suspendMode)
        {
            var game = args.Game;
            playStateManager.AddGameToDetection(game);
            var sourceActionHandled = ScanGameSourceAction(args.Game, args.SourceAction, suspendMode);
            if (sourceActionHandled)
            {
                return;
            }

            var gameInstallDir = GetGameInstallDir(args.Game);
            if (!gameInstallDir.IsNullOrEmpty())
            {
                var scanHandled = await ScanGameProcessesFromDirectoryAsync(game, gameInstallDir, suspendMode);
                if (scanHandled)
                {
                    return;
                }
            }

            // If no processes were found, playtime suspend mode can still be added as fallback.
            // Only missing functionality will be automatic suspending when game window is not in foreground
            if (suspendMode == SuspendModes.Playtime)
            {
                playStateManager.AddPlayStateData(game, SuspendModes.Playtime, new List<ProcessItem> { });
            }
        }

        private async Task<bool> ScanGameProcessesFromDirectoryAsync(Game game, string gameInstallDir, SuspendModes suspendMode)
        {
            // Fix for some games that take longer to start, even when already detected as running
            await Task.Delay(15000);
            if (!playStateManager.IsGameBeingDetected(game))
            {
                logger.Debug($"Detection Id was not detected. Execution of WMI Query task stopped.");
                return true;
            }

            var gameProcesses = ProcessesHandler.GetProcessesWmiQuery(true, gameInstallDir);
            if (gameProcesses.Count > 0)
            {
                logger.Debug($"Found {gameProcesses.Count} game processes in initial WMI query");
                playStateManager.AddPlayStateData(game, suspendMode, gameProcesses);
                return true;
            }

            // Waiting is useful for games that use a startup launcher, since
            // it can take some time before the user launches the game from it
            await Task.Delay(40000);
            var filterPaths = true;
            for (int i = 0; i < 7; i++)
            {
                // This is done to stop execution in case a new game was launched
                // or the launched game was closed
                if (!playStateManager.IsGameBeingDetected(game))
                {
                    logger.Debug($"Detection Id was not detected. Execution of WMI Query task stopped.");
                    return true;
                }

                // Try a few times with filters.
                // If nothing is found, try without filters. This helps in cases
                // where the active process is being filtered out by filters
                logger.Debug($"Starting WMI loop number {i}");
                if (i == 4)
                {
                    logger.Debug("FilterPaths set to false for WMI Query");
                    filterPaths = false;
                }

                gameProcesses = ProcessesHandler.GetProcessesWmiQuery(filterPaths, gameInstallDir);
                if (gameProcesses.Count > 0)
                {
                    logger.Debug($"Found {gameProcesses.Count} game processes");
                    playStateManager.AddPlayStateData(game, suspendMode, gameProcesses);
                    return true;
                }
                else
                {
                    await Task.Delay(15000);
                }
            }

            logger.Debug("Couldn't find any game process");
            return false;
        }

        private string GetGameInstallDir(Game game)
        {
            // Games from Xbox library are UWP apps. UWP apps run from the primary drive, e.g. "C:\".
            // If the game install location is not in the primary drive, Windows creates a symlink from the real files
            // to the primary drive and starts running the game from there. For this reason, we need to obtain the location
            // from where the game is running for Xbox game by detecting installed UWP apps as the Xbox plugin
            // reports the real installation directory in the games and not the fake one in C:\
            if (game.PluginId == Guid.Parse("7e4fbb5e-2ae3-48d4-8ba0-6b30e7a4e287"))
            {
                var gameInstallDir = Programs.GetUwpWorkdirFromGameId(game.GameId);
                if (gameInstallDir.IsNullOrEmpty() || !FileSystem.DirectoryExists(gameInstallDir))
                {
                    playStateManager.RemoveGameFromDetection(game);
                    return gameInstallDir;
                }
            }
            else if (!game.InstallDirectory.IsNullOrEmpty())
            {
                return game.InstallDirectory;
            }

            return null;
        }

        private bool ScanGameSourceAction(Game game, GameAction sourceAction, SuspendModes suspendMode)
        {
            if (sourceAction == null || sourceAction.Type != GameActionType.Emulator)
            {
                return false;
            }

            logger.Debug("Source action is emulator.");
            var emulatorProfileId = sourceAction.EmulatorProfileId;
            if (emulatorProfileId.StartsWith("#builtin_"))
            {
                //Currently it isn't possible to obtain the emulator path
                //for emulators using Builtin profiles
                logger.Debug("Source action was builtin emulator, which is not compatible. Execution stopped.");
                return true;
            }

            var emulator = PlayniteApi.Database.Emulators[sourceAction.EmulatorId];
            var profile = emulator?.CustomProfiles.FirstOrDefault(p => p.Id == emulatorProfileId);
            if (profile != null)
            {
                logger.Debug($"Custom emulator profile executable is {profile.Executable}");
                var gameProcesses = ProcessesHandler.GetProcessesWmiQuery(false, string.Empty, profile.Executable);
                if (gameProcesses.Count > 0)
                {
                    playStateManager.AddPlayStateData(game, suspendMode, gameProcesses);
                }
            }

            return true;
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            var game = args.Game;
            messagesHandler.HideWindow();

            playStateManager.RemoveGameFromDetection(game);
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
                        var featureAddedCount = PlayniteUtilities.AddFeatureToGames(PlayniteApi, PlayniteApi.MainView.SelectedGames.Distinct(), featureBlacklist);
                        PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCPlayState_BlacklistAddedResultsMessage"), featureAddedCount), "PlayState");
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCPlayState_MenuItemRemoveFromBlacklistDescription"),
                    MenuSection = "@PlayState",
                    Action = a => {
                        var featureRemovedCount = PlayniteUtilities.RemoveFeatureFromGames(PlayniteApi, PlayniteApi.MainView.SelectedGames.Distinct(), featureBlacklist);
                        PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCPlayState_BlacklistRemovedResultsMessage"), featureRemovedCount), "PlayState");
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCPlayState_MenuItemAddToPlaytimeSuspendDescription"),
                    MenuSection = "@PlayState",
                    Action = a => {
                        var featureAddedCount = PlayniteUtilities.AddFeatureToGames(PlayniteApi, PlayniteApi.MainView.SelectedGames.Distinct(), featureSuspendPlaytime);
                        PlayniteUtilities.RemoveFeatureFromGames(PlayniteApi, PlayniteApi.MainView.SelectedGames.Distinct(), featureSuspendProcesses);
                        PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCPlayState_PlaytimeSuspendAddedResultsMessage"), featureAddedCount), "PlayState");
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCPlayState_MenuItemRemoveFromPlaytimeSuspendDescription"),
                    MenuSection = "@PlayState",
                    Action = a => {
                        var featureRemovedCount = PlayniteUtilities.RemoveFeatureFromGames(PlayniteApi, PlayniteApi.MainView.SelectedGames.Distinct(), featureSuspendPlaytime);
                        PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCPlayState_PlaytimeSuspendRemovedResultsMessage"), featureRemovedCount), "PlayState");
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCPlayState_MenuItemAddToProcessesSuspendDescription"),
                    MenuSection = "@PlayState",
                    Action = a => {
                        var featureAddedCount = PlayniteUtilities.AddFeatureToGames(PlayniteApi, PlayniteApi.MainView.SelectedGames.Distinct(), featureSuspendProcesses);
                        PlayniteUtilities.RemoveFeatureFromGames(PlayniteApi, PlayniteApi.MainView.SelectedGames.Distinct(), featureSuspendPlaytime);
                        PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCPlayState_ProcessesSuspendAddedResultsMessage"), featureAddedCount), "PlayState");
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCPlayState_MenuItemRemoveFromProcessesSuspendDescription"),
                    MenuSection = "@PlayState",
                    Action = a => {
                        var featureRemovedCount = PlayniteUtilities.RemoveFeatureFromGames(PlayniteApi, PlayniteApi.MainView.SelectedGames.Distinct(), featureSuspendProcesses);
                        PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCPlayState_ProcessesSuspendRemovedResultsMessage"), featureRemovedCount), "PlayState");
                    }
                }
            };
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var game = args.Games.LastOrDefault();
            if (game == null)
            {
                return null;
            }

            var isGameSuspended = playStateManager.GetIsGameSuspended(game);
            if (isGameSuspended == null)
            {
                return null;
            }

            return new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = GetGameMenuSwitchStatusDescription(game, isGameSuspended),
                    Icon = playstateIconImagePath,
                    Action = a =>
                    {
                        playStateManager.SwitchGameState(game);
                    }
                }
            };
        }

        private string GetGameMenuSwitchStatusDescription(Game game, bool? isGameSuspended)
        {
            if (isGameSuspended == true)
            {
                return string.Format(ResourceProvider.GetString("LOCPlayState_GameMenuItemResumeGameDescription"), game.Name);
            }
            else if (isGameSuspended == false)
            {
                return string.Format(ResourceProvider.GetString("LOCPlayState_GameMenuItemSuspendGameDescription"), game.Name);
            }

            return string.Empty;
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new PlayStateSettingsView();
        }

        public StartPageExtensionArgs GetAvailableStartPageViews()
        {
            var views = new List<StartPageViewArgsBase> {
                new StartPageViewArgsBase
                {
                    ViewId = "PlayStateManager",
                    Name = ResourceProvider.GetString("LOCPlayState_PlayStateManagerViewHeaderLabel"),
                    Description = ResourceProvider.GetString("LOCPlayState_PlayStateManagerViewHeaderLabel")
                }
            };

            return new StartPageExtensionArgs() { ExtensionName = "PlayState", Views = views };
        }

        public object GetStartPageView(string viewId, Guid instanceId)
        {
            if (viewId == "PlayStateManager")
            {
                return new PlayStateManagerStartPageView() { DataContext = playStateManager };
            }

            return null;
        }

        public Control GetStartPageViewSettings(string viewId, Guid instanceId)
        {
            return null;
        }

        public void OnViewRemoved(string viewId, Guid instanceId)
        {
            
        }

    }
}