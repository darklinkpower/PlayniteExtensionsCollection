using EventsCommon;
using FlowHttp.Events;
using JastUsaLibrary.DownloadManager.Domain.Entities;
using JastUsaLibrary.DownloadManager.Domain.Enums;
using JastUsaLibrary.DownloadManager.Domain.Events;
using JastUsaLibrary.DownloadManager.Domain.Exceptions;
using JastUsaLibrary.Features.DownloadManager.Domain.Events;
using JastUsaLibrary.JastLibraryCacheService.Interfaces;
using JastUsaLibrary.JastUsaIntegration.Application.Services;
using JastUsaLibrary.JastUsaIntegration.Domain.Entities;
using JastUsaLibrary.ProgramsHelper;
using JastUsaLibrary.ProgramsHelper.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JastUsaLibrary.Features.DownloadManager.Application
{
    public class DownloadsManager : IDownloadService, IDisposable
    {
        private readonly IDownloadDataPersistence _downloadsPersistence;
        public event EventHandler<GameInstallationAppliedEventArgs> GameInstallationApplied;
        public event EventHandler<GlobalDownloadProgressChangedEventArgs> GlobalDownloadProgressChanged;
        public event EventHandler<DownloadsListItemsAddedEventArgs> DownloadsListItemsAdded;
        public event EventHandler<DownloadsListItemsRemovedEventArgs> DownloadsListItemsRemoved;
        public event EventHandler<DownloadItemMovedEventArgs> DownloadItemMoved;
        public event EventHandler<DownloadItemStatusChangedEventArgs> DownloadItemStatusChanged;

        private void OnGameInstallationApplied(Game game, GameCache cache)
        {
            GameInstallationApplied?.Invoke(this, new GameInstallationAppliedEventArgs(game, cache));
        }

        private static readonly ILogger _logger = LogManager.GetLogger();
        private readonly IPlayniteAPI _playniteApi;
        private readonly ILibraryCacheService _libraryCacheService;
        private readonly JastUsaLibrary _plugin;
        private readonly JastUsaAccountClient _jastAccountClient;
        private readonly JastUsaLibrarySettingsViewModel _settingsViewModel;
        private readonly ObservableCollection<DownloadItem> _downloadsList;
        public IReadOnlyCollection<DownloadItem> DownloadsList => _downloadsList;
        private readonly SemaphoreSlim _downloadsListAddRemoveSemaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim _bulkStartDownloadsSemaphore = new SemaphoreSlim(1);
        private readonly CancellationTokenSource _extractionCancellationToken = new CancellationTokenSource();
        private bool _isDisposed = false;
        private bool _persistOnListChanges = false;
        private bool _enableDownloadsOnAdd = false;
        private readonly object _disposeLock = new object();

        public int AvailableDownloadSlots => (int)(_settingsViewModel.Settings.MaximumConcurrentDownloads -
            _downloadsList.Count(x => x.DownloadData.Status == DownloadItemStatus.Downloading));

        public DownloadsManager(
            JastUsaLibrary plugin,
            JastUsaAccountClient jastAccountClient,
            JastUsaLibrarySettingsViewModel settingsViewModel,
            IPlayniteAPI playniteApi,
            IDownloadDataPersistence downloadsPersistence,
            ILibraryCacheService libraryCacheService)
        {
            _playniteApi = playniteApi;
            _libraryCacheService = libraryCacheService;
            _plugin = plugin;
            _downloadsPersistence = downloadsPersistence;
            _jastAccountClient = jastAccountClient;
            _settingsViewModel = settingsViewModel;
            _downloadsList = new ObservableCollection<DownloadItem>();           
            Task.Run(async () => await RestorePersistingDownloads()).Wait();
            _persistOnListChanges = true;
            _enableDownloadsOnAdd = true;
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _downloadsList.CollectionChanged += DownloadsList_CollectionChanged;
        }

        private void UnsubscribeToEvents()
        {
            _downloadsList.CollectionChanged -= DownloadsList_CollectionChanged;
        }

        private void Item_DownloadItemProgressChanged(object sender, DownloadItemProgressChangedEventArgs e)
        {
            OnDownloadItemProgressChanged(e);
        }

        private void Item_DownloadItemStatusChanged(object sender, DownloadItemStatusChangedEventArgs e)
        {
            DownloadItemStatusChanged?.Invoke(this, e);
            _downloadsPersistence.PersistDownloadData(e.DownloadItem.DownloadData);
            OnDownloadItemStatusChanged(e);
        }

        private async void OnDownloadItemStatusChanged(DownloadItemStatusChangedEventArgs args)
        {
            var downloadStatus = args.NewStatus;
            var downloadItem = args.DownloadItem;
            if (downloadStatus == DownloadItemStatus.Completed)
            {
                _ = StartDownloadsAsync(false, false);

                var downloadSettings = GetItemDownloadSettings(downloadItem.DownloadData.AssetType);
                var isExecutable = Path.GetExtension(downloadItem.DownloadData.FileName)
                    .Equals(".exe", StringComparison.OrdinalIgnoreCase);
                var databaseGame = _playniteApi.Database.Games[downloadItem.DownloadData.GameId];
                if (isExecutable && databaseGame != null && !databaseGame.IsInstalled)
                {
                    var program = ProgramsService.GetProgramData(downloadItem.DownloadData.DownloadPath);
                    _libraryCacheService.ApplyProgramToGameCache(databaseGame, program);
                }
                else if (downloadSettings.ExtractOnDownload)
                {
                    await Task.Run(() => ExtractCompressedFile(downloadItem, downloadSettings));
                }
            }
            else if (downloadStatus == DownloadItemStatus.Canceled ||
                        downloadStatus == DownloadItemStatus.Paused ||
                        downloadStatus == DownloadItemStatus.Failed)
            {
                _ = StartDownloadsAsync(false, false);
            }
        }

        private void OnDownloadItemProgressChanged(DownloadItemProgressChangedEventArgs args)
        {
            NotifyGlobalProgress();
        }

        private async void DownloadsList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    var newItemsList = e.NewItems.Cast<DownloadItem>();
                    DownloadsListItemsAdded?.Invoke(this, new DownloadsListItemsAddedEventArgs(newItemsList));
                    if (_persistOnListChanges)
                    {
                        PersistDownloadData();
                    }

                    if (_enableDownloadsOnAdd)
                    {
                        await StartDownloadsAsync(false, false);
                    }

                    NotifyGlobalProgress();
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    var oldItemsList = e.OldItems.Cast<DownloadItem>();
                    DownloadsListItemsRemoved?.Invoke(this, new DownloadsListItemsRemovedEventArgs(oldItemsList));
                    if (_persistOnListChanges)
                    {
                        PersistDownloadData();
                    }

                    NotifyGlobalProgress();
                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void NotifyGlobalProgress()
        {
            double? averageProgress = 0;
            long? totalBytesDownloaded = null;
            long? totalBytesToDownload = null;
            double? totalDownloadProgress = null;
            var totalItems = _downloadsList.Count;
            if (totalItems > 0 && _downloadsList.Any(x => x.DownloadData.TotalSize > 0))
            {
                var totalProgress = _downloadsList.Sum(x => x.DownloadData.Progress);
                averageProgress = totalProgress / totalItems;

                totalBytesDownloaded = _downloadsList.Sum(x => x.DownloadData.ProgressSize);
                totalBytesToDownload = _downloadsList.Sum(x => x.DownloadData.TotalSize);
                totalDownloadProgress = (double)totalBytesDownloaded.Value * 100 / totalBytesToDownload.Value;
            }

            OnGlobalProgressChanged(totalItems, averageProgress, totalBytesToDownload, totalBytesDownloaded, totalDownloadProgress);
        }

        private void OnGlobalProgressChanged(
            int totalItems,
            double? averageProgressPercentage,
            long? totalBytesToDownload,
            long? totalBytesDownloaded, 
            double? totalDownloadProgress)
        {
            var args = new GlobalDownloadProgressChangedEventArgs(
                totalItems, averageProgressPercentage,
                totalBytesToDownload, totalBytesDownloaded,
                totalDownloadProgress);

            GlobalDownloadProgressChanged?.Invoke(this, args);
        }

        private async Task RestorePersistingDownloads()
        {
            var persistedDownloads = _downloadsPersistence.LoadPersistedDownloads();
            foreach (var downloadData in persistedDownloads)
            {
                if (downloadData.Status == DownloadItemStatus.Paused ||
                    downloadData.Status == DownloadItemStatus.Failed ||
                    downloadData.Status == DownloadItemStatus.Canceled ||
                    downloadData.Status == DownloadItemStatus.Downloading)
                {
                    downloadData.Status = DownloadItemStatus.Idle;
                }

                var downloadItem = new DownloadItem(_jastAccountClient, downloadData, this);
                await AddDownloadAsync(downloadItem);
            }
        }

        public async Task<bool> AddAssetToDownloadAsync(JastAssetWrapper assetWrapper, CancellationToken cancellationToken = default)
        {
            try
            {
                var assetParentGameWrapper = _libraryCacheService.LibraryGames
                    .FirstOrDefault(x => x.Assets?.Any(
                        y => y.Asset.GameId == assetWrapper.Asset.GameId
                        && y.Asset.GameLinkId == assetWrapper.Asset.GameLinkId) == true);
                if (assetParentGameWrapper is null)
                {
                    return false;
                }

                var assetAddedToDownloads = false;
                var downloadItem = CreateNewDownloadItem(assetParentGameWrapper, assetWrapper, cancellationToken);
                if (downloadItem != null)
                {
                    assetAddedToDownloads = await AddDownloadAsync(downloadItem);
                }

                return assetAddedToDownloads;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error while adding asset download {assetWrapper.Asset.GameId-assetWrapper.Asset.GameLinkId}");
                throw;
            }
        }

        public void StopDownloadsAndPersistDownloadData()
        {
            var downloadingItems = _downloadsList.Where(item => item.DownloadData.Status == DownloadItemStatus.Downloading).ToList();
            if (downloadingItems.Count > 0)
            {
                var dialogText = "JAST USA Library" + "\n\n" + ResourceProvider.GetString("LOC_JUL_StoppingDownloads");
                var progressOptions = new GlobalProgressOptions(dialogText, false)
                {
                    IsIndeterminate = false,
                };

                _playniteApi.Dialogs.ActivateGlobalProgress(async (a) =>
                {
                    a.ProgressMaxValue = downloadingItems.Count;
                    for (int i = 0; i < 100; i++)
                    {
                        if (downloadingItems.Count == 0)
                        {
                            await Task.Delay(150);
                            break;
                        }

                        foreach (var item in downloadingItems.ToList())
                        {
                            if (item.DownloadData.Status == DownloadItemStatus.Downloading)
                            {
                                await item.CancelDownloadAsync();
                            }
                            else
                            {
                                downloadingItems.Remove(item);
                                a.CurrentProgressValue++;
                            }
                        }

                        await Task.Delay(150);
                    }
                }, progressOptions);
            }

            var extractingItems = _downloadsList
                .Where(item => item.DownloadData.Status == DownloadItemStatus.Extracting)
                .ToList();
            
            if (extractingItems.Count > 0)
            {
                var dialogText = "JAST USA Library" + "\n\n" + ResourceProvider.GetString("LOC_JUL_WaitingForExtractions");
                var progressOptions = new GlobalProgressOptions(dialogText, true)
                {
                    IsIndeterminate = false
                };

                _playniteApi.Dialogs.ActivateGlobalProgress(async (a) =>
                {
                    a.ProgressMaxValue = downloadingItems.Count + 1;
                    while (true)
                    {
                        if (a.CancelToken.IsCancellationRequested)
                        {
                            _extractionCancellationToken.Cancel();
                        }

                        if (extractingItems.Count == 0)
                        {
                            break;
                        }

                        foreach (var item in extractingItems.ToList())
                        {
                            if (item.DownloadData.Status != DownloadItemStatus.Extracting)
                            {
                                extractingItems.Remove(item);
                                a.CurrentProgressValue++;
                            }
                        }

                        await Task.Delay(250);
                    }
                }, progressOptions);
            }

            //PersistDownloadData();
        }

        private void PersistDownloadData()
        {
            var downloadDataItems = _downloadsList.Select(item => item.DownloadData).ToList();
            _downloadsPersistence.PersistDownloadData(downloadDataItems);
        }

        private string GenerateGameLinkDownloadId(GameLink gamelink)
        {
            return $"{gamelink.GameId}-{gamelink.GameLinkId}";
        }

        public bool IsGameLinkAssetInQueue(GameLink gamelink)
        {
            var id = GenerateGameLinkDownloadId(gamelink);
            var alreadyInQueue = GetExistsById(id);
            return alreadyInQueue;
        }

        private DownloadItem CreateNewDownloadItem(JastGameWrapper gameWrapper, JastAssetWrapper jastAsset, CancellationToken cancellationToken)
        {
            var downloadAsset = jastAsset.Asset;            
            var alreadyInQueue = IsGameLinkAssetInQueue(downloadAsset);
            if (alreadyInQueue)
            {
                throw new DownloadAlreadyInQueueException(downloadAsset);
            }

            var assetUri = GetAssetUri(downloadAsset, cancellationToken);
            if (assetUri is null)
            {
                return null;
            }

            var downloadSettings = GetItemDownloadSettings(jastAsset.Type);
            var baseDownloadDirectory = downloadSettings.DownloadDirectory;
            var satinizedGameDirectoryName = Paths.ReplaceInvalidCharacters(gameWrapper.Game.Name);
            var gameDownloadDirectory = Path.Combine(baseDownloadDirectory, satinizedGameDirectoryName);

            var id = GenerateGameLinkDownloadId(downloadAsset);
            var downloadData = new DownloadData(gameWrapper.Game, id, jastAsset, assetUri, gameDownloadDirectory);
            if (FileSystem.FileExists(downloadData.DownloadPath))
            {
                throw new AssetAlreadyDownloadedException(jastAsset.Asset, downloadData.DownloadPath);
            }

            return new DownloadItem(_jastAccountClient, downloadData, this);
        }

        public bool RefreshDownloadItemUri(DownloadItem downloadItem, CancellationToken cancellationToken = default)
        {
            var uri = GetAssetUri(downloadItem.DownloadData.GameLink, cancellationToken);
            if (uri != null)
            {
                downloadItem.DownloadData.SetUrl(uri);
                return true;
            }
            else
            {
                return false;
            }
        }

        private Uri GetAssetUri(GameLink downloadAsset, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => _jastAccountClient.GetAssetDownloadLinkAsync(downloadAsset, cancellationToken)).GetAwaiter().GetResult();
        }

        public bool GetExistsById(string Id)
        {
            return GetFromDownloadsListById(Id) != null;
        }

        public DownloadItem GetFromDownloadsListById(string Id)
        {
            return _downloadsList.FirstOrDefault(existingItem => existingItem.DownloadData.Id == Id);
        }

        public async Task<bool> AddDownloadAsync(DownloadItem item)
        {
            await _downloadsListAddRemoveSemaphore.WaitAsync();
            var added = false;
            try
            {
                if (!_downloadsList.Any(existingItem => existingItem.DownloadData.Id == item.DownloadData.Id))
                {
                    _downloadsList.Add(item);
                    item.DownloadItemStatusChanged += Item_DownloadItemStatusChanged;
                    item.DownloadItemProgressChanged += Item_DownloadItemProgressChanged;
                    added = true;
                }
                else
                {
                    item.Dispose();
                }
            }
            finally
            {
                _downloadsListAddRemoveSemaphore.Release();
            }

            return added;
        }

        public async Task RemoveFromDownloadsListAsync(DownloadItem item)
        {
            await _downloadsListAddRemoveSemaphore.WaitAsync();
            try
            {
                _downloadsList.Remove(item);
                item.DownloadItemStatusChanged -= Item_DownloadItemStatusChanged;
                item.DownloadItemProgressChanged -= Item_DownloadItemProgressChanged;
            }
            finally
            {
                _downloadsListAddRemoveSemaphore.Release();
            }

            var waitForCancellation = false;
            await item.CancelDownloadAsync();
            if (item.DownloadData.Status == DownloadItemStatus.Paused)
            {
                await item.ResumeDownloadAsync();
                waitForCancellation = true;
            }

            if (item.DownloadData.Status == DownloadItemStatus.Downloading)
            {
                waitForCancellation = true;
            }

            var deleteTemporaryFile = true;
            if (waitForCancellation)
            {
                for (int i = 0; i < 100; i++)
                {
                    if (item.DownloadData.Status == DownloadItemStatus.Canceled)
                    {
                        deleteTemporaryFile = true;
                        break;
                    }

                    deleteTemporaryFile = false;
                    await Task.Delay(100);
                }
            }

            if (deleteTemporaryFile && !item.DownloadData.IsComplete &&
                FileSystem.FileExists(item.DownloadData.TemporaryDownloadPath))
            {
                FileSystem.DeleteFileSafe(item.DownloadData.TemporaryDownloadPath);
            }

            item.Dispose();
        }

        public async Task MoveDownloadItemOnePlaceBeforeAsync(DownloadItem item)
        {
            await _downloadsListAddRemoveSemaphore.WaitAsync();
            try
            {
                if (item is null || !_downloadsList.Contains(item))
                {
                    return;
                }

                var currentIndex = _downloadsList.IndexOf(item);
                var newIndex = currentIndex - 1;
                if (newIndex >= 0)
                {
                    _downloadsList.Move(currentIndex, newIndex);
                    DownloadItemMoved?.Invoke(this, new DownloadItemMovedEventArgs(item, currentIndex, newIndex));
                }
            }
            finally
            {
                _downloadsListAddRemoveSemaphore.Release();
            }
        }

        public async Task MoveDownloadItemOnePlaceAfterAsync(DownloadItem item)
        {
            await _downloadsListAddRemoveSemaphore.WaitAsync();
            try
            {
                if (item is null || !_downloadsList.Contains(item))
                {
                    return;
                }

                var currentIndex = _downloadsList.IndexOf(item);
                var newIndex = currentIndex + 1;
                if (newIndex >= 0 && newIndex < _downloadsList.Count)
                {
                    _downloadsList.Move(currentIndex, newIndex);
                    DownloadItemMoved?.Invoke(this, new DownloadItemMovedEventArgs(item, currentIndex, newIndex));
                }
            }
            finally
            {
                _downloadsListAddRemoveSemaphore.Release();
            }
        }

        public async Task CancelDownloadsAsync()
        {
            foreach (var item in _downloadsList.ToList())
            {
                if (item.DownloadData.Status == DownloadItemStatus.Paused)
                {
                    await item.CancelDownloadAsync();
                    await item.ResumeDownloadAsync();
                }

                if (item.DownloadData.Status == DownloadItemStatus.Downloading)
                {
                    await item.CancelDownloadAsync();
                }
            }
        }

        public async Task PauseDownloadsAsync()
        {
            foreach (var downloadItem in _downloadsList)
            {
                if (downloadItem.DownloadData.Status == DownloadItemStatus.Downloading)
                {
                    await downloadItem.PauseDownloadAsync();
                }
            }
        }

        public async Task RemoveCompletedDownloadsAsync()
        {
            _persistOnListChanges = false;
            try
            {
                foreach (var downloadItem in _downloadsList.ToList())
                {
                    if (downloadItem.DownloadData.Status == DownloadItemStatus.Completed ||
                        downloadItem.DownloadData.Status == DownloadItemStatus.ExtractionCompleted ||
                        downloadItem.DownloadData.Status == DownloadItemStatus.ExtractionFailed)
                    {
                        await RemoveFromDownloadsListAsync(downloadItem);
                    }
                }
            }
            finally
            {
                _persistOnListChanges = true;
            }

            PersistDownloadData();
        }

        private DownloadSettings GetItemDownloadSettings(JastAssetType assetType)
        {
            switch (assetType)
            {
                case JastAssetType.Game:
                    return _settingsViewModel.Settings.GamesDownloadSettings;
                case JastAssetType.Patch:
                    return _settingsViewModel.Settings.PatchesDownloadSettings;
                case JastAssetType.Extra:
                    return _settingsViewModel.Settings.ExtrasDownloadSettings;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void ExtractCompressedFile(DownloadItem downloadItem, DownloadSettings downloadSettings)
        {
            var filePath = downloadItem.DownloadData.DownloadPath;
            var fileName = Path.GetFileName(filePath);
            var isZipFile = Path.GetExtension(fileName).Equals(".zip", StringComparison.OrdinalIgnoreCase);
            var isRarFile = Path.GetExtension(fileName).Equals(".rar", StringComparison.OrdinalIgnoreCase);
            var isCompressedFile = isZipFile || isRarFile;
            if (!isCompressedFile)
            {
                return;
            }

            var extractDirectory = string.Empty;
            var extractSuccess = true;
            try
            {
                if (!FileSystem.FileExists(filePath))
                {
                    _logger.Warn($"File not found: {filePath}");
                    return;
                }

                var gameCommonDirectory = Path.GetFileName(Path.GetDirectoryName(filePath));
                var sanitizedItemDirectory = Paths.ReplaceInvalidCharacters(downloadItem.DownloadData.Name);
                extractDirectory = Path.Combine(downloadSettings.ExtractDirectory, gameCommonDirectory, sanitizedItemDirectory);
                downloadItem.DownloadData.Status = DownloadItemStatus.Extracting;
                if (isZipFile)
                {
                    extractSuccess = CompressionUtility.ExtractZipFile(filePath, extractDirectory, _extractionCancellationToken.Token);
                }
                else if (isRarFile)
                {
                    extractSuccess = CompressionUtility.ExtractRarFile(filePath, extractDirectory, _extractionCancellationToken.Token);
                }

                downloadItem.DownloadData.Status = extractSuccess ? DownloadItemStatus.ExtractionCompleted : DownloadItemStatus.ExtractionFailed;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error while extracting compressed file: {ex.Message}");
                downloadItem.DownloadData.Status = DownloadItemStatus.ExtractionFailed;
                return;
            }

            if (extractSuccess && downloadSettings.DeleteOnExtract)
            {
                try
                {
                    _logger.Info($"Deleting compressed file {filePath} after extraction.");
                    FileSystem.DeleteFileSafe(filePath);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Failed to delete compressed file {ex.Message}");
                }
            }

            if (extractSuccess && downloadItem.DownloadData.AssetType == JastAssetType.Game)
            {
                _playniteApi.MainView.UIDispatcher.Invoke(() =>
                {
                    ApplyGameInstalation(downloadItem, extractDirectory);
                });
            }
        }

        private void ApplyGameInstalation(DownloadItem downloadItem, string extractDirectory)
        {
            var databaseGame = _playniteApi.Database.Games[downloadItem.DownloadData.GameId];
            if (databaseGame is null || databaseGame.IsInstalled)
            {
                return;
            }

            if (!TryFindExecutable(extractDirectory, out var gameExecutablePath))
            {
                return;
            }

            var program = ProgramsService.GetProgramData(gameExecutablePath);
            _libraryCacheService.ApplyProgramToGameCache(databaseGame, program);
        }

        private static bool TryFindExecutable(string extractDirectory, out string executableFullPath)
        {
            executableFullPath = null;
            if (!Directory.Exists(extractDirectory))
            {
                return false;
            }

            var invalidExtensions = new List<string>
            {
                ".txt",
                ".url",
                ".pdf"
            };

            var setupExecutableParts = new List<string>
            {
                "setup",
                "install",
                "config",
                "setting",
                "sys" // system
            };

            // The heuristics used here is as follows: if an executable file is found along with any other
            // non-executable file(s), it's considered that the directory contains additional game files,
            // implying that it's likely a game directory.
            var anyNonExecutableFound = false;
            var foundExecutables = new List<string>();
            foreach (var filePath in Directory.EnumerateFiles(extractDirectory, "*", SearchOption.AllDirectories))
            {
                var fileName = Path.GetFileName(filePath);
                var fileExtension = Path.GetExtension(fileName);
                if (fileExtension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    foundExecutables.Add(filePath);
                }
                else if (!anyNonExecutableFound
                    && !invalidExtensions.Any(ext => fileExtension.Equals(ext, StringComparison.InvariantCultureIgnoreCase)))
                {
                    anyNonExecutableFound = true;
                }
            }

            if (!anyNonExecutableFound || foundExecutables.Count == 0)
            {
                return false;
            }

            foreach (var executablePath in foundExecutables)
            {
                var executableName = Path.GetFileName(executablePath);
                var matchesForbidden = setupExecutableParts
                    .Any(x => executableName.Contains(x, StringComparison.InvariantCultureIgnoreCase));
                if (!matchesForbidden)
                {
                    executableFullPath = executablePath;
                    return true;
                }
            }

            // As fallback, return the first executable whatever it is
            executableFullPath = foundExecutables[0];
            return true;
        }

        public async Task StartDownloadsAsync(bool startPaused, bool startCancelled)
        {
            await _bulkStartDownloadsSemaphore.WaitAsync();
            try
            {
                var remainingSlots = AvailableDownloadSlots;
                if (remainingSlots <= 0)
                {
                    return;
                }

                var itermsStarted = false;
                foreach (var item in _downloadsList.ToList())
                {
                    if (remainingSlots == 0)
                    {
                        break;
                    }

                    if (item.DownloadData.Status == DownloadItemStatus.Idle)
                    {
                        _ = item.StartDownloadAsync();
                        remainingSlots--;
                        itermsStarted = true;
                    }
                    else if (startPaused && item.DownloadData.Status == DownloadItemStatus.Paused)
                    {
                        _ = item.ResumeDownloadAsync();
                        remainingSlots--;
                        itermsStarted = true;
                    }
                    else if (startCancelled && item.DownloadData.Status == DownloadItemStatus.Canceled)
                    {
                        _ = item.ResumeDownloadAsync();
                        remainingSlots--;
                        itermsStarted = true;
                    }
                }

                if (itermsStarted)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(50));
                }
            }
            finally
            {
                _bulkStartDownloadsSemaphore.Release();
            }
        }

        public void Dispose()
        {
            lock (_disposeLock)
            {
                if (_isDisposed)
                {
                    return;
                }

                _downloadsListAddRemoveSemaphore?.Dispose();               
                UnsubscribeToEvents();
                _isDisposed = true;
            }
        }
    }
}