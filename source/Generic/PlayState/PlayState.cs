﻿using Playnite.SDK;
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
        private bool suspendPlaytimeOnly = false;
        private Window mainWindow;
        private WindowInteropHelper windowInterop;
        private IntPtr mainWindowHandle;
        private HwndSource source = null;
        private bool globalHotkeyRegistered = false;
        private bool processesSuspended = false;
        private List<PlayStateData> playStateData;

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

            playStateData = new List<PlayStateData>();

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
                            SwitchGameState();
                        }
                        else if (vkey == (uint)KeyInterop.VirtualKeyFromKey(settings.Settings.SavedInformationHotkeyGesture.Key))
                        {
                            ShowNotification(NotificationTypes.Information);
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
            InitializePlaytimeInfoFile(); // Temporary workaround for sharing PlayState paused time until Playnite allows to share data among extensions

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
                AddGame(game);
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
                            AddGame(game);
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
                    AddGame(game);
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
                        AddGame(game);
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

        /// <summary>
        /// Method for obtaining the real playtime of the actual session, which is the playtime after substracting the paused time.
        /// </summary>
        private ulong GetRealPlaytime()
        {
            var suspendedTime = playStateData.FirstOrDefault(x => x.Game.Id == currentGame.Id)?.Stopwatch.Elapsed;
            ulong elapsedSeconds = 0;
            if (suspendedTime != null)
            {
                elapsedSeconds = Convert.ToUInt64(suspendedTime.Value.TotalSeconds);
            }
            return Convert.ToUInt64(DateTime.Now.Subtract(playStateData.First(x => x.Game.Id == currentGame.Id).StartDate).TotalSeconds) - elapsedSeconds;
        }

        /// <summary>
        /// Method for obtaining the pertinent "{0} hours {1} minutes" string from playtime in seconds.<br/><br/>
        /// <param name="playtimeSeconds">Playtime in seconds</param>
        /// </summary>
        private string GetHoursString(ulong playtimeSeconds)
        {
            var playtime = TimeSpan.FromSeconds(playtimeSeconds);
            var playtimeHours = playtime.Hours + playtime.Days * 24;
            if (playtimeHours == 1)
            {
                if (playtime.Minutes == 1)
                {
                    return String.Format(ResourceProvider.GetString("LOCPlayState_HourMinutePlayed"), playtimeHours.ToString(), playtime.Minutes.ToString());
                }
                else
                {
                    return String.Format(ResourceProvider.GetString("LOCPlayState_HourMinutesPlayed"), playtimeHours.ToString(), playtime.Minutes.ToString());
                }
            }
            else if (playtimeHours == 0 && playtime.Minutes == 0) // If the playtime is less than a minute, show the seconds instead
            {
                if (playtime.Seconds == 1)
                {
                    return String.Format(ResourceProvider.GetString("LOCPlayState_SecondPlayed"), playtime.Seconds.ToString());
                }
                else
                {
                    return String.Format(ResourceProvider.GetString("LOCPlayState_SecondsPlayed"), playtime.Seconds.ToString());
                }
            }
            else
            {
                if (playtime.Minutes == 1)
                {
                    return String.Format(ResourceProvider.GetString("LOCPlayState_HoursMinutePlayed"), playtimeHours.ToString(), playtime.Minutes.ToString());
                }
                else
                {
                    return String.Format(ResourceProvider.GetString("LOCPlayState_HoursMinutesPlayed"), playtimeHours.ToString(), playtime.Minutes.ToString());
                }
            }
        }

        /// <summary>
        /// Method for showing notifications. It will respect the style (Playnite / Windows) notification settings.<br/><br/>
        /// <param name="status">Status of the game to be notified:<br/>
        /// - "resumed" for resuming process and playtime<br/>
        /// - "playtimeResumed" for resuming playtime<br/>
        /// - "suspended" for suspend process and playtime<br/>
        /// - "playtimeSuspended" for suspend playtime<br/>
        /// - "information" for showing the actual status<br/>
        /// </param>
        /// </summary>
        private void ShowNotification(NotificationTypes status)
        {
            var showWindowsNotificationsStyle = settings.Settings.GlobalShowWindowsNotificationsStyle;

            switch (status)
            {
                case NotificationTypes.Resumed: // for resuming process and playtime
                    if (!showWindowsNotificationsStyle)
                    {
                        ShowSplashWindow(ResourceProvider.GetString("LOCPlayState_StatusResumedMessage"));
                    }
                    else
                    {
                        // Windows notification only admits 3 .Addtext properties, so for more text lines you have to use {Environment.NewLine}
                        new ToastContentBuilder()
                            .AddText(currentGame.Name) // First AddText field will act as a title
                            .AddText(ResourceProvider.GetString("LOCPlayState_StatusResumedMessage"))
                            .AddText($"{ResourceProvider.GetString("LOCPlayState_Playtime")} {GetHoursString(GetRealPlaytime())}" +
                                $"{Environment.NewLine}{ResourceProvider.GetString("LOCPlayState_TotalPlaytime")} {GetHoursString(GetRealPlaytime() + currentGame.Playtime)}")
                            .AddHeroImage(new Uri(PlayniteApi.Database.GetFullFilePath(currentGame.BackgroundImage))) // Show game image in the notification
                            .Show();
                    }
                    break;
                case NotificationTypes.PlaytimeResumed: // for resuming playtime
                    if (!showWindowsNotificationsStyle)
                    {
                        ShowSplashWindow(ResourceProvider.GetString("LOCPlayState_StatusPlaytimeResumedMessage"));
                    }
                    else
                    {
                        new ToastContentBuilder()
                            .AddText(currentGame.Name)
                            .AddText(ResourceProvider.GetString("LOCPlayState_StatusPlaytimeResumedMessage"))
                            .AddText($"{ResourceProvider.GetString("LOCPlayState_Playtime")} {GetHoursString(GetRealPlaytime())}" +
                                $"{Environment.NewLine}{ResourceProvider.GetString("LOCPlayState_TotalPlaytime")} {GetHoursString(GetRealPlaytime() + currentGame.Playtime)}")
                            .AddHeroImage(new Uri(PlayniteApi.Database.GetFullFilePath(currentGame.BackgroundImage)))
                            .Show();
                    }
                    break;
                case NotificationTypes.Suspended: // for suspend process and playtime
                    if (!showWindowsNotificationsStyle)
                    {
                        ShowSplashWindow(ResourceProvider.GetString("LOCPlayState_StatusSuspendedMessage"));
                    }
                    else
                    {
                        new ToastContentBuilder()
                            .AddText(currentGame.Name)
                            .AddText(ResourceProvider.GetString("LOCPlayState_StatusSuspendedMessage"))
                            .AddText($"{ResourceProvider.GetString("LOCPlayState_Playtime")} {GetHoursString(GetRealPlaytime())}" +
                                $"{Environment.NewLine}{ResourceProvider.GetString("LOCPlayState_TotalPlaytime")} {GetHoursString(GetRealPlaytime() + currentGame.Playtime)}")
                            .AddHeroImage(new Uri(PlayniteApi.Database.GetFullFilePath(currentGame.BackgroundImage)))
                            .Show();
                    }
                    break;
                case NotificationTypes.PlaytimeSuspended: // for suspend playtime
                    if (!showWindowsNotificationsStyle)
                    {
                        ShowSplashWindow(ResourceProvider.GetString("LOCPlayState_StatusPlaytimeSuspendedMessage"));
                    }
                    else
                    {
                        new ToastContentBuilder()
                            .AddText(currentGame.Name)
                            .AddText(ResourceProvider.GetString("LOCPlayState_StatusPlaytimeSuspendedMessage"))
                            .AddText($"{ResourceProvider.GetString("LOCPlayState_Playtime")} {GetHoursString(GetRealPlaytime())}" +
                                $"{Environment.NewLine}{ResourceProvider.GetString("LOCPlayState_TotalPlaytime")} {GetHoursString(GetRealPlaytime() + currentGame.Playtime)}")
                            .AddHeroImage(new Uri(PlayniteApi.Database.GetFullFilePath(currentGame.BackgroundImage)))
                            .Show();
                    }
                    break;
                case NotificationTypes.Information: // for showing the actual status
                    {
                        if (currentGame == null || !playStateData.Any(x => x.Game.Id == currentGame.Id))
                        {
                            break;
                        }
                        if (isSuspended)
                        {
                            if (processesSuspended)
                            {
                                ShowNotification(NotificationTypes.Suspended);
                            }
                            else
                            {
                                ShowNotification(NotificationTypes.PlaytimeSuspended);
                            }
                        }
                        else
                        {
                            if (processesSuspended)
                            {
                                ShowNotification(NotificationTypes.Resumed);
                            }
                            else
                            {
                                ShowNotification(NotificationTypes.PlaytimeResumed);
                            }
                        }
                    }
                    break;
            }
        }

        private void SwitchGameState()
        {
            if (currentGame == null)
            {
                if (isSuspended)
                {
                    isSuspended = false;
                    processesSuspended = false;
                }
                return;
            }

            try
            {
                processesSuspended = false;
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
                            ShowNotification(NotificationTypes.Resumed);
                        }
                        else
                        {
                            ShowNotification(NotificationTypes.PlaytimeResumed);
                        }
                        playStateData.Find(x => x.Game.Id == currentGame.Id)?.Stopwatch.Stop();
                        logger.Debug($"Game {currentGame.Name} resumed");
                    }
                    else
                    {
                        isSuspended = true;
                        if (processesSuspended)
                        {
                            ShowNotification(NotificationTypes.Suspended);
                        }
                        else
                        {
                            ShowNotification(NotificationTypes.PlaytimeSuspended);
                        }
                        playStateData.Find(x => x.Game.Id == currentGame.Id)?.Stopwatch.Start();
                        logger.Debug($"Game {currentGame.Name} suspended");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Error while suspending or resuming game");
                gameProcesses = null;
                playStateData.Find(x => x.Game.Id == currentGame.Id)?.Stopwatch.Stop();
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
                    var suspendedTime = playStateData.Find(x => x.Game.Id == currentGame.Id)?.Stopwatch.Elapsed;
                    if (suspendedTime != null)
                    {
                        var elapsedSeconds = Convert.ToUInt64(suspendedTime.Value.TotalSeconds);
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
                }
            }

            RemoveGame(game);
        }

        private void AddGame(Game game)
        {
            if (playStateData.Any(x => x.Game.Id == game.Id))
            {
                logger.Debug($"Data for game {game.Name} with id {game.Id} already exists");
            }
            else
            {
                playStateData.Add(new PlayStateData(game));
                logger.Debug($"Data for game {game.Name} with id {game.Id} was created");
            }
        }

        private void RemoveGame(Game game)
        {
            var gameData = playStateData.FirstOrDefault(x => x.Game.Id == game.Id);
            if (gameData != null)
            {
                playStateData.Remove(gameData);
                logger.Debug($"Data for game {game.Name} with id {game.Id} was removed on game stopped");
                if (currentGame == game)
                {
                    currentGame = playStateData.Any() ? playStateData.Last().Game : null;
                }
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