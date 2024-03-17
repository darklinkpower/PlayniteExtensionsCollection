using InstallationStatusUpdater.Models;
using InstallationStatusUpdater.ViewModels;
using InstallationStatusUpdater.Views;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace InstallationStatusUpdater
{
    public class InstallationStatusUpdater : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private const string driveTagPrefix = "[Install Drive]";
        private const string scanSkipFeatureName = "[Status Updater] Ignore";
        private const char backslash = '\\';
        private const char fordwslash = '/';
        private const char doubleDot = ':';
        private static readonly HashSet<char> invalidFileChars = new HashSet<char>(Path.GetInvalidFileNameChars());
        private List<FileSystemWatcher> dirWatchers = new List<FileSystemWatcher>();
        private DispatcherTimer timer;
        private Window mainWindow;
        private WindowInteropHelper windowInterop;
        private IntPtr mainWindowHandle;
        private HwndSource source;

        private InstallationStatusUpdaterSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("ed9c467f-5ab5-478f-a09f-936146188ad0");

        public InstallationStatusUpdater(IPlayniteAPI api) : base(api)
        {
            settings = new InstallationStatusUpdaterSettingsViewModel(this, PlayniteApi);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            SetDirWatchers();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(5000);
            timer.Tick += new EventHandler(Timer_Tick);
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            mainWindow = Application.Current.MainWindow;
            if (mainWindow != null)
            {
                windowInterop = new WindowInteropHelper(mainWindow);
                mainWindowHandle = windowInterop.Handle;
                source = HwndSource.FromHwnd(mainWindowHandle);
                source.AddHook(GlobalHotkeyCallback);
            }
            else
            {
                logger.Debug("Could not find Playnite main window.");
            }

            if (settings.Settings.UpdateOnStartup)
            {
                DetectInstallationStatus(false);
            }
        }

        private const int WM_DEVICECHANGE = 0x0219;                 // device change event
        private const int DBT_DEVICEARRIVAL = 0x8000;               // system detected a new device
        private const int DBT_DEVICEREMOVEPENDING = 0x8003;         // about to remove, still available
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;        // device is gone
        private IntPtr GlobalHotkeyCallback(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (!settings.Settings.UpdateStatusOnUsbChanges)
            {
                return IntPtr.Zero;
            }

            if (msg == WM_DEVICECHANGE)
            {
                switch (wParam.ToInt32())
                {
                    //case WM_DEVICECHANGE:
                    //    break;
                    case DBT_DEVICEARRIVAL:
                        logger.Debug("Started timer from DBT_DEVICEARRIVAL event");
                        timer.Stop();
                        timer.Start();
                        handled = true;
                        break;
                    //case DBT_DEVICEREMOVEPENDING:
                    //    break;
                    case DBT_DEVICEREMOVECOMPLETE:
                        logger.Debug("Started timer from DBT_DEVICEREMOVECOMPLETE event");
                        timer.Stop();
                        timer.Start();
                        handled = true;
                        break;
                    default:
                        break;
                }
            }

            return IntPtr.Zero;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Timer is used to ensure multiple executions are not triggered
            // when there are multiple changes in bulk
            timer.Stop();
            logger.Debug("Starting detection by timer");
            DetectInstallationStatus(false);
        }

        private void SetDirWatchers()
        {
            if (dirWatchers.Count > 0)
            {
                foreach (FileSystemWatcher watcher in dirWatchers)
                {
                    watcher.Dispose();
                }
            }

            dirWatchers = new List<FileSystemWatcher>();
            if (!settings.Settings.UpdateStatusOnDirChanges || settings.Settings.DetectionDirectories.Count == 0)
            {
                return;
            }

            foreach (SelectableDirectory dir in settings.Settings.DetectionDirectories)
            {
                if (!dir.Enabled)
                {
                    continue;
                }

                if (!Directory.Exists(dir.DirectoryPath))
                {
                    logger.Warn($"Directory {dir.DirectoryPath} for watcher doesn't exist");
                    continue;
                }

                var watcher = new FileSystemWatcher(dir.DirectoryPath)
                {
                    NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Size,

                    Filter = "*.*"
                };

                watcher.Changed += OnChanged;
                watcher.Created += OnCreated;
                watcher.Deleted += OnDeleted;
                watcher.Renamed += OnRenamed;

                watcher.IncludeSubdirectories = dir.ScanSubDirs;
                watcher.EnableRaisingEvents = true;
                dirWatchers.Add(watcher);
            }
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            WatcherEventHandler(e.FullPath);
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            WatcherEventHandler(e.FullPath);
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            WatcherEventHandler(e.FullPath);
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            WatcherEventHandler(e.FullPath);
        }

        private void WatcherEventHandler(string invokerPath)
        {
            logger.Info(string.Format("Watcher invoked by path {0}", invokerPath));
            timer.Stop();
            timer.Start();
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            if (settings.Settings.UpdateOnLibraryUpdate == true)
            {
                DetectInstallationStatus(false);
            }
            if (settings.Settings.UpdateLocTagsOnLibUpdate == true)
            {
                UpdateInstallDirTags();
            }
            SetDirWatchers();
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new InstallationStatusUpdaterSettingsView();
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {

            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCInstallation_Status_Updater_MenuItemStatusUpdaterDescription"),
                    MenuSection = "@Installation Status Updater",
                    Action = a => {
                        DetectInstallationStatus(true);
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCInstallation_Status_Updater_MenuUpdateDriveInstallTagDescription"),
                    MenuSection = "@Installation Status Updater",
                    Action = a => {
                        UpdateInstallDirTags();
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCInstallation_Status_Updater_StatusUpdaterUpdatingTagsFinishMessage"), "Installation Status Updater");
                    }
                }
            };
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            return new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCInstallation_Status_Updater_MenuAddIgnoreFeatureDescription"),
                    MenuSection = "@Installation Status Updater",
                    Action = a => {
                        AddIgnoreFeature(a.Games.Distinct());
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCInstallation_Status_Updater_MenuRemoveIgnoreFeatureDescription"),
                    MenuSection = "@Installation Status Updater",
                    Action = a => {
                        RemoveIgnoreFeature(a.Games.Distinct());
                    }
                }
            };
        }

        public override IEnumerable<InstallController> GetInstallActions(GetInstallActionsArgs args)
        {
            if (!settings.Settings.EnableInstallButtonAction || ShouldGameBeScanned(args.Game))
            {
                yield break;
            }

            yield return new StatusUpdaterInstallController(args.Game, IsGameInstalled(args.Game));
        }

        private bool DetectIsRomInstalled(Game game, GameRom rom, string installDirectory)
        {
            if (rom.Path.IsNullOrEmpty())
            {
                return false;
            }

            var romPath = RemoveInvalidPathChars(rom.Path);
            // ExpandGameVariables method doesn't expand EmulatorDir variable
            // manually expanding this variable is needed before the Api method as
            // it removes the variable without expanding it
            if (romPath.Contains("{EmulatorDir}"))
            {
                var emulator = GetGameEmulator(game);
                if (emulator != null && !emulator.InstallDir.IsNullOrEmpty())
                {
                    romPath = romPath.Replace("{EmulatorDir}", emulator.InstallDir);
                }
            }

            if (romPath.Contains('{'))
            {
                romPath = PlayniteApi.ExpandGameVariables(game, romPath);
            }

            if (Path.IsPathRooted(romPath))
            {
                return FileSystem.FileExists(romPath);
            }

            if (!installDirectory.IsNullOrEmpty())
            {
                romPath = Path.Combine(installDirectory, romPath);
            }

            return FileSystem.FileExists(romPath);
        }

        private Emulator GetGameEmulator(Game game)
        {
            if (!game.GameActions.HasItems())
            {
                return null;
            }

            foreach (var gameAction in game.GameActions)
            {
                if (gameAction.Type != GameActionType.Emulator)
                {
                    continue;
                }

                var emulator = PlayniteApi.Database.Emulators[gameAction.EmulatorId];
                if (emulator != null)
                {
                    return emulator;
                }
            }

            return null;
        }

        private bool IsRomInstalled(Game game, string installDirectory)
        {
            if (!game.Roms.HasItems())
            {
                return false;
            }

            if (settings.Settings.UseOnlyFirstRomDetection)
            {
                return DetectIsRomInstalled(game, game.Roms[0], installDirectory);
            }
            else
            {
                foreach (GameRom rom in game.Roms)
                {
                    if (DetectIsRomInstalled(game, rom, installDirectory))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool DetectIsFileActionInstalled(Game game, GameAction gameAction, string installDirectory)
        {
            if (gameAction.Path.IsNullOrEmpty())
            {
                return false;
            }

            //Games added as Microsoft Store Application use explorer and arguments to launch the game
            if (gameAction.Path.Equals("explorer.exe", StringComparison.OrdinalIgnoreCase))
            {
                //If directory has been set, it can be used to detect if game is installed or not
                if (!installDirectory.IsNullOrEmpty())
                {
                    return FileSystem.DirectoryExists(installDirectory);
                }
                else
                {
                    return true;
                }
            }

            var actionPath = gameAction.Path;
            if (gameAction.Path.Contains('{'))
            {
                actionPath = PlayniteApi.ExpandGameVariables(game, actionPath);
            }

            if (Path.IsPathRooted(actionPath))
            {
                return FileSystem.FileExists(actionPath);
            }

            if (!installDirectory.IsNullOrEmpty())
            {
                actionPath = Path.Combine(installDirectory, actionPath);
            }

            return FileSystem.FileExists(actionPath);
        }

        private bool IsAnyActionInstalled(Game game, string installDirectory)
        {
            foreach (GameAction gameAction in game.GameActions)
            {
                if (!gameAction.IsPlayAction && settings.Settings.OnlyUsePlayActionGameActions)
                {
                    continue;
                }
                
                if (gameAction.Type == GameActionType.URL)
                {
                    if (settings.Settings.UrlActionIsInstalled)
                    {
                        return true;
                    }
                }
                else if (gameAction.Type == GameActionType.Script)
                {
                    if (settings.Settings.ScriptActionIsInstalled)
                    {
                        return true;
                    }
                }
                else if (gameAction.Type == GameActionType.File)
                {
                    return DetectIsFileActionInstalled(game, gameAction, installDirectory);
                }
            }

            return false;
        }

        private void DetectInstallationStatus(bool showResultsDialog)
        {
            var markedInstalled = 0;
            var markedUninstalled = 0;
            var updateResults = new StatusUpdateResults();
            using (PlayniteApi.Database.BufferedUpdate())
            {
                foreach (var game in PlayniteApi.Database.Games)
                {
                    if (!ShouldGameBeScanned(game))
                    {
                        continue;
                    }

                    var detectedAsInstalled = IsGameInstalled(game);
                    if (game.IsInstalled && !detectedAsInstalled)
                    {
                        game.IsInstalled = false;
                        PlayniteApi.Database.Games.Update(game);
                        markedUninstalled++;
                        updateResults.AddSetAsUninstalledGame(game);
                    }
                    else if (!game.IsInstalled && detectedAsInstalled)
                    {
                        game.IsInstalled = true;
                        PlayniteApi.Database.Games.Update(game);
                        markedInstalled++;
                        updateResults.AddSetAsInstalledGame(game);
                    }
                }
            }

            var anyGameUpdated = markedInstalled > 0 || markedUninstalled > 0;
            if (showResultsDialog)
            {
                if (anyGameUpdated)
                {
                    var options = new List<MessageBoxOption>
                    {
                        new MessageBoxOption(ResourceProvider.GetString("LOCInstallation_Status_Updater_StatusUpdateResultsDialogViewResultsLabel")),
                        new MessageBoxOption(ResourceProvider.GetString("LOCInstallation_Status_Updater_StatusUpdateResultsDialogCloseLabel"), true, true)
                    };

                    var selected = PlayniteApi.Dialogs.ShowMessage(
                        string.Format(ResourceProvider.GetString("LOCInstallation_Status_Updater_StatusUpdaterResultsMessage"),
                            markedUninstalled.ToString(),
                            markedInstalled.ToString()),
                        "Installation Status Updater",
                        MessageBoxImage.None,
                        options);
                    if (selected.IsCancel)
                    {
                        return;
                    }

                    if (selected == options[0])
                    {
                        OpenResultsWindow(updateResults);
                    }
                }
                else
                {
                    PlayniteApi.Dialogs.ShowMessage(
                        string.Format(ResourceProvider.GetString("LOCInstallation_Status_Updater_StatusUpdaterResultsMessage"),
                        markedUninstalled.ToString(),
                        markedInstalled.ToString()), "Installation Status Updater"
                    );
                }
            }
            else if (anyGameUpdated)
            {
                if (FileSystem.FileExists(Path.Combine(GetPluginUserDataPath(), "DisableNotifications")))
                {
                    logger.Info("Super secret \"DisableNotifications\" file detected. Notification not added.");
                    return;
                }
                
                PlayniteApi.Notifications.Add(new NotificationMessage(Guid.NewGuid().ToString(),
                    string.Format(ResourceProvider.GetString("LOCInstallation_Status_Updater_NotificationMessageMarkedInstalledResults"), markedInstalled, markedUninstalled),
                    NotificationType.Info,
                    () => OpenResultsWindow(updateResults)));
            }

            logger.Info(string.Format("Marked as installed: {0}, as uinstalled: {1} ", markedInstalled, markedUninstalled));
        }

        private void OpenResultsWindow(StatusUpdateResults updateResults)
        {
            var window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = true
            });

            window.Height = 500;
            window.Width = 650;
            window.Title = ResourceProvider.GetString("LOCInstallation_Status_Updater_StatusUpdateResultsWindowTitle");

            window.Content = new StatusUpdateResultsWindow();
            window.DataContext = new StatusUpdateResultsWindowViewModel(updateResults);
            window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            window.ShowDialog();
        }

        private bool ShouldGameBeScanned(Game game)
        {
            if (game.PluginId != Guid.Empty && game.IncludeLibraryPluginAction
                && !settings.Settings.ScanGamesHandledByLibPlugins)
            {
                return false;
            }

            if (game.Features != null && game.Features.Any(x => x.Name == scanSkipFeatureName))
            {
                return false;
            }

            return true;
        }

        private static string RemoveInvalidPathChars(string str)
        {
            if (!str.Any(c => invalidFileChars.Contains(c) && c != backslash && c != fordwslash && c != doubleDot))
            {
                return str;
            }

            return new string(str.Where(c => !invalidFileChars.Contains(c) || c == backslash || c == fordwslash || c == doubleDot).ToArray());
        }

        private bool IsGameInstalled(Game game)
        {
            var installDirectory = GetInstallDirForDetection(game);
            if (game.IncludeLibraryPluginAction && !game.InstallDirectory.IsNullOrEmpty())
            {
                if (FileSystem.DirectoryExists(game.InstallDirectory))
                {
                    return true;
                }
            }

            if (game.GameActions.HasItems() && IsAnyActionInstalled(game, installDirectory))
            {
                return true;
            }

            return IsRomInstalled(game, installDirectory);
        }

        public string GetInstallDirForDetection(Game game)
        {
            if (game.InstallDirectory.IsNullOrEmpty())
            {
                return string.Empty;
            }

            if (game.InstallDirectory.IndexOf('{') != 0)
            {
                var expandedDirectory = PlayniteApi.ExpandGameVariables(game, game.InstallDirectory);
                return RemoveInvalidPathChars(expandedDirectory).ToLower();
            }
            else
            {
                return RemoveInvalidPathChars(game.InstallDirectory).ToLower();
            }
        }

        private void AddIgnoreFeature(IEnumerable<Game> games)
        {
            var featureAddedCount = PlayniteUtilities.AddFeatureToGames(PlayniteApi, games, scanSkipFeatureName); ;
            PlayniteApi.Dialogs.ShowMessage(
                string.Format(ResourceProvider.GetString("LOCInstallation_Status_Updater_StatusUpdaterAddIgnoreFeatureMessage"),
                featureAddedCount.ToString()), "Installation Status Updater"
            );
        }

        private void RemoveIgnoreFeature(IEnumerable<Game> games)
        {
            var featureRemovedCount = PlayniteUtilities.RemoveFeatureFromGames(PlayniteApi, games, scanSkipFeatureName);
            PlayniteApi.Dialogs.ShowMessage(
                string.Format(ResourceProvider.GetString("LOCInstallation_Status_Updater_StatusUpdaterRemoveIgnoreFeatureMessage"), 
                featureRemovedCount.ToString()), "Installation Status Updater"
            );
        }

        private void UpdateInstallDirTags()
        {
            var progRes = PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                var drivesTagsDictionary = new Dictionary<string, Tag>();
                using (PlayniteApi.Database.BufferedUpdate())
                foreach (var game in PlayniteApi.Database.Games)
                {
                    var tagName = string.Empty;
                    if (!game.InstallDirectory.IsNullOrEmpty() && game.IsInstalled)
                    {
                        var s = new FileInfo(game.InstallDirectory);
                        var sourceDrive = Path.GetPathRoot(s.FullName).ToUpper();
                        tagName = $"{driveTagPrefix} {sourceDrive}";
                        if (!drivesTagsDictionary.ContainsKey(tagName))
                        {
                            var driveTag = PlayniteApi.Database.Tags.Add(tagName);
                            drivesTagsDictionary.Add(tagName, driveTag);
                        }

                        PlayniteUtilities.AddTagToGame(PlayniteApi, game, drivesTagsDictionary[tagName]);
                    }

                    if (!game.Tags.HasItems())
                    {
                        continue;
                    }

                    foreach (Tag tag in game.Tags.Where(x => x.Name.StartsWith(driveTagPrefix)))
                    {
                        if (!tagName.IsNullOrEmpty())
                        {
                            if (tag.Name != tagName)
                            {
                                PlayniteUtilities.RemoveTagFromGame(PlayniteApi, game, tag);
                            }
                        }
                        else
                        {
                            PlayniteUtilities.RemoveTagFromGame(PlayniteApi, game, tag);
                        }
                    }
                }
            }, new GlobalProgressOptions(ResourceProvider.GetString("LOCInstallation_Status_Updater_StatusUpdaterUpdatingTagsProgressMessage")));
        }

    }
}