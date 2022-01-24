using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
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

namespace PlayState
{
    public class PlayState : GenericPlugin
    {
        [DllImport("ntdll.dll", PreserveSig = false)]
        public static extern void NtSuspendProcess(IntPtr processHandle);
        [DllImport("ntdll.dll", PreserveSig = false)]
        public static extern void NtResumeProcess(IntPtr processHandle);
        private static readonly ILogger logger = LogManager.GetLogger();
        private Game currentGame;
        private List<string> exclusionList;
        private string gameInstallDir;
        private bool isSuspended = false;
        private Window currentSplashWindow;
        private DispatcherTimer timer;
        private List<ProcessItem> gameProcesses;
        private List<Tuple<Guid, Stopwatch>> stopwatchList;
        private bool suspendPlaytimeOnly = false;
        private Window mainWindow;
        private WindowInteropHelper windowInterop;
        private IntPtr mainWindowHandle;
        private HwndSource source = null;
        private bool globalHotkeyRegistered = false;

        private PlayStateSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("26375941-d460-4d32-925f-ad11e2facd8f");
        internal SplashWindowViewModel splashWindowViewModel { get; private set; }

        public PlayState(IPlayniteAPI api) : base(api)
        {
            settings = new PlayStateSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
            SetExclusionList();

            stopwatchList = new List<Tuple<Guid, Stopwatch>>();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += (src, args) =>
            {
                timer.Stop();
                if (currentSplashWindow != null)
                {
                    currentSplashWindow.Hide();
                    currentSplashWindow.Topmost = false;
                }
            };

            splashWindowViewModel = new SplashWindowViewModel();
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
        }

        internal bool RegisterGlobalHotkey()
        {
            if (!globalHotkeyRegistered)
            {
                var window = mainWindow;
                var handle = mainWindowHandle;
                source.AddHook(GlobalHotkeyCallback);
                globalHotkeyRegistered = true;
                var registered = HotkeyHelper.RegisterHotKey(handle, HOTKEY_ID, settings.Settings.SavedHotkeyGesture.Modifiers.ToVK(), (uint)KeyInterop.VirtualKeyFromKey(settings.Settings.SavedHotkeyGesture.Key));

                if (registered)
                {
                    logger.Debug($"Hotkey registered with hotkey {settings.Settings.SavedHotkeyGesture}.");
                }
                else
                {
                    PlayniteApi.Notifications.Add(new NotificationMessage(Guid.NewGuid().ToString(),
                        "PlayState: " + string.Format(ResourceProvider.GetString("LOCPlayState_NotificationMessageHotkeyRegisterFailed"), settings.Settings.SavedHotkeyGesture),
                        NotificationType.Error));
                    logger.Error($"Failed to register configured Hotkey {settings.Settings.SavedHotkeyGesture}.");
                }

                return registered;
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
                            SwitchGameState();
                        }
                        handled = true;
                        break;
                    default:
                        break;
                }
            }

            return IntPtr.Zero;
        }

        private void SetExclusionList()
        {
            exclusionList = new List<string>
            {
                "7z.exe",
                "7za.exe",
                "Archive.exe",
                "asset_.exe",
                "anetdrop.exe",
                "Bat_To_Exe_Convertor.exe",
                "BsSndRpt.exe",
                "BootBoost.exe",
                "bootstrap.exe",
                "cabarc.exe",
                "CDKey.exe",
                "Cheat Engine.exe",
                "cheatengine",
                "Civ2Map.exe",
                "config",
                "CLOSEPW.EXE",
                "CrashDump",
                "CrashReport",
                "crc32.exe",
                "CreationKit.exe",
                "CreatureUpload.exe",
                "EasyHook.exe",
                "dgVoodooCpl.exe",
                "dotNet",
                "doc.exe",
                "DXSETUP",
                "dw.exe",
                "ENBInjector.exe",
                "HavokBehaviorPostProcess.exe",
                "help",
                "install",
                "LangSelect.exe",
                "Language.exe",
                "Launch",
                "loader",
                "MapCreator.exe",
                "master_dat_fix_up.exe",
                "md5sum.exe",
                "MGEXEgui.exe",
                "modman.exe",
                "ModOrganizer.exe",
                "notepad++.exe",
                "notification_helper.exe",
                "oalinst.exe",
                "PalettestealerSuspender.exe",
                "pak",
                "patch",
                "planet_mapgen.exe",
                "Papyrus",
                "RADTools.exe",
                "readspr.exe",
                "register.exe",
                "SekiroFPSUnlocker",
                "settings",
                "setup",
                "SCUEx64.exe",
                "synchronicity.exe",
                "syscheck.exe",
                "SystemSurvey.exe",
                "TES Construction Set.exe",
                "Texmod.exe",
                "unins",
                "UnityCrashHandler",
                "x360ce",
                "Unpack",
                "UnX_Calibrate",
                "update",
                "UnrealCEFSubProcess.exe",
                "url.exe",
                "versioned_json.exe",
                "vcredist",
                "xtexconv.exe",
                "xwmaencode.exe",
                "Website.exe",
                "wide_on.exe"
            };
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            if (isSuspended)
            {
                SwitchGameState();
            }

            var game = args.Game;
            if (game.Features != null && game.Features.Any(a => a.Name.Equals("[PlayState] Blacklist", StringComparison.OrdinalIgnoreCase)))
            {
                logger.Info($"{game.Name} is in PlayState blacklist. Extension execution stopped");
                return;
            }

            suspendPlaytimeOnly = false;
            if (settings.Settings.SubstractSuspendedPlaytimeOnStopped &&
                (settings.Settings.GlobalOnlySuspendPlaytime ||
                game.Features != null && game.Features.Any(a => a.Name.Equals("[PlayState] Suspend Playtime only", StringComparison.OrdinalIgnoreCase))))
            {
                suspendPlaytimeOnly = true;
                currentGame = game;
                splashWindowViewModel.GameName = currentGame.Name;
                gameProcesses = null;
                CreateGameStopwatchTuple(game);
                return;
            }

            Task.Run(async () =>
            {
                currentGame = game;
                splashWindowViewModel.GameName = currentGame.Name;
                gameProcesses = null;
                isSuspended = false;
                logger.Debug($"Changed game to {currentGame.Name} game processes");
                var sourceAction = args.SourceAction;
                if (sourceAction?.Type == GameActionType.Emulator)
                {
                    var emulatorProfileId = sourceAction.EmulatorProfileId;
                    if (emulatorProfileId.StartsWith("#builtin_"))
                    {
                        //Currently it isn't possible to obtain the emulator path
                        //for emulators using Builtin profiles
                        return;
                    }

                    var emulator = PlayniteApi.Database.Emulators[sourceAction.EmulatorId];
                    var profile = emulator?.CustomProfiles.FirstOrDefault(p => p.Id == emulatorProfileId);
                    if (profile != null)
                    {
                        gameProcesses = GetProcessesWmiQuery(false, profile.Executable.ToLower());
                        if (gameProcesses.Count > 0)
                        {
                            CreateGameStopwatchTuple(game);
                        }
                    }
                    return;
                }

                if (string.IsNullOrEmpty(game.InstallDirectory))
                {
                    return;
                }
                gameInstallDir = game.InstallDirectory.ToLower();

                // Fix for some games that take longer to start, even when already detected as running
                await Task.Delay(15000);
                if (CurrentGameChanged(game))
                {
                    return;
                }

                gameProcesses = GetProcessesWmiQuery(true);
                if (gameProcesses.Count > 0)
                {
                    logger.Debug($"Found {gameProcesses.Count} game processes");
                    CreateGameStopwatchTuple(game);
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
                    if (CurrentGameChanged(game))
                    {
                        return;
                    }

                    // Try a few times with filters.
                    // If nothing is found, try without filters. This helps in cases
                    // where the active process is being filtered out by filters
                    if (i == 5)
                    {
                        filterPaths = false;
                    }
                    gameProcesses = GetProcessesWmiQuery(filterPaths);
                    if (gameProcesses.Count > 0)
                    {
                        logger.Debug($"Found {gameProcesses.Count} game processes");
                        CreateGameStopwatchTuple(game);
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

        private bool CurrentGameChanged(Game game)
        {
            if (currentGame == null || currentGame.Id != game.Id)
            {
                return true;
            }

            return false;
        }

        private void CreateSplashWindow()
        {
            currentSplashWindow = new Window
            {
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Topmost = false,
                ShowActivated = false,
                SizeToContent = SizeToContent.WidthAndHeight,
                ShowInTaskbar = false,
                Focusable = false,
                Content = new SplashWindow(),
                DataContext = splashWindowViewModel
            };

            currentSplashWindow.Closed += WindowClosed;
        }

        private void ShowSplashWindow(string status)
        {
            if (currentSplashWindow == null)
            {
                CreateSplashWindow();
            }

            splashWindowViewModel.SuspendStatus = status;
            currentSplashWindow.Topmost = true;
            currentSplashWindow.Show();
            timer.Start();
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            currentSplashWindow.Topmost = false;
            currentSplashWindow.Closed -= WindowClosed;
        }

        private void SwitchGameState()
        {
            if (currentGame == null)
            {
                return;
            }

            try
            {
                var processesSuspended = false;
                if (gameProcesses != null && gameProcesses.Count > 0)
                {
                    foreach (var gameProcess in gameProcesses)
                    {
                        if (gameProcess == null || gameProcess.Process.Handle == null || gameProcess.Process.Handle == IntPtr.Zero)
                        {
                            return;
                        }
                        if (isSuspended)
                        {
                            NtResumeProcess(gameProcess.Process.Handle);
                        }
                        else
                        {
                            NtSuspendProcess(gameProcess.Process.Handle);
                        }
                    }
                    processesSuspended = true;
                }

                if (processesSuspended || suspendPlaytimeOnly)
                {
                    if (isSuspended)
                    {
                        isSuspended = false;
                        if (processesSuspended)
                        {
                            ShowSplashWindow(ResourceProvider.GetString("LOCPlayState_StatusResumedMessage"));
                        }
                        else
                        {
                            ShowSplashWindow(ResourceProvider.GetString("LOCPlayState_StatusPlaytimeResumedMessage"));
                        }
                        stopwatchList.FirstOrDefault(x => x.Item1 == currentGame.Id)?.Item2.Stop();
                        logger.Debug($"Game {currentGame.Name} resumed");
                    }
                    else
                    {
                        isSuspended = true;
                        if (processesSuspended)
                        {
                            ShowSplashWindow(ResourceProvider.GetString("LOCPlayState_StatusSuspendedMessage"));
                        }
                        else
                        {
                            ShowSplashWindow(ResourceProvider.GetString("LOCPlayState_StatusPlaytimeSuspendedMessage"));
                        }
                        stopwatchList.FirstOrDefault(x => x.Item1 == currentGame.Id)?.Item2.Start();
                        logger.Debug($"Game {currentGame.Name} suspended");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Error while suspending or resuming game");
                gameProcesses = null;
                stopwatchList.FirstOrDefault(x => x.Item1 == currentGame.Id)?.Item2.Stop();
            }
        }

        private List<ProcessItem> GetProcessesWmiQuery(bool filterPaths, string exactPath = null)
        {
            var wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process";
            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            using (var results = searcher.Get())
            {
                // Unfortunately due to Playnite being a 32 bits process, the GetProcess()
                // method can't access needed values of 64 bits processes, so it's needed
                // to correlate with data obtained from a WMI query that is exponentially slower.
                // It needs to be done this way until #1199 is done
                var query = from p in Process.GetProcesses()
                            join mo in results.Cast<ManagementObject>()
                            on p.Id equals (int)(uint)mo["ProcessId"]
                            select new
                            {
                                Process = p,
                                Path = (string)mo["ExecutablePath"],
                            };

                var gameProcesses = new List<ProcessItem>();
                if (exactPath != null)
                {
                    foreach (var fItem in query.Where(i => i.Path != null && i.Path.ToLower() == exactPath))
                    {
                        gameProcesses.Add(
                           new ProcessItem
                           {
                               ExecutablePath = fItem.Path,
                               Process = fItem.Process
                           }
                       );
                    }
                }
                else
                {
                    foreach (var item in query)
                    {
                        if (item.Path == null)
                        {
                            continue;
                        }

                        var pathLower = item.Path.ToLower();
                        if (!pathLower.StartsWith(gameInstallDir))
                        {
                            continue;
                        }

                        if (filterPaths)
                        {
                            var fileName = Path.GetFileName(pathLower);
                            if (exclusionList.Any(e => fileName.Contains(e)))
                            {
                                continue;
                            }
                        }

                        gameProcesses.Add(
                            new ProcessItem
                            {
                                ExecutablePath = item.Path,
                                Process = item.Process
                            }
                        );
                    }
                }

                return gameProcesses;
            }
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            var game = args.Game;
            if (currentGame != null && currentGame.Id == game.Id)
            {
                gameProcesses = null;
            }
            if (currentSplashWindow != null)
            {
                currentSplashWindow.Hide();
                currentSplashWindow.Topmost = false;
            }

            if (settings.Settings.SubstractSuspendedPlaytimeOnStopped)
            {
                if (game.PluginId == Guid.Empty ||
                    (game.PluginId != Guid.Empty && !settings.Settings.SubstractOnlyNonLibraryGames))
                {
                    var suspendedTime = stopwatchList.FirstOrDefault(x => x.Item1 == game.Id)?.Item2.Elapsed;
                    if (suspendedTime != null)
                    {
                        var elapsedSeconds = Convert.ToUInt64(suspendedTime.Value.TotalSeconds);
                        logger.Debug($"Suspend elapsed seconds for game {game.Name} was {elapsedSeconds}");
                        if (elapsedSeconds != 0)
                        {
                            var newPlaytime = game.Playtime > elapsedSeconds ? game.Playtime - elapsedSeconds : elapsedSeconds - game.Playtime;
                            logger.Debug($"Old playtime {game.Playtime}, new playtime {newPlaytime}");
                            game.Playtime = newPlaytime;
                            PlayniteApi.Database.Games.Update(game);
                        }
                    }
                }
            }

            RemoveGameStopwatchTuple(game);
        }

        private void CreateGameStopwatchTuple(Game game)
        {
            if (stopwatchList.Any(x => x.Item1 == game.Id))
            {
                logger.Debug($"A stopwatch for game {game.Name} with id {game.Id} already exists");
            }
            else
            {
                stopwatchList.Add(new Tuple<Guid, Stopwatch>(game.Id, new Stopwatch()));
                logger.Debug($"Stopwatch for game {game.Name} with id {game.Id} was created");
            }
        }

        private void RemoveGameStopwatchTuple(Game game)
        {
            var tuple = stopwatchList.FirstOrDefault(x => x.Item1 == game.Id);
            if (tuple != null)
            {
                stopwatchList.Remove(tuple);
                logger.Debug($"Stopwatch for game {game.Name} with id {game.Id} was removed on game stopped");
            }
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
                        var featureAddedCount = AddFeatureToSelectedGames("[PlayState] Blacklist");
                        PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCPlayState_BlacklistAddedResultsMessage"), featureAddedCount), "PlayState");
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCPlayState_MenuItemRemoveFromBlacklistDescription"),
                    MenuSection = "@PlayState",
                    Action = a => {
                        var featureRemovedCount = RemoveFeatureFromSelectedGames("[PlayState] Blacklist");
                        PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCPlayState_BlacklistRemovedResultsMessage"), featureRemovedCount), "PlayState");
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCPlayState_MenuItemAddToPlaytimeSuspendDescription"),
                    MenuSection = "@PlayState",
                    Action = a => {
                        var featureAddedCount = AddFeatureToSelectedGames("[PlayState] Suspend Playtime only");
                        PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCPlayState_PlaytimeSuspendAddedResultsMessage"), featureAddedCount), "PlayState");
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCPlayState_MenuItemRemoveFromPlaytimeSuspendDescription"),
                    MenuSection = "@PlayState",
                    Action = a => {
                        var featureRemovedCount = RemoveFeatureFromSelectedGames("[PlayState] Suspend Playtime only");
                        PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCPlayState_PlaytimeSuspendRemovedResultsMessage"), featureRemovedCount), "PlayState");
                    }
                }
            };
        }

        private int RemoveFeatureFromSelectedGames(string featureName)
        {
            var feature = PlayniteApi.Database.Features.Add(featureName);
            int featureRemovedCount = 0;
            foreach (var game in PlayniteApi.MainView.SelectedGames)
            {
                if (game.FeatureIds != null && game.FeatureIds.Contains(feature.Id))
                {
                    game.FeatureIds.Remove(feature.Id);
                    PlayniteApi.Database.Games.Update(game);
                    featureRemovedCount++;
                    logger.Info(string.Format("Removed \"{0}\" feature from \"{1}\"", featureName, game.Name));
                }
            }
            return featureRemovedCount;
        }

        public int AddFeatureToSelectedGames(string featureName)
        {
            var feature = PlayniteApi.Database.Features.Add(featureName);
            int featureAddedCount = 0;
            foreach (var game in PlayniteApi.MainView.SelectedGames)
            {
                if (game.FeatureIds == null)
                {
                    game.FeatureIds = new List<Guid> { feature.Id };
                    PlayniteApi.Database.Games.Update(game);
                    featureAddedCount++;
                }
                else if (game.FeatureIds.AddMissing(feature.Id))
                {
                    PlayniteApi.Database.Games.Update(game);
                    featureAddedCount++;
                }
                else
                {
                    continue;
                }
                logger.Info(string.Format("Added \"{0}\" feature to \"{1}\"", featureName, game.Name));
            }
            return featureAddedCount;
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