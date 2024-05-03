using JastUsaLibrary.Models;
using JastUsaLibrary.Services;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WebCommon;
using WebCommon.Builders;
using WebCommon.HttpRequestClient;
using WebCommon.HttpRequestClient.Events;

namespace JastUsaLibrary.ViewModels
{
    public enum DownloadStatus
    {
        Queued,
        Downloading,
        Paused,
        Completed,
        Failed,
        Canceled
    }

    //public abstract class DownloadItem
    //{
    //    private readonly Func<Task<string>> _urlResolver;
    //    private readonly Func<Task<HttpRequestClient>> _requestClientResolver;
    //    private readonly bool _refreshUrlOnStart;

    //    private HttpRequestClient _httpRequestClient;
    //    public string Url { get; private set; }

    //    public DownloadStatus Status { get; protected set; }

    //    protected DownloadItem(Func<Task<string>> urlResolver, Func<Task<HttpRequestClient>> requestClientResolver, bool refreshUrlOnStart)
    //    {
    //        _urlResolver = Guard.Against.Null(urlResolver);
    //        _requestClientResolver = Guard.Against.Null(requestClientResolver);
    //        _refreshUrlOnStart = refreshUrlOnStart;
    //        Status = DownloadStatus.Queued;
    //    }

    //    public async Task StartAsync()
    //    {
    //        if (Status != DownloadStatus.Queued && Status != DownloadStatus.Paused)
    //        {
    //            throw new InvalidOperationException("Download can only be started when it's queued or paused.");
    //        }

    //        if (_httpRequestClient is null)
    //        {
    //            _httpRequestClient = await _requestClientResolver();
    //        }

    //        if (Url.IsNullOrEmpty() || _refreshUrlOnStart)
    //        {
    //            Url = await _urlResolver();
    //        }

    //        await StartDownloadAsync();
    //        Status = DownloadStatus.Downloading;
    //    }

    //    public async Task PauseAsync()
    //    {
    //        if (Status != DownloadStatus.Downloading)
    //        {
    //            throw new InvalidOperationException("Download can only be paused when it's downloading.");
    //        }

    //        await PauseDownloadAsync();
    //        Status = DownloadStatus.Paused;
    //    }

    //    public async Task CancelAsync()
    //    {
    //        if (Status == DownloadStatus.Completed || Status == DownloadStatus.Failed || Status == DownloadStatus.Canceled)
    //        {
    //            return;
    //        }

    //        await CancelDownloadAsync();
    //        Status = DownloadStatus.Canceled;
    //    }

    //    protected abstract Task StartDownloadAsync();
    //    protected abstract Task PauseDownloadAsync();
    //    protected abstract Task CancelDownloadAsync();
    //}

    public class DownloadItem
    {
        private CancellationTokenSource _cancellationTokenSource;
        private readonly Func<Task<Uri>> _urlResolver;
        private readonly bool _refreshUrlOnStart;
        private Func<Task<FileClientBuilder>> _requestBuilder;
        private DownloadFileClient _requestClient;
        public Uri Url { get; private set; }

        public DownloadStatus Status { get; protected set; }

        public DownloadItem(Func<Task<Uri>> urlResolver, Func<Task<FileClientBuilder>> requestBuilder, bool refreshUrlOnStart)
        {
            _urlResolver = Guard.Against.Null(urlResolver);
            _requestBuilder = Guard.Against.Null(requestBuilder);
            _refreshUrlOnStart = refreshUrlOnStart;
            Status = DownloadStatus.Queued;
        }

        public async Task StartAsync()
        {
            if (Status != DownloadStatus.Queued && Status != DownloadStatus.Paused)
            {
                throw new InvalidOperationException("Download can only be started when it's queued or paused.");
            }

            if (Url is null || _refreshUrlOnStart)
            {
                Url = await _urlResolver();
            }

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _cancellationTokenSource = new CancellationTokenSource();
            var _cancellationToken = _cancellationTokenSource.Token;
            var requestBuilder = await _requestBuilder();
            _requestClient = requestBuilder.WithCancellationToken(_cancellationToken).WithUrl(Url.ToString()).Build();
            _requestClient.DownloadFileAsync();
            Status = DownloadStatus.Downloading;
        }

        public async Task PauseAsync()
        {
            if (Status != DownloadStatus.Downloading)
            {
                throw new InvalidOperationException("Download can only be paused when it's downloading.");
            }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            Status = DownloadStatus.Paused;
        }

        public async Task CancelAsync()
        {
            if (Status == DownloadStatus.Completed || Status == DownloadStatus.Failed || Status == DownloadStatus.Canceled)
            {
                return;
            }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            Status = DownloadStatus.Canceled;
        }

        private async void CancelDownloadAsync()
        {

        }
    }

    public class DownloadsManagerTest : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly JastUsaAccountClient _accountClient;
        private ObservableCollection<DownloadItem> _queue = new ObservableCollection<DownloadItem>();

        public ObservableCollection<DownloadItem> Queue
        {
            get { return _queue; }
            set
            {
                if (_queue != value)
                {
                    _queue = value;
                    OnPropertyChanged(nameof(Queue));
                }
            }
        }

        public void CreateDownloadItem()
        {
            var downloadAsset = new GameLink();
            Task<Uri> urlResolver() => CreateUriResolver(downloadAsset);
            Task<FileClientBuilder> requestBuilder() => CreateHttpRequestClient();
            var downloadItem = new DownloadItem(urlResolver, requestBuilder, true);
        }

        private Task<Uri> CreateUriResolver(GameLink downloadAsset)
        {
            return Task.FromResult(_accountClient.GetAssetDownloadLinkAsync(downloadAsset.GameId, downloadAsset.GameLinkId));
        }

        private Task<FileClientBuilder> CreateHttpRequestClient()
        {
            return Task.FromResult(HttpBuilderFactory.GetFileClientBuilder().WithAppendToFile(true));
        }

        public void AddToQueue(DownloadItem item)
        {
            Queue.Add(item);
        }

        public void RemoveFromQueue(DownloadItem item)
        {
            Queue.Remove(item);
        }

        public void RemoveCompleted()
        {
            var completedItems = Queue.Where(item => item.Status == DownloadStatus.Completed).ToList();
            foreach (var item in completedItems)
            {
                Queue.Remove(item);
            }
        }

        public async void PauseDownloads()
        {
            foreach (var item in Queue)
            {
                if (item.Status == DownloadStatus.Downloading)
                {
                    await item.PauseAsync();
                }
            }
        }

        public async void CancelAll()
        {
            foreach (var item in Queue)
            {
                if (item.Status != DownloadStatus.Completed)
                {
                    await item.CancelAsync();
                }
            }
        }
    }

    //public event EventHandler<DownloadItem> DownloadStarted;
    //public event EventHandler<DownloadItem> DownloadPaused;
    //public event EventHandler<DownloadItem> DownloadCanceled;
    //public event EventHandler<DownloadItem> DownloadCompleted;
    //public event EventHandler<DownloadItem> DownloadFailed;

    public class DownloadsManager : ObservableObject
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
        public DownloadsManager(IPlayniteAPI playniteApi, Game game, GameTranslationsResponse gameTranslationsResponse, JastUsaAccountClient accountClient, JastUsaLibrarySettingsViewModel settingsViewModel)
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
                var requestClient = HttpBuilderFactory.GetFileClientBuilder()
                    .WithUrl(url.ToString())
                    .WithDownloadTo(downloadPath)
                    .WithCancellationToken(a.CancelToken)
                    .Build();
                a.ProgressMaxValue = 100;
                var progressHandler = new EventHandler<DownloadProgressArgs>((sender, progressReport) =>
                {
                    UpdateProgressValues(a, progressReport, fileName, progressTitle);
                });

                requestClient.DownloadProgressChanged += progressHandler;
                var result = requestClient.DownloadFile();
                downloadSuccess = result.IsSuccess;
                downloadCanceled = result.IsCancelled;
                requestClient.DownloadProgressChanged -= progressHandler;
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

        private void UpdateProgressValues(GlobalProgressActionArgs a, DownloadProgressArgs progressReport, string fileName, string progressTitle)
        {
            var reportLines = new List<string>
            {
                progressTitle,
                fileName,
                string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderDownloadSizeProgress"),
                progressReport.FormattedBytesReceived, progressReport.FormattedTotalBytesToReceive), // Download progress e.g. 5MB of 10MB
                string.Format("{0:0.00}/100.00%", progressReport.ProgressPercentage),// Percentage e.g. 24.42/100%
                string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderDownloadTimeRemaining"),
                GetTimeReadable(progressReport.TimeRemaining.TotalSeconds)), //Time remaining e.g. 6 seconds left
                progressReport.FormattedDownloadSpeedPerSecond //Download speed e.g. 5MB/s
            };

            a.Text = string.Join("\n\n", reportLines);
            a.CurrentProgressValue = progressReport.ProgressPercentage;
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