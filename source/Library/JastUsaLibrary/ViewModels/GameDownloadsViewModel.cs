﻿using JastUsaLibrary.Models;
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

                var tickOldDate = DateTime.Now;
                long tickoldBytesReceived = 0;
                void progressChangedAction(DownloadProgressChangedEventArgs e)
                {
                    var tickNewDate = DateTime.Now;
                    var secondsElapsed = (tickNewDate - tickOldDate).TotalMilliseconds / 1000;

                    // Only update text if at least half a second has passed since last update
                    if (secondsElapsed < 0.5)
                    {
                        return;
                    }

                    // Calculate download speed per second
                    var tickNewBytesReceived = e.BytesReceived;

                    var tickBytesReceived = tickNewBytesReceived - tickoldBytesReceived;
                    tickOldDate = tickNewDate;
                    tickoldBytesReceived = tickNewBytesReceived;

                    var bytesPerSecond = tickBytesReceived / secondsElapsed;
                    var speedReadeable = GetBytesReadable((long)bytesPerSecond);

                    // Calculate remaining time
                    var bytesRemaining = e.TotalBytesToReceive - tickNewBytesReceived;
                    var remainingTimeSeconds = (bytesRemaining * secondsElapsed) / tickBytesReceived;
                    var timeRemainingReadable = GetTimeReadable(remainingTimeSeconds);

                    // Calculate current download percentage
                    double percentage = double.Parse(e.BytesReceived.ToString()) / double.Parse(e.TotalBytesToReceive.ToString()) * 100;
                    a.CurrentProgressValue = percentage;

                    // Update dialog text
                    a.Text = $"{progressTitle}\n\n" +
                            $"{fileName}\n\n" +
                            $"{string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderDownloadSizeProgress"), GetBytesReadable(e.BytesReceived), GetBytesReadable(e.TotalBytesToReceive))}\n\n" + // Download progress e.g. 5MB of 10MB
                            $"{string.Format("{0:0.00}", percentage)}/100.00%\n\n" + // Percentage e.g. 24.42/100%
                            $"{string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderDownloadTimeRemaining"), timeRemainingReadable)}\n\n" + //Time remaining e.g. 6 seconds left
                            $"{string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderDownloadSpeedPerSecond"), speedReadeable)}"; //Download speed e.g. 5MB/s
                }

                downloadSuccess = HttpDownloader.DownloadFile(url.ToString(), downloadPath, a.CancelToken, progressChangedAction);
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

        // Returns the human-readable file size for an arbitrary, 64-bit file size 
        // The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
        // From https://stackoverflow.com/a/11124118
        private string GetBytesReadable(long i)
        {
            // Get absolute value
            long absolute_i = (i < 0 ? -i : i);
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = i;
            }
            else
            {
                return i.ToString("0 B"); // Byte
            }

            // Divide by 1024 to get fractional value
            readable /= 1024;
            // Return formatted number with suffix
            return readable.ToString("0.000 ") + suffix;
        }
    }
}