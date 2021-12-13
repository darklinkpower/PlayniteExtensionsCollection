using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
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
        private static readonly Regex driveRegex = new Regex(@"^\w:\\", RegexOptions.Compiled);
        private static readonly Regex installDirVarRegex = new Regex(@"{InstallDir}", RegexOptions.Compiled);
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
            mainWindow = Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w.Title.Equals("Playnite", StringComparison.InvariantCultureIgnoreCase));
            if (mainWindow != null)
            {
                windowInterop = new WindowInteropHelper(mainWindow);
                mainWindowHandle = windowInterop.Handle;
                source = HwndSource.FromHwnd(mainWindowHandle);
                source.AddHook(GlobalHotkeyCallback);
            }
            else
            {
                logger.Error("Could not find Playnite main window.");
            }

            if (settings.Settings.UpdateOnStartup == true)
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
                        logger.Info("Started timer from DBT_DEVICEARRIVAL event");
                        timer.Start();
                        handled = true;
                        break;
                    //case DBT_DEVICEREMOVEPENDING:
                    //    break;
                    case DBT_DEVICEREMOVECOMPLETE:
                        logger.Info("Started timer from DBT_DEVICEREMOVECOMPLETE event");
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

        public void SetDirWatchers()
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

        public void WatcherEventHandler(string invokerPath)
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
                    Description = ResourceProvider.GetString("LOCInstallation_Status_Updater_MenuAddIgnoreFeatureDescription"),
                    MenuSection = "@Installation Status Updater",
                    Action = a => {
                        AddIgnoreFeature();
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCInstallation_Status_Updater_MenuRemoveIgnoreFeatureDescription"),
                    MenuSection = "@Installation Status Updater",
                    Action = a => {
                        RemoveIgnoreFeature();
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

        public bool DetectIsRomInstalled(GameRom rom, string installDirectory)
        {
            if (string.IsNullOrEmpty(rom.Path))
            {
                return false;
            }

            if (driveRegex.IsMatch(rom.Path))
            {
                return File.Exists(rom.Path);
            }

            string romFullPath = rom.Path;
            if (!string.IsNullOrEmpty(installDirectory))
            {
                if (installDirVarRegex.IsMatch(rom.Path))
                {
                    romFullPath = rom.Path.Replace("{InstallDir}", installDirectory);
                }
                else
                {
                    romFullPath = Path.Combine(installDirectory, rom.Path);
                }
            }

            return File.Exists(romFullPath);
        }

        public bool DetectIsRomInstalled(Game game, string installDirectory)
        {
            if (game.Roms == null)
            {
                return false;
            }
            if (game.Roms.Count == 0)
            {
                return false;
            }
            if (game.Roms.Count > 1 && settings.Settings.UseOnlyFirstRomDetection == false)
            {
                foreach (GameRom rom in game.Roms)
                {
                    if (DetectIsRomInstalled(rom, installDirectory))
                    {
                        return true;
                    }
                }
            }
            else
            {
                return DetectIsRomInstalled(game.Roms[0], installDirectory);
            }

            return false;
        }

        public bool DetectIsFileActionInstalled(GameAction gameAction, string installDirectory)
        {
            if (string.IsNullOrEmpty(gameAction.Path))
            {
                return false;
            }

            if (driveRegex.IsMatch(gameAction.Path))
            {
                return File.Exists(gameAction.Path);
            }

            var fullfilePath = gameAction.Path;
            if (!string.IsNullOrEmpty(installDirectory))
            {
                if (installDirVarRegex.IsMatch(gameAction.Path))
                {
                    fullfilePath = gameAction.Path.Replace("{InstallDir}", installDirectory);
                }
                else
                {
                    fullfilePath = Path.Combine(installDirectory, gameAction.Path);
                }
            }

            return File.Exists(fullfilePath);
        }

        public bool DetectIsAnyActionInstalled(Game game, string installDirectory)
        {
            foreach (GameAction gameAction in game.GameActions)
            {
                if (gameAction.IsPlayAction == false && settings.Settings.OnlyUsePlayActionGameActions == true)
                {
                    continue;
                }
                
                if (gameAction.Type == GameActionType.URL)
                {
                    if (settings.Settings.UrlActionIsInstalled == true)
                    {
                        return true;
                    }
                }
                else if (gameAction.Type == GameActionType.Script)
                {
                    if (settings.Settings.ScriptActionIsInstalled == true)
                    {
                        return true;
                    }
                }
                else if (gameAction.Type == GameActionType.File)
                {
                    if (DetectIsFileActionInstalled(gameAction, installDirectory))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void DetectInstallationStatus(bool showResultsDialog)
        {
            var gameCollection = PlayniteApi.Database.Games;
            string skipFeatureName = "[Status Updater] Ignore";
            int markedInstalled = 0;
            int markedUninstalled = 0;
            foreach (Game game in gameCollection)
            {
                if (game.IncludeLibraryPluginAction == true && settings.Settings.SkipHandledByPlugin == true && game.PluginId != Guid.Empty)
                {
                    continue;
                }
                
                if (game.Features != null && game.Features.Any(x => x.Name == skipFeatureName))
                {
                    continue;
                }
                
                var isInstalled = false;
                var installDirectory = string.Empty;
                if (!string.IsNullOrEmpty(game.InstallDirectory))
                {
                    installDirectory = game.InstallDirectory.ToLower();
                }

                if (game.GameActions != null && game.GameActions.Count > 0)
                {
                    isInstalled = DetectIsAnyActionInstalled(game, installDirectory);
                }

                if (isInstalled == false)
                {
                    isInstalled = DetectIsRomInstalled(game, installDirectory);
                }

                if (game.IsInstalled == true && isInstalled == false)
                {
                    game.IsInstalled = false;
                    PlayniteApi.Database.Games.Update(game);
                    markedUninstalled++;
                    logger.Info(string.Format("Game: {0} marked as uninstalled", game.Name));
                }
                else if (game.IsInstalled == false && isInstalled == true)
                {
                    game.IsInstalled = true;
                    PlayniteApi.Database.Games.Update(game);
                    markedInstalled++;
                    logger.Info(string.Format("Game: {0} marked as installed", game.Name));
                }
            }

            if (showResultsDialog == true)
            {
                PlayniteApi.Dialogs.ShowMessage(
                    string.Format(ResourceProvider.GetString("LOCInstallation_Status_Updater_StatusUpdaterResultsMessage"), 
                    markedUninstalled.ToString(), 
                    markedInstalled.ToString()), "Installation Status Updater"
                );
            }
        }

        public void AddIgnoreFeature()
        {
            string skipFeatureName = "[Status Updater] Ignore";
            GameFeature feature = PlayniteApi.Database.Features.Add(skipFeatureName);
            int featureAddedCount = 0;
            var gameCollection = PlayniteApi.MainView.SelectedGames;
            foreach (Game game in gameCollection)
            {
                if (game.FeatureIds == null)
                {
                    game.FeatureIds = new List<Guid> { feature.Id };
                    continue;
                }

                var containsFeature = false;
                foreach (Guid featureId in game.FeatureIds)
                {
                    if (featureId == feature.Id)
                    {
                        containsFeature = true;
                        break;
                    }
                }

                if (containsFeature == false)
                {
                    game.FeatureIds.Add(feature.Id);
                    PlayniteApi.Database.Games.Update(game);
                    featureAddedCount++;
                    logger.Info(string.Format("Game: {0} Added ignore feature", game.Name));
                }
            }

            PlayniteApi.Dialogs.ShowMessage(
                string.Format(ResourceProvider.GetString("LOCInstallation_Status_Updater_StatusUpdaterAddIgnoreFeatureMessage"),
                featureAddedCount.ToString()), "Installation Status Updater"
            );
        }

        public void RemoveIgnoreFeature()
        {
            string skipFeatureName = "[Status Updater] Ignore";
            GameFeature feature = PlayniteApi.Database.Features.Add(skipFeatureName);
            int featureRemovedCount = 0;
            var gameCollection = PlayniteApi.MainView.SelectedGames;
            foreach (Game game in gameCollection)
            {
                if (game.FeatureIds == null)
                {
                    continue;
                }

                foreach (Guid featureId in game.FeatureIds)
                {
                    if (featureId == feature.Id)
                    {
                        game.FeatureIds.Remove(feature.Id);
                        PlayniteApi.Database.Games.Update(game);
                        featureRemovedCount++;
                        logger.Info(string.Format("Game: {0} Removed ignore feature", game.Name));
                        break;
                    }
                }
            }

            PlayniteApi.Dialogs.ShowMessage(
                string.Format(ResourceProvider.GetString("LOCInstallation_Status_Updater_StatusUpdaterRemoveIgnoreFeatureMessage"), 
                featureRemovedCount.ToString()), "Installation Status Updater"
            );
        }

        public void UpdateInstallDirTags()
        {
            var progRes = PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                var gameDatabase = PlayniteApi.Database.Games;
                string driveTagPrefix = "[Install Drive]";
                foreach (Game game in gameDatabase)
                {
                    string tagName = string.Empty;
                    if (!string.IsNullOrEmpty(game.InstallDirectory) && game.IsInstalled == true)
                    {
                        FileInfo s = new FileInfo(game.InstallDirectory);
                        string sourceDrive = System.IO.Path.GetPathRoot(s.FullName).ToUpper();
                        tagName = string.Format("{0} {1}", driveTagPrefix, sourceDrive);
                        Tag driveTag = PlayniteApi.Database.Tags.Add(tagName);
                        AddTag(game, driveTag);
                    }

                    if (game.Tags == null)
                    {
                        continue;
                    }

                    foreach (Tag tag in game.Tags.Where(x => x.Name.StartsWith(driveTagPrefix)))
                    {
                        if (!string.IsNullOrEmpty(tagName))
                        {
                            if (tag.Name != tagName)
                            {
                                RemoveTag(game, tag);
                            }
                        }
                        else
                        {
                            RemoveTag(game, tag);
                        }
                    }
                }
            }, new GlobalProgressOptions(ResourceProvider.GetString("LOCInstallation_Status_Updater_StatusUpdaterUpdatingTagsProgressMessage")));
        }

        public bool RemoveTag(Game game, Tag tag)
        {
            if (game.TagIds != null)
            {
                if (game.TagIds.Contains(tag.Id))
                {
                    game.TagIds.Remove(tag.Id);
                    PlayniteApi.Database.Games.Update(game);
                    bool tagRemoved = true;
                    return tagRemoved;
                }
                else
                {
                    bool tagRemoved = false;
                    return tagRemoved;
                }
            }
            else
            {
                bool tagRemoved = false;
                return tagRemoved;
            }
        }

        public bool AddTag(Game game, Tag tag)
        {
            if (game.TagIds == null)
            {
                game.TagIds = new List<Guid> { tag.Id };
                PlayniteApi.Database.Games.Update(game);
                bool tagAdded = true;
                return tagAdded;
            }
            else if (game.TagIds.Contains(tag.Id) == false)
            {
                game.TagIds.AddMissing(tag.Id);
                PlayniteApi.Database.Games.Update(game);
                bool tagAdded = true;
                return tagAdded;
            }
            else
            {
                bool tagAdded = false;
                return tagAdded;
            }
        }
    }
}