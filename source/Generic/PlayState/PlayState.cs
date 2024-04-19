using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
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
using System.Windows.Interop;
using PluginsCommon;
using PlayniteUtilitiesCommon;
using System.Reflection;
using System.Windows.Media;
using StartPage.SDK;
using PlayState.Controls;
using System.Timers;
using PlayState.Enums;

namespace PlayState
{
    public partial class PlayState : GenericPlugin, IStartPageExtension
    {

        private static readonly ILogger logger = LogManager.GetLogger();

        private IntPtr mainWindowHandle;
        private bool globalHotkeyRegistered = false;
        private bool _isAnyGameRunning = false;
        private bool switchPlayniteModeStarted = false;
        public PlayStateSettingsViewModel Settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("26375941-d460-4d32-925f-ad11e2facd8f");

        private readonly PlayStateManagerViewModel playStateManager;
        private readonly string playstateIconImagePath;
        private readonly GamePadHandler _gamePadHandler;
        private readonly MessagesHandler messagesHandler;
        private readonly bool isWindows10Or11;
        private const string featureBlacklist = "[PlayState] Blacklist";
        private const string featureSuspendPlaytime = "[PlayState] Suspend Playtime only";
        private const string featureSuspendProcesses = "[PlayState] Suspend Processes";

        public PlayState(IPlayniteAPI api) : base(api)
        {
            isWindows10Or11 = IsWindows10Or11();
            Settings = new PlayStateSettingsViewModel(this, isWindows10Or11);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = "PlayState",
                SettingsRoot = $"{nameof(Settings)}.{nameof(Settings.Settings)}"
            });

            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                ElementList = new List<string> { "GameStateSwitchControl" },
                SourceName = "PlayState",
            });

            playStateManager = new PlayStateManagerViewModel(PlayniteApi, Settings);
            messagesHandler = new MessagesHandler(PlayniteApi, Settings, playStateManager);
            playstateIconImagePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "playstateIcon.png");

            _gamePadHandler = new GamePadHandler(this, Settings.Settings, playStateManager);
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlayStateSettings.InformationHotkey) ||
                e.PropertyName == nameof(PlayStateSettings.SuspendHotKey))
            {
                RegisterHotkey();
            }
        }

        public bool IsAnyGameRunning()
        {
            return _isAnyGameRunning;
        }

        public void SetSwitchModesOnControlCheck()
        {
            Task.Run(() =>
            {
                var switchModeControllerTimer = new Timer(2000)
                {
                    AutoReset = true,
                    Enabled = true
                };

                Task.Delay(TimeSpan.FromSeconds(Settings.Settings.SwitchModeIgnoreCtrlStateOnStartupSeconds))
                    .GetAwaiter().GetResult();
                switchModeControllerTimer.Elapsed += (_, __) =>
                {
                    SwitchModeOnControllerStatus();
                };
            });
        }

        private void SwitchModeOnControllerStatus()
        {
            if ((_isAnyGameRunning && Settings.Settings.SwitchModesOnlyIfNoRunningGames) || switchPlayniteModeStarted)
            {
                return;
            }

            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                if (Settings.Settings.SwitchToFullscreenModeOnControllerStatus && _gamePadHandler.IsAnyControllerConnected())
                {
                    PerformModeSwitch("Playnite.FullscreenApp.exe", "LOCPlayState_SwitchingToFullscreenModeMessage");
                }
            }
            else
            {
                if (Settings.Settings.SwitchToDesktopModeOnControllerStatus && !_gamePadHandler.IsAnyControllerConnected())
                {
                    PerformModeSwitch("Playnite.DesktopApp.exe", "LOCPlayState_SwitchingToDesktopModeMessage");
                }
            }
        }

        private void PerformModeSwitch(string modeToSwitch, string message)
        {
            var playniteExecutable = Path.Combine(PlayniteApi.Paths.ApplicationPath, modeToSwitch);
            if (FileSystem.FileExists(playniteExecutable))
            {
                messagesHandler.ShowGenericNotification(ResourceProvider.GetString(message));
                ProcessStarter.StartProcess(playniteExecutable);
                switchPlayniteModeStarted = true;
            }
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == "GameStateSwitchControl")
            {
                return new GameStateSwitchControl(playStateManager, PlayniteApi, Settings);
            }

            return null;
        }

        public override IEnumerable<SidebarItem> GetSidebarItems()
        {
            if (Settings.Settings.ShowManagerSidebarItem)
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

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (!Settings.Settings.WindowsNotificationStyleFirstSetupDone && isWindows10Or11)
            {
                PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCPlayState_MessageWinStyleNotificationsFirstSetup"), "PlayState");
                Settings.Settings.WindowsNotificationStyleFirstSetupDone = true;
                SavePluginSettings(Settings.Settings);

                // A notification is shown so Playnite is added to the list
                // to add Playnite to the priority list
                messagesHandler.ShowGenericNotification("PlayState");
                ProcessStarter.StartUrl(@"https://github.com/darklinkpower/PlayniteExtensionsCollection/wiki/PlayState#window-notification-style-configuration");
            }

            var mainWindow = Application.Current.MainWindow;
            if (mainWindow is null)
            {
                logger.Error("Could not find main window. Shortcuts could not be registered.");
                return;
            }

            var windowInterop = new WindowInteropHelper(mainWindow);
            mainWindowHandle = windowInterop.Handle;
            var hwndSource = HwndSource.FromHwnd(mainWindowHandle);
            hwndSource.AddHook(HwndHook);
            RegisterHotkey();
            Settings.Settings.PropertyChanged += Settings_PropertyChanged;
            SetSwitchModesOnControlCheck();
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            _isAnyGameRunning = true;
            if (Settings.Settings.GlobalSuspendMode == SuspendModes.Disabled)
            {
                return;
            }

            if (playStateManager.IsGameBeingDetected(args.Game))
            {
                return;
            }
            
            InitializePlaytimeInfoFile(); // Temporary workaround for sharing PlayState paused time until Playnite allows to share data among extensions

            var game = args.Game;
            if (PlayniteUtilities.GetGameHasFeature(game, featureBlacklist, true))
            {
                logger.Info($"{game.Name} is in PlayState blacklist. Extension execution stopped");
                return;
            }

            InvokeGameProcessesDetection(args);
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            _isAnyGameRunning = PlayniteApi.Database.Games.Any(x => x.IsRunning);
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
            if (Settings.Settings.SubstractSuspendedPlaytimeOnStopped)
            {
                SubstractPlaytimeFromPlayStateData(game, gameData);
            }

            playStateManager.RemovePlayStateData(gameData);
        }

        private void SubstractPlaytimeFromPlayStateData(Game game, PlayStateData gameData)
        {
            if (game.PluginId != Guid.Empty && Settings.Settings.SubstractOnlyNonLibraryGames)
            {
                return;
            }

            var suspendedTime = gameData.SuspendedTime;
            if (suspendedTime == 0)
            {
                logger.Debug($"PlayState data for {game.Name} didn't ellapse time");
                return;
            }

            var elapsedSeconds = Convert.ToUInt64(suspendedTime);
            logger.Debug($"Suspend elapsed seconds for game {game.Name} was {elapsedSeconds}");
            ExportPausedTimeInfo(game, elapsedSeconds); // Temporary workaround for sharing PlayState paused time until Playnite allows to share data among extensions
            if (elapsedSeconds != 0 && game.Playtime >= elapsedSeconds)
            {
                var newPlaytime = game.Playtime - elapsedSeconds;
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
            var menuList = new List<GameMenuItem>();
            if (game is null)
            {
                return menuList;
            }

            var isGameSuspended = playStateManager.GetIsGameSuspended(game);
            if (isGameSuspended is null)
            {
                return menuList;
            }

            menuList.Add(new GameMenuItem
            {
                Description = GetGameMenuSwitchStatusDescription(game, isGameSuspended),
                Icon = playstateIconImagePath,
                Action = a =>
                {
                    playStateManager.SwitchGameState(game);
                }
            });

            return menuList;
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
            return Settings;
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