using JastUsaLibrary.Models;
using JastUsaLibrary.Services;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using WebCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using WebCommon.Models;

namespace JastUsaLibrary.ViewModels
{
    public class GameDownloadsViewModel : ObservableObject
    {

        private readonly JastUsaAccountClient accountClient;

        private int selectedTabItemIndex = 0;
        public int SelectedTabItemIndex
        {
            get => selectedTabItemIndex;
            set
            {
                selectedTabItemIndex = value;
                OnPropertyChanged();
            }
        }

        private GameTranslationsResponse gameTranslationsResponse;
        public GameTranslationsResponse GameTranslationsResponse
        {
            get => gameTranslationsResponse;
            set
            {
                gameTranslationsResponse = value;
                OnPropertyChanged();
            }
        }

        private readonly IPlayniteAPI playniteApi;

        private JastUsaLibrarySettingsViewModel settingsViewModel;
        public JastUsaLibrarySettingsViewModel SettingsViewModel
        {
            get => settingsViewModel;
            set
            {
                settingsViewModel = value;
                OnPropertyChanged();
            }
        }


        private Game game;
        public Game Game
        {
            get => game;
            set
            {
                game = value;
                OnPropertyChanged();
            }
        }

        public string DownloadDirectory
        {
            get
            {
                if (!SettingsViewModel.Settings.DownloadsPath.IsNullOrEmpty())
                {
                    var path = Path.Combine(SettingsViewModel.Settings.DownloadsPath, game.GameId);
                    FileSystem.CreateDirectory(path);
                    return path;
                }
                else
                {
                    // Download to desktop if user has not configured a download directory
                    var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "JAST_USA", game.GameId);
                    FileSystem.CreateDirectory(path);
                    return path;
                }
            }
        }

        private static readonly ILogger logger = LogManager.GetLogger();
        public GameDownloadsViewModel(IPlayniteAPI playniteApi, Game game, GameTranslationsResponse gameTranslationsResponse, JastUsaAccountClient accountClient, JastUsaLibrarySettingsViewModel settingsViewModel)
        {
            GameTranslationsResponse = gameTranslationsResponse;
            this.playniteApi = playniteApi;
            this.SettingsViewModel = settingsViewModel;
            Game = game;
            this.accountClient = accountClient;
            if (gameTranslationsResponse.GamePathLinks.Count > 0)
            {
                SelectedTabItemIndex = 0;
            }
            else if (gameTranslationsResponse.GamePatchLinks.Count > 0)
            {
                SelectedTabItemIndex = 1;
            }
            else if (gameTranslationsResponse.GameExtraLinks.Count > 0)
            {
                SelectedTabItemIndex = 2;
            }
        }

        public RelayCommand<GameLink> GetAndOpenDownloadLinkCommand
        {
            get => new RelayCommand<GameLink>((a) =>
            {
                GetAndOpenDownloadLink(a);
            });
        }

        public RelayCommand OpenDownloadsDirectory
        {
            get => new RelayCommand(() =>
            {
                ProcessStarter.StartProcess(DownloadDirectory);
            });
        }

        private void GetAndOpenDownloadLink(GameLink downloadAsset)
        {
            var url = accountClient.GetAssetDownloadLinkAsync(downloadAsset.GameId, downloadAsset.GameLinkId);
            if (url != null)
            {
                DownloadAssetLink(url);
            }
        }

        private void DownloadAssetLink(Uri url)
        {
            var fileName = Path.GetFileName(url.LocalPath);
            var progressTitle = ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderDownloadingLabel");
            var tempFileName = fileName + ".tmp";
            var downloadPath = Path.Combine(DownloadDirectory, tempFileName);
            var finalDownloadPath = Path.Combine(DownloadDirectory, fileName);

            if (FileSystem.FileExists(finalDownloadPath))
            {
                playniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderDownloadingFileAlreadyDl"), fileName));
                return;
            }

            // For zip files, detect if the extracted files are present and ask user if they want to download anyway
            if (Path.GetExtension(fileName).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                var extractDirectory = Path.Combine(DownloadDirectory, Path.GetFileNameWithoutExtension(fileName));
                var targetDirectoryExists = FileSystem.DirectoryExists(extractDirectory);
                if (targetDirectoryExists)
                {
                    var shouldDownloadChoice = playniteApi.Dialogs.ShowMessage(
                        string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderExtractedDirectoryDetected"), fileName, extractDirectory),
                        ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderWindowTitle"),
                        MessageBoxButton.YesNo);
                    if (shouldDownloadChoice != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
            }

            var downloadSuccess = false;
            var downloadCanceled = false;
            var progressOptions = new GlobalProgressOptions(progressTitle, true)
            {
                IsIndeterminate = false
            };

            playniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                a.ProgressMaxValue = 100;
                //downloadSuccess = HttpDownloader.DownloadFile(url.ToString(), downloadPath, a.CancelToken, progressChangedAction);
                var progress = new Progress<DownloadProgressReport>(report =>
                {
                    var reportLines = new List<string>
                    {
                        progressTitle,
                        fileName,
                        string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderDownloadSizeProgress"),
                            report.FormattedBytesReceived, report.FormattedTotalBytesToReceive), // Download progress e.g. 5MB of 10MB
                        string.Format("{0:0.00}/100.00%", report.ProgressPercentage),// Percentage e.g. 24.42/100%
                        string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderDownloadTimeRemaining"),
                            GetTimeReadable(report.TimeRemaining.TotalSeconds)), //Time remaining e.g. 6 seconds left
                        report.FormattedDownloadSpeedPerSecond //Download speed e.g. 5MB/s
                    };

                    a.Text = string.Join("\n\n", reportLines);
                    a.CurrentProgressValue = report.ProgressPercentage;
                });

                var request = HttpDownloader.GetRequestBuilder()
                    .WithUrl(url.ToString())
                    .WithDownloadTo(downloadPath)
                    .WithCancellationToken(a.CancelToken)
                    .WithProgressReporter(progress);

                var result = request.DownloadFile();

                downloadSuccess = result.IsSuccessful;
                downloadCanceled = a.CancelToken.IsCancellationRequested;
            }, progressOptions);

            if (!downloadSuccess)
            {
                FileSystem.DeleteFileSafe(downloadPath);
                if (!downloadCanceled) // Download failed
                {
                    playniteApi.Dialogs.ShowErrorMessage(ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderFileDownloadError"), "");
                }

                return;
            }

            FileSystem.MoveFile(downloadPath, finalDownloadPath);
            if (SettingsViewModel.Settings.ExtractDownloadedZips && Path.GetExtension(finalDownloadPath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                var destinationDirectory = Path.Combine(Path.GetDirectoryName(finalDownloadPath), Path.GetFileNameWithoutExtension(finalDownloadPath));
                ExtractZip(finalDownloadPath, destinationDirectory, SettingsViewModel.Settings.DeleteDownloadedZips);
            }

            playniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderFileDownloadSuccessMessage"), finalDownloadPath));
        }

        private void ExtractZip(string filePath, string destinationDirectory, bool deleteSourceZip)
        {
            // If directory exists, don't extract to not replace any data
            if (FileSystem.DirectoryExists(destinationDirectory))
            {
                logger.Debug($"Zip extraction aborted because extraction directory {destinationDirectory} already existed");
                return;
            }

            try
            {
                playniteApi.Dialogs.ActivateGlobalProgress((_) =>
                {
                    logger.Info($"Decompressing database zip file {filePath} to {destinationDirectory}. Delete source file; {deleteSourceZip}...");
                    FileSystem.CreateDirectory(destinationDirectory);
                    ZipFile.ExtractToDirectory(filePath, destinationDirectory);
                    logger.Info("Decompressed database zip file");

                    if (deleteSourceZip)
                    {
                        FileSystem.DeleteFileSafe(filePath);
                    }
                }, new GlobalProgressOptions(string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderExtractingZip"), Path.GetFileName(filePath), destinationDirectory)));
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error while decompressing file {filePath}");
                playniteApi.Dialogs.ShowErrorMessage(string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderExtractZipError"), filePath), "");
            }
        }

        private string GetTimeReadable(double timeSeconds)
        {
            timeSeconds = Math.Ceiling(timeSeconds);

            if (timeSeconds > 3600) //hours
            {
                return string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderTimeHoursMinsFormat"), timeSeconds / 3600, timeSeconds % 3600);
            }
            if (timeSeconds > 60) // Minutes
            {
                return string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderTimeMinsSecondsFormat"), timeSeconds / 60, timeSeconds % 60);
            }
            else // Seconds
            {
                return string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderTimeSecondsFormat"), timeSeconds);
            }
        }
    }
}