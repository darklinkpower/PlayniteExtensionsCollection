using Microsoft.Win32;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using SteamCommon;
using SteamKit2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SteamClientLib = SteamCommon.SteamClient;

namespace SteamGameTransferUtility.ViewModels
{
    class TransferWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            var caller = name;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI PlayniteApi;
        private readonly string steamInstallationPath;
        private readonly string steamInstallationDirectory;
        private List<string> steamLibraries;
        private List<Game> selectedSteamGames;
        private string targetLibraryPath;
        private bool deleteSourceGame = false;
        private bool restartSteamIfNeeded = false;
        private bool isTargetLibrarySelected = false;

        public List<Game> SelectedSteamGames
        {
            get => selectedSteamGames;
            set
            {
                selectedSteamGames = value;
                OnPropertyChanged();
            }
        }

        public List<string> SteamLibraries
        {
            get => steamLibraries;
            set
            {
                steamLibraries = value;
                OnPropertyChanged();
            }
        }

        public string TargetLibraryPath
        {
            get => targetLibraryPath;
            set
            {
                targetLibraryPath = value;
                if (string.IsNullOrEmpty(targetLibraryPath))
                {
                    isTargetLibrarySelected = false;
                }
                else
                {
                    isTargetLibrarySelected = true;
                }
                OnPropertyChanged();
            }
        }

        public bool DeleteSourceGame
        {
            get => deleteSourceGame;
            set
            {
                deleteSourceGame = value;
                OnPropertyChanged();
            }
        }

        public bool RestartSteamIfNeeded
        {
            get => restartSteamIfNeeded;
            set
            {
                restartSteamIfNeeded = value;
                OnPropertyChanged();
            }
        }

        public TransferWindowViewModel(IPlayniteAPI api)
        {
            PlayniteApi = api;
            steamInstallationPath = SteamClientLib.GetSteamInstallationPath();
            steamInstallationDirectory = Path.GetDirectoryName(steamInstallationPath);
            steamLibraries = GetLibraryFolders();
            SelectedSteamGames = PlayniteApi.MainView.SelectedGames.Where(g => g.IsInstalled && Steam.IsGameSteamGame(g)).OrderBy(x => x.Name).ToList();
        }

        public RelayCommand<string> OpenLibraryCommand
        {
            get => new RelayCommand<string>((targetLibraryPath) =>
            {
                ProcessStarter.StartProcess(targetLibraryPath);
            }, (a) => isTargetLibrarySelected);
        }

        public RelayCommand<object> TransferGamesCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                IList items = (IList)a;
                var collection = items.Cast<Game>().ToList();
                TransferSteamGames(collection);
            }, (a) => isTargetLibrarySelected);
        }

        public RelayCommand TransferAllGamesCommand
        {
            get => new RelayCommand(() =>
            {
                TransferSteamGames(SelectedSteamGames);
            }, () => isTargetLibrarySelected);
        }

        internal static List<string> GetLibraryFolders(KeyValue foldersData)
        {
            var dbs = new List<string>();
            foreach (var child in foldersData.Children)
            {
                if (int.TryParse(child.Name, out int _))
                {
                    if (!string.IsNullOrEmpty(child.Value))
                    {
                        dbs.Add(child.Value);
                    }
                    else if (child.Children.HasItems())
                    {
                        var path = child.Children.FirstOrDefault(a => a.Name?.Equals("path", StringComparison.OrdinalIgnoreCase) == true);
                        if (!string.IsNullOrEmpty(path.Value))
                        {
                            dbs.Add(Path.Combine(path.Value, "steamapps"));
                        }
                    }
                }
            }

            return dbs;
        }

        private List<string> GetLibraryFolders()
        {
            var dbs = new List<string>();
            var configPath = Path.Combine(steamInstallationDirectory, "steamapps", "libraryfolders.vdf");
            if (!FileSystem.FileExists(configPath))
            {
                return dbs;
            }

            try
            {
                using (var fs = new FileStream(configPath, FileMode.Open, FileAccess.Read))
                {
                    var kv = new KeyValue();
                    kv.ReadAsText(fs);
                    foreach (var dir in GetLibraryFolders(kv))
                    {
                        if (Directory.Exists(dir))
                        {
                            dbs.Add(dir);
                        }
                        else
                        {
                            logger.Warn($"Found external Steam directory, but path doesn't exists: {dir}");
                        }
                    }
                }
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                logger.Error(e, "Failed to get additional Steam library folders.");
            }

            return dbs;
        }

        public void TransferSteamGames(List<Game> selectedGameItems)
        {
            if (selectedGameItems == null || selectedGameItems.Count == 0)
            {
                PlayniteApi.Dialogs.ShowErrorMessage(ResourceProvider.GetString("LOCSteam_Game_Transfer_Utility_DialogMessageNoSteamGamesSel"), "Steam Game Transfer Utility");
                return;
            }

            FileInfo t = new FileInfo(targetLibraryPath);
            var targetDrive = Path.GetPathRoot(t.FullName).ToLower();

            var copiedGamesCount = 0;
            var skippedGamesCount = 0;
            var deletedSourceFilesCount = 0;
            foreach (Game game in selectedGameItems)
            {
                logger.Info(string.Format("Processing game: \"{0}\"", game.Name));

                // Verify that source and target library are not in the same drive
                FileInfo s = new FileInfo(game.InstallDirectory);
                var sourceDrive = Path.GetPathRoot(s.FullName).ToLower();
                if (sourceDrive.Equals(targetDrive, StringComparison.OrdinalIgnoreCase))
                {
                    var errorMessage = string.Format("Source and target library are the same drive: \"{0}\"", sourceDrive);
                    logger.Warn(errorMessage);
                    skippedGamesCount++;
                    continue;
                }

                // Get steam source library that contains game
                var sourceLibraryPath = steamLibraries.FirstOrDefault(x => x.StartsWith(sourceDrive, StringComparison.InvariantCultureIgnoreCase));
                if (sourceLibraryPath == null)
                {
                    var errorMessage = string.Format(ResourceProvider.GetString("LOCSteam_Game_Transfer_Utility_SteamLibNotDetected"), game.Name, sourceDrive);
                    PlayniteApi.Dialogs.ShowErrorMessage(errorMessage, "Steam Game Transfer Utility");
                    logger.Warn(errorMessage);
                    skippedGamesCount++;
                    continue;
                }

                // Check if game source manifest exists
                var gameManifest = string.Format("appmanifest_{0}.acf", game.GameId);
                var sourceManifestPath = Path.Combine(sourceLibraryPath, gameManifest);
                if (!FileSystem.FileExists(sourceManifestPath))
                {
                    var errorMessage = string.Format(ResourceProvider.GetString("LOCSteam_Game_Transfer_Utility_ErrorMessageSourceManifestNotDetected"), game.Name, sourceManifestPath);
                    PlayniteApi.Dialogs.ShowErrorMessage(errorMessage, "Steam Game Transfer Utility");
                    logger.Warn(errorMessage);
                    skippedGamesCount++;
                    continue;
                }

                AcfReader acfReader = new AcfReader(sourceManifestPath);
                ACF_Struct sourceAcf = acfReader.ACFFileToStruct();

                // Check if game source directory exists
                var sourceGameDirectoryPath = Path.Combine(sourceLibraryPath, "common", GetAcfAppSubItem(sourceAcf, "installdir"));
                if (!Directory.Exists(sourceGameDirectoryPath))
                {
                    var errorMessage = string.Format(ResourceProvider.GetString("LOCSteam_Game_Transfer_Utility_ErrorMessageSourceDirectoryNotDetected"), game.Name, sourceGameDirectoryPath);
                    PlayniteApi.Dialogs.ShowErrorMessage(errorMessage, "Steam Game Transfer Utility");
                    logger.Warn(errorMessage);
                    skippedGamesCount++;
                    continue;
                }

                // Check if game manifest already exists in target library
                var targetManifestPath = Path.Combine(targetLibraryPath, gameManifest);
                if (FileSystem.FileExists(targetManifestPath))
                {
                    var sourceBuildId = int.Parse(GetAcfAppSubItem(sourceAcf, "buildid"));
                    var targetBuildId = int.Parse(GetAcfAppSubItem(targetManifestPath, "buildid"));

                    if (sourceBuildId <= targetBuildId)
                    {
                        var errorMessage = string.Format("Game: {0}. Equal or greater BuldId. Source BuildId {1} - Target BuildId {2}", game.Name, sourceBuildId.ToString(), targetBuildId.ToString());
                        logger.Info(errorMessage);
                        skippedGamesCount++;

                        if (deleteSourceGame)
                        {
                            FileSystem.DeleteDirectory(sourceGameDirectoryPath, true);
                            FileSystem.DeleteFileSafe(sourceManifestPath);
                            logger.Info($"Deleted source files of game {game.Name} in {sourceGameDirectoryPath}");
                            deletedSourceFilesCount++;
                        }
                        continue;
                    }
                }

                //Calculate size of source game directory
                var sourceDirectorySize = CalculateSize(sourceGameDirectoryPath);
                var progRes = PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
                {
                    string targetGameDirectoryPath = Path.Combine(targetLibraryPath, "common", GetAcfAppSubItem(sourceAcf, "installdir"));
                    if (Directory.Exists(targetGameDirectoryPath))
                    {
                        FileSystem.DeleteDirectory(targetGameDirectoryPath, true);
                        logger.Info(string.Format("Deleted directory: {0}", targetGameDirectoryPath));
                    }

                    FileSystem.CopyDirectory(sourceGameDirectoryPath, targetGameDirectoryPath, true);
                    logger.Info(string.Format("Game copied: {0}. sourceDirName: {1}, destDirName: {2}", game.Name, sourceGameDirectoryPath, targetGameDirectoryPath));
                    FileSystem.CopyFile(sourceManifestPath, targetManifestPath, true);
                    logger.Info(string.Format("Game manifest copied: {0}. sourceDirName: {1}, destDirName: {2}", game.Name, sourceManifestPath, targetManifestPath));
                    copiedGamesCount++;

                    if (deleteSourceGame)
                    {
                        FileSystem.DeleteDirectory(sourceGameDirectoryPath, true);
                        FileSystem.DeleteFileSafe(sourceManifestPath);
                        logger.Info("Deleted source files");
                        deletedSourceFilesCount++;
                    }

                }, new GlobalProgressOptions(string.Format(ResourceProvider.GetString("LOCSteam_Game_Transfer_Utility_ProgressDialogProcessingGame"), game.Name, sourceDirectorySize)));
            }

            var results = string.Format(ResourceProvider.GetString("LOCSteam_Game_Transfer_Utility_ResultsDialogMessage"), copiedGamesCount.ToString(), skippedGamesCount.ToString(), deletedSourceFilesCount.ToString());
            PlayniteApi.Dialogs.ShowMessage(results, "Steam Game Transfer Utility");

            if (restartSteamIfNeeded == true && (copiedGamesCount > 0 || deletedSourceFilesCount > 0))
            {
                SteamClientLib.StartSteam(true);
            }
        }

        public string GetAcfAppSubItem(string acfPath, string subItemName)
        {
            AcfReader acfReader = new AcfReader(acfPath);
            ACF_Struct acfStruct = acfReader.ACFFileToStruct();
            return acfStruct.SubACF["AppState"].SubItems[subItemName];
        }

        public string GetAcfAppSubItem(ACF_Struct acfStruct, string subItemName)
        {
            return acfStruct.SubACF["AppState"].SubItems[subItemName];
        }

        public string CalculateSize(string directory)
        {
            var task = Task.Run(() =>
            {
                return FileSystem.GetDirectorySize(directory);
            });

            var isCompletedSuccessfully = task.Wait(TimeSpan.FromMilliseconds(9000));
            if (isCompletedSuccessfully)
            {
                if (task.Result == 0)
                {
                    return "Unknown Size";
                }
                else
                {
                    return FormatBytes(task.Result);
                }
            }
            else
            {
                return "Unknown Size";
            }
        }

        private static string FormatBytes(long bytes)
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = bytes;
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            return string.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
        }

    }
}