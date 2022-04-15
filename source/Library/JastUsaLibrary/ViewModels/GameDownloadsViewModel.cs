using JastUsaLibrary.Models;
using JastUsaLibrary.Services;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using PluginsCommon.Web;
using System;
using System.Collections.Generic;
using System.IO;
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

        private JastUsaAccountClient accountClient;

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
        private readonly JastUsaLibrarySettingsViewModel settingsViewModel;
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
                if (!settingsViewModel.Settings.DownloadsPath.IsNullOrEmpty())
                {
                    var path = Path.Combine(settingsViewModel.Settings.DownloadsPath, game.GameId);
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

        public GameDownloadsViewModel(IPlayniteAPI playniteApi, Game game, GameTranslationsResponse gameTranslationsResponse, JastUsaAccountClient accountClient, JastUsaLibrarySettingsViewModel settingsViewModel)
        {
            GameTranslationsResponse = gameTranslationsResponse;
            this.playniteApi = playniteApi;
            this.settingsViewModel = settingsViewModel;
            Game = game;
            this.accountClient = accountClient;
            if (gameTranslationsResponse.GamePathLinks.HydraMember.Count > 0)
            {
                SelectedTabItemIndex = 0;
            }
            else if (gameTranslationsResponse.GamePatchLinks.HydraMember.Count > 0)
            {
                SelectedTabItemIndex = 1;
            }
            else if (gameTranslationsResponse.GameExtraLinks.HydraMember.Count > 0)
            {
                SelectedTabItemIndex = 2;
            }
        }

        public RelayCommand<HydraMember> GetAndOpenDownloadLinkCommand
        {
            get => new RelayCommand<HydraMember>((a) =>
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

        private void GetAndOpenDownloadLink(HydraMember downloadAsset)
        {
            var url = accountClient.GetAssetDownloadLinkAsync(downloadAsset.GameId, downloadAsset.GameLinkId);
            if (url != null)
            {
                //ProcessStarter.StartUrl(url);
                DownloadAssetLink(url);
            }
        }

        private void DownloadAssetLink(string url)
        {
            var fileName = Regex.Match(url, @"([^\/]+)(?=\?token=)").Groups[1].Value.UrlDecode();
            var progressTitle = ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderDownloadingLabel");
            var tempFileName = fileName + ".tmp";
            var downloadPath = Path.Combine(DownloadDirectory, tempFileName);
            var finalDownloadPath = Path.Combine(DownloadDirectory, fileName);
            if (FileSystem.FileExists(finalDownloadPath))
            {
                playniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderDownloadingFileAlreadyDl"), fileName));
                return;
            }

            var downloadSuccess = false;
            var downloadCanceled = false;
            var progressOptions = new GlobalProgressOptions(progressTitle, true);
            progressOptions.IsIndeterminate = false;
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

                downloadSuccess = HttpDownloader.DownloadFile(url, downloadPath, a.CancelToken, progressChangedAction);
                downloadCanceled = a.CancelToken.IsCancellationRequested;
            }, progressOptions);

            if (downloadSuccess)
            {
                FileSystem.MoveFile(downloadPath, finalDownloadPath);
                playniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderFileDownloadSuccessMessage"), finalDownloadPath));
            }
            else
            {
                FileSystem.DeleteFileSafe(downloadPath);
                if (!downloadCanceled) // Download failed
                {
                    playniteApi.Dialogs.ShowErrorMessage(ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderFileDownloadError"), "");
                }
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