﻿using JastUsaLibrary.DownloadManager.Domain.Entities;
using JastUsaLibrary.DownloadManager.Domain.Enums;
using JastUsaLibrary.DownloadManager.Domain.Events;
using JastUsaLibrary.DownloadManager.Domain.Exceptions;
using JastUsaLibrary.Features.DownloadManager.Domain.Events;
using JastUsaLibrary.Features.InstallationHandler.Application;
using JastUsaLibrary.Features.InstallationHandler.Domain;
using JastUsaLibrary.JastLibraryCacheService.Application;
using JastUsaLibrary.JastUsaIntegration.Application.Services;
using JastUsaLibrary.ProgramsHelper;
using JastUsaLibrary.ProgramsHelper.Models;
using JastUsaLibrary.Services.GameInstallationManager.Application;
using JastUsaLibrary.Services.JastUsaIntegration.Domain.Entities;
using JastUsaLibrary.Services.JastUsaIntegration.Domain.Enums;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static JastUsaLibrary.CompressionUtility;

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

        private void OnGameInstallationApplied(Game game, Program program)
        {
            GameInstallationApplied?.Invoke(this, new GameInstallationAppliedEventArgs(game, program));
        }

        private readonly ILogger _logger;
        private readonly IPlayniteAPI _playniteApi;
        private readonly ILibraryCacheService _libraryCacheService;
        private readonly IGameInstallationManagerService _gameInstallationManagerService;
        private readonly JastUsaLibrary _plugin;
        private readonly JastUsaAccountClient _jastAccountClient;
        private readonly JastUsaLibrarySettingsViewModel _settingsViewModel;
        private readonly ObservableCollection<DownloadItem> _downloadsList;
        private readonly InstallerService _installerService;

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
            ILogger logger,
            IPlayniteAPI playniteApi,
            IDownloadDataPersistence downloadsPersistence,
            ILibraryCacheService libraryCacheService,
            IGameInstallationManagerService gameInstallationManagerService,
            InstallerService installerService)
        {
            _logger = Guard.Against.Null(logger);
            _playniteApi = Guard.Against.Null(playniteApi);
            _libraryCacheService = Guard.Against.Null(libraryCacheService);
            _gameInstallationManagerService = Guard.Against.Null(gameInstallationManagerService);
            _plugin = Guard.Against.Null(plugin);
            _downloadsPersistence = Guard.Against.Null(downloadsPersistence);
            _jastAccountClient = Guard.Against.Null(jastAccountClient);
            _settingsViewModel = Guard.Against.Null(settingsViewModel);
            _downloadsList = new ObservableCollection<DownloadItem>();
            _installerService = installerService;
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
                    _gameInstallationManagerService.ApplyProgramToGameCache(databaseGame, program);
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

                var downloadItem = new DownloadItem(downloadData, this);
                await AddDownloadAsync(downloadItem);
            }
        }

        public async Task<bool> AddAssetToDownloadAsync(JastGameDownloadData assetWrapper, CancellationToken cancellationToken = default)
        {
            try
            {
                var assetParentGameWrapper = _libraryCacheService.GetLibraryGames()
                    .FirstOrDefault(x => x.Assets?.Any(
                        y => y.GameId == assetWrapper.GameId && y.GameLinkId == assetWrapper.GameLinkId) == true);
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
                _logger.Error(e, $"Error while adding asset download {assetWrapper.GameId-assetWrapper.GameLinkId}");
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

        private string GenerateGameLinkDownloadId(JastGameDownloadData gamelink)
        {
            return $"{gamelink.GameId}-{gamelink.GameLinkId}";
        }

        public bool IsGameLinkAssetInQueue(JastGameDownloadData gamelink)
        {
            var id = GenerateGameLinkDownloadId(gamelink);
            var alreadyInQueue = GetExistsById(id);
            return alreadyInQueue;
        }

        private DownloadItem CreateNewDownloadItem(JastGameWrapper gameWrapper, JastGameDownloadData downloadAsset, CancellationToken cancellationToken)
        {
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

            var downloadSettings = GetItemDownloadSettings(downloadAsset.JastDownloadType);
            var baseDownloadDirectory = downloadSettings.DownloadDirectory;
            var satinizedGameDirectoryName = Paths.ReplaceInvalidCharacters(gameWrapper.Game.Name);
            var gameDownloadDirectory = Path.Combine(baseDownloadDirectory, satinizedGameDirectoryName);

            var id = GenerateGameLinkDownloadId(downloadAsset);
            var downloadData = new DownloadData(gameWrapper.Game, id, downloadAsset, assetUri, gameDownloadDirectory);
            if (FileSystem.FileExists(downloadData.DownloadPath))
            {
                throw new AssetAlreadyDownloadedException(downloadAsset, downloadData.DownloadPath);
            }

            return new DownloadItem(downloadData, this);
        }

        public bool RefreshDownloadItemUri(DownloadItem downloadItem, CancellationToken cancellationToken = default)
        {
            var uri = GetAssetUri(downloadItem.DownloadData.JastGameDownloadData, cancellationToken);
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

        private Uri GetAssetUri(JastGameDownloadData downloadAsset, CancellationToken cancellationToken = default)
        {
            try
            {
                return Task.Run(() => _jastAccountClient.GetAssetDownloadLinkAsync(downloadAsset, cancellationToken))
                           .GetAwaiter()
                           .GetResult();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unexpected error while generating asset URI.");
                return null;
            }
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

            var tempDownloadPath = item.DownloadData.TemporaryDownloadPath + ".tmp";
            if (deleteTemporaryFile && !item.DownloadData.IsComplete &&
                FileSystem.FileExists(tempDownloadPath))
            {
                FileSystem.DeleteFileSafe(tempDownloadPath);
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

        private DownloadSettings GetItemDownloadSettings(JastDownloadType assetType)
        {
            switch (assetType)
            {
                case JastDownloadType.Game:
                    return _settingsViewModel.Settings.GamesDownloadSettings;
                case JastDownloadType.Patch:
                    return _settingsViewModel.Settings.PatchesDownloadSettings;
                case JastDownloadType.Extra:
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
            ExtractionResult extractResult = null;
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
                    extractResult = CompressionUtility.ExtractZipFile(filePath, extractDirectory, _extractionCancellationToken.Token);
                }
                else if (isRarFile)
                {
                    extractResult = CompressionUtility.ExtractRarFile(filePath, extractDirectory, _extractionCancellationToken.Token);

                }

                downloadItem.DownloadData.Status = extractResult.Success
                    ? DownloadItemStatus.ExtractionCompleted
                    : DownloadItemStatus.ExtractionFailed;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error while extracting compressed file: {filePath}");
                downloadItem.DownloadData.Status = DownloadItemStatus.ExtractionFailed;
                return;
            }

            if (extractResult.Success && downloadSettings.DeleteOnExtract)
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

            if (extractResult.Success && downloadItem.DownloadData.AssetType == JastDownloadType.Game)
            {
                _playniteApi.MainView.UIDispatcher.Invoke(() =>
                {
                    ApplyGameInstalation(downloadItem, extractDirectory, extractResult);
                });
            }
        }

        private void ApplyGameInstalation(DownloadItem downloadItem, string extractDirectory, ExtractionResult extractResult)
        {
            try
            {
                var databaseGame = _playniteApi.Database.Games[downloadItem.DownloadData.GameId];
                if (databaseGame is null || databaseGame.IsInstalled)
                {
                    return;
                }

                if (!TryFindExecutable(extractDirectory, out var gameExecutablePath, out var isInstaller))
                {
                    return;
                }

                if (!isInstaller)
                {
                    var program = ProgramsService.GetProgramData(gameExecutablePath);
                    _gameInstallationManagerService.ApplyProgramToGameCache(databaseGame, program);
                    OnGameInstallationApplied(databaseGame, program);
                }

                var installRequest = new InstallRequest(gameExecutablePath, extractDirectory);
                var installSuccess = _installerService.Install(installRequest);
                if (installSuccess)
                {
                    // No need to keep installation files if the game has been installed
                    foreach (var filePath in extractResult.ExtractedFiles)
                    {
                        FileSystem.DeleteFileSafe(filePath);
                    }

                    if (!TryFindExecutable(installRequest.TargetDirectory, out var installedFilesExecutable, out var isInstalledExecutable))
                    {
                        return;
                    }

                    var program = ProgramsService.GetProgramData(installedFilesExecutable);
                    _gameInstallationManagerService.ApplyProgramToGameCache(databaseGame, program);
                    OnGameInstallationApplied(databaseGame, program);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error while applying game installation");
            }
        }

        private static bool TryFindExecutable(string extractDirectory, out string executableFullPath, out bool isInstaller)
        {
            executableFullPath = null;
            isInstaller = false;
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
            isInstaller = true;
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