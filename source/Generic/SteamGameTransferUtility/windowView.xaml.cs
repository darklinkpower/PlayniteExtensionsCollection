using Microsoft.Win32;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using SteamKit2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace SteamGameTransferUtility
{
    /// <summary>
    /// Interaction logic for WindowView.xaml
    /// </summary>
    public partial class WindowView : UserControl
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        public IPlayniteAPI PlayniteApi { get; set; } = null;
        private readonly string steamInstallationDirectory;

        public WindowView()
        {
            InitializeComponent();

            steamInstallationDirectory = GetSteamInstallationPath();
            List<string> steamLibraries = GetLibraryFolders();
            cmbTargetLibrarySelection.ItemsSource = steamLibraries;
        }

        private void BtnOpenLibrary_Click(object sender, RoutedEventArgs e)
        {
            string targetLibraryPath = cmbTargetLibrarySelection.SelectedValue.ToString();
            System.Diagnostics.Process.Start(targetLibraryPath);
        }

        private void BtnProcess_Click(object sender, RoutedEventArgs e)
        {
            string targetLibraryPath = cmbTargetLibrarySelection.SelectedValue.ToString();
            List<string> steamLibraries = cmbTargetLibrarySelection.Items.OfType<string>().ToList();
            bool deleteSourceGame = cbDeleteSourceGame.IsChecked ?? true;
            bool restartSteam = cbRestartSteam.IsChecked ?? true;
            TransferSteamGames(steamLibraries, targetLibraryPath, deleteSourceGame, restartSteam);
        }

        public string GetSteamInstallationPath()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
            {
                if (key?.GetValueNames().Contains("SteamPath") == true)
                {
                    return key.GetValue("SteamPath")?.ToString().Replace('/', '\\') ?? "c:\\program files (x86)\\steam";
                }
            }
            return "c:\\program files (x86)\\steam";
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
            if (!File.Exists(configPath))
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

        public void TransferSteamGames(List<string> steamLibraries, string targetLibraryPath, bool deleteSourceGame, bool restartSteam)
        {
            var gameDatabase = PlayniteApi.MainView.SelectedGames.Where(g => g.PluginId == BuiltinExtensions.GetIdFromExtension(BuiltinExtension.SteamLibrary)).Where(g => g.IsInstalled == true);
            if (gameDatabase.Count() == 0)
            {
                PlayniteApi.Dialogs.ShowErrorMessage("There are no installed Steam games selected", "Steam Game Transfer Utility");
                return;
            }
            
            FileInfo t = new FileInfo(targetLibraryPath);
            string targetDrive = System.IO.Path.GetPathRoot(t.FullName).ToLower();

            int copiedGamesCount = 0;
            int skippedGamesCount = 0;
            int deletedSourceFilesCount = 0;
            foreach (Game game in gameDatabase)
            {
                logger.Info(string.Format("Processing game: \"{0}\"", game.Name));

                // Verify that source and target library are not in the same drive
                FileInfo s = new FileInfo(game.InstallDirectory);
                string sourceDrive = System.IO.Path.GetPathRoot(s.FullName).ToLower();
                if (sourceDrive == targetDrive)
                {
                    string errorMessage = string.Format("Source and target library are the same drive: \"{0}\"", sourceDrive);
                    logger.Warn(errorMessage);
                    skippedGamesCount++;
                    continue;
                }
                
                // Get steam source library that contains game
                string sourceLibraryPath = steamLibraries.Where(x => x.StartsWith(sourceDrive, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (sourceLibraryPath == null)
                {
                    string errorMessage = string.Format("Game: {0}. Error: Steam library on drive {1} not detected", game.Name, sourceDrive);
                    PlayniteApi.Dialogs.ShowErrorMessage(errorMessage, "Steam Game Transfer Utility");
                    logger.Warn(errorMessage);
                    skippedGamesCount++;
                    continue;
                }
                
                // Check if game source manifest exists
                string gameManifest = string.Format("appmanifest_{0}.acf", game.GameId);
                string sourceManifestPath = System.IO.Path.Combine(sourceLibraryPath, gameManifest);
                if (!File.Exists(sourceManifestPath))
                {
                    string errorMessage = string.Format("Game: {0}. Error: Source manifest doesn't exist in {1}" +
                        "\n\nThis issue can be happen if you used this utility on this game and you have not restarted Steam" +
                        " and updated your Playnite library to reflect the changes", game.Name, sourceManifestPath);
                    PlayniteApi.Dialogs.ShowErrorMessage(errorMessage, "Steam Game Transfer Utility");
                    logger.Warn(errorMessage);
                    skippedGamesCount++;
                    continue;
                }

                // Check if game source directory exists
                string sourceGameDirectoryPath = System.IO.Path.Combine(sourceLibraryPath, "common", GetAcfAppSubItem(sourceManifestPath, "installdir"));
                if (!Directory.Exists(sourceGameDirectoryPath))
                {
                    string errorMessage = string.Format("Game: {0}. Error: Source directory doesn't exist in {1}" +
                        "\n\nThis issue can be happen if you used this utility on this game and you have not restarted Steam" +
                        " and updated your Playnite library to reflect the changes", game.Name, sourceGameDirectoryPath);
                    PlayniteApi.Dialogs.ShowErrorMessage(errorMessage, "Steam Game Transfer Utility");
                    logger.Warn(errorMessage);
                    skippedGamesCount++;
                    continue;
                }

                // Check if game manifest already exists in target library
                string targetManifestPath = System.IO.Path.Combine(targetLibraryPath, gameManifest);
                if (File.Exists(targetManifestPath))
                {
                    int sourceBuildId = Int32.Parse(GetAcfAppSubItem(sourceManifestPath, "buildid"));
                    int targetBuildId = Int32.Parse(GetAcfAppSubItem(targetManifestPath, "buildid"));

                    if (sourceBuildId <= targetBuildId)
                    {
                        string errorMessage = string.Format("Game: {0}. Equal or greater BuldId. Source BuildId {1} - Target BuildId {2}", game.Name, sourceBuildId.ToString(), targetBuildId.ToString());
                        logger.Info(errorMessage);
                        skippedGamesCount++;

                        if (deleteSourceGame == true)
                        {
                            Directory.Delete(sourceGameDirectoryPath, true);
                            File.Delete(sourceManifestPath);
                            logger.Info("Deleted source files");
                            deletedSourceFilesCount++;
                        }
                        continue;
                    }
                }

                //Calculate size of source game directory
                string sourceDirectorySize = CalculateSize(sourceGameDirectoryPath);
                
                var progRes = PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
                {
                    string targetGameDirectoryPath = System.IO.Path.Combine(targetLibraryPath, "common", GetAcfAppSubItem(sourceManifestPath, "installdir"));
                    if (Directory.Exists(targetGameDirectoryPath))
                    {
                        Directory.Delete(targetGameDirectoryPath, true);
                        logger.Info(string.Format("Deleted directory: {0}", targetGameDirectoryPath));
                    }

                    DirectoryCopy(sourceGameDirectoryPath, targetGameDirectoryPath, true);
                    logger.Info(string.Format("Game copied: {0}. sourceDirName: {1}, destDirName: {2}", game.Name, sourceGameDirectoryPath, targetGameDirectoryPath));
                    File.Copy(sourceManifestPath, targetManifestPath, true);
                    logger.Info(string.Format("Game manifest copied: {0}. sourceDirName: {1}, destDirName: {2}", game.Name, sourceManifestPath, targetManifestPath));
                    copiedGamesCount++;

                    if (deleteSourceGame == true)
                    {
                        Directory.Delete(sourceGameDirectoryPath, true);
                        File.Delete(sourceManifestPath);
                        logger.Info("Deleted source files");
                        deletedSourceFilesCount++;
                    }

                }, new GlobalProgressOptions(string.Format("Processing game: {0}\n\nGame size: {1}", game.Name, sourceDirectorySize)));
            }

            string results = string.Format("Finished.\n\nCopied games: {0}\nSkipped games: {1}", copiedGamesCount.ToString(), skippedGamesCount.ToString());
            if (deleteSourceGame == true)
            {
                results += string.Format("\nDeleted source games: {0}", deletedSourceFilesCount.ToString());
            }
            if (copiedGamesCount > 0 || deletedSourceFilesCount > 0)
            {
                results += "\n\nUpdate your Playnite library after restarting Steam to reflect the changes";
            }
            PlayniteApi.Dialogs.ShowMessage(results, "Steam Game Transfer Utility");

            if (restartSteam == true && (copiedGamesCount > 0 || deletedSourceFilesCount > 0))
            {
                RestartSteam();
            }
        }

        public string GetAcfAppSubItem(string acfPath, string subItemName)
        {
            AcfReader acfReader = new AcfReader(acfPath);
            ACF_Struct acfStruct = acfReader.ACFFileToStruct();
            return acfStruct.SubACF["AppState"].SubItems[subItemName];
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = System.IO.Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = System.IO.Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }

        public string CalculateSize(string directory)
        {
            var task = Task.Run(() =>
            {
                return CalculateSizeTask(new DirectoryInfo(directory));
            });

            bool isCompletedSuccessfully = task.Wait(TimeSpan.FromMilliseconds(9000));

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

        private static long CalculateSizeTask(DirectoryInfo dir)
        {
            try
            {
                long size = 0;
                // Add file sizes.
                FileInfo[] fis = dir.GetFiles();
                foreach (FileInfo fi in fis)
                {
                    size += fi.Length;
                }
                
                // Add subdirectory sizes.
                DirectoryInfo[] dis = dir.GetDirectories();
                foreach (DirectoryInfo di in dis)
                {
                    size += CalculateSizeTask(di);
                }
                return size;
            }
            catch
            {
                return 0;
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

        private async void RestartSteam()
        {
            string steamInstallationPath = System.IO.Path.Combine(GetSteamInstallationPath(), "steam.exe");
            if (!File.Exists(steamInstallationPath))
            {
                logger.Error(string.Format("Steam executable not detected in path \"{0}\"", steamInstallationPath));
                return;
            }
            bool isSteamRunning = GetIsSteamRunning();
            if (isSteamRunning == true)
            {
                Process.Start(steamInstallationPath, "-shutdown");
                logger.Info("Steam detected running. Closing via command line.");
                for (int i = 0; i < 8; i++)
                {
                    isSteamRunning = GetIsSteamRunning();
                    await PutTaskDelay(2000);
                    if (isSteamRunning == true)
                    {
                        logger.Info("Steam detected running.");
                    }
                    else
                    {
                        logger.Info("Steam has closed.");
                        break;
                    }
                }
            }
            if (isSteamRunning == false)
            {
                Process.Start(steamInstallationPath);
                logger.Info("Steam started.");
            }
        }

        async Task PutTaskDelay(int delayTime)
        {
            await Task.Delay(delayTime);
        }

        public bool GetIsSteamRunning()
        {
            Process[] processes = Process.GetProcessesByName("Steam");
            if (processes.Length > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
