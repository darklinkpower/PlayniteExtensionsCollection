using FlowHttp.Events;
using JastUsaLibrary.DownloadManager.Enums;
using JastUsaLibrary.DownloadManager.Models;
using JastUsaLibrary.Models;
using JastUsaLibrary.ProgramsHelper;
using JastUsaLibrary.ProgramsHelper.Models;
using JastUsaLibrary.Services;
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

namespace JastUsaLibrary.DownloadManager.ViewModels
{
    public class GameInstallationAppliedEventArgs : EventArgs
    {
        public Game Game { get; }
        public GameCache Cache { get; }

        public GameInstallationAppliedEventArgs(Game game, GameCache cache)
        {
            Game = game;
            Cache = cache;
        }
    }

    public class GlobalProgressChangedEventArgs : EventArgs
    {
        public int TotalItems { get; }
        public double? AverageProgressPercentage { get; }
        public long? TotalBytesToDownload { get; }
        public long? TotalBytesDownloaded { get; }
        public double? TotalDownloadProgress { get; }

        public GlobalProgressChangedEventArgs(int totalItems, double? averageProgressPercentage, long? totalBytesToDownload, long? totalBytesDownloaded, double? totalDownloadProgress)
        {
            TotalItems = totalItems;
            AverageProgressPercentage = averageProgressPercentage;
            TotalBytesToDownload = totalBytesToDownload;
            TotalBytesDownloaded = totalBytesDownloaded;
            TotalDownloadProgress = totalDownloadProgress;
        }
    }

    public class DownloadsManagerViewModel : INotifyPropertyChanged, IDisposable
    {
        public event EventHandler<GameInstallationAppliedEventArgs> GameInstallationApplied;

        private void OnGameInstallationApplied(Game game, GameCache cache)
        {
            GameInstallationApplied?.Invoke(this, new GameInstallationAppliedEventArgs(game, cache));
        }

        public event EventHandler<GlobalProgressChangedEventArgs> GlobalProgressChanged;

        private void OnGlobalProgressChanged(
            int totalItems, double? averageProgressPercentage, long? totalBytesToDownload, long? totalBytesDownloaded, double? totalDownloadProgress)
        {
            GlobalProgressChanged?.Invoke(this, new GlobalProgressChangedEventArgs(totalItems, averageProgressPercentage, totalBytesToDownload, totalBytesDownloaded, totalDownloadProgress));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static readonly ILogger _logger = LogManager.GetLogger();
        private readonly IPlayniteAPI _playniteApi;
        private readonly JastUsaLibrary _plugin;
        private readonly JastUsaAccountClient _jastAccountClient;
        private readonly JastUsaLibrarySettingsViewModel _settingsViewModel;
        private ObservableCollection<DownloadItem> _downloadsList;
        private readonly SemaphoreSlim _downloadsListAddRemoveSemaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim _bulkStartDownloadsSemaphore = new SemaphoreSlim(1);
        private readonly CancellationTokenSource _extractionCancellationToken = new CancellationTokenSource();
        private bool _isDisposed = false;
        private bool _persistOnListChanges = false;
        private bool _enableDownloadsOnAdd = false;
        private readonly object _disposeLock = new object();

        private bool _canPauseAllDownloads => _downloadsList.Any(x => x.DownloadData.Status == DownloadItemStatus.Downloading);
        private bool _canCancelAllDownloads => _downloadsList.Any(x => x.DownloadData.Status == DownloadItemStatus.Downloading);
        private bool _canRemoveCompletedDownloads => _downloadsList
            .Any(x => x.DownloadData.Status == DownloadItemStatus.Completed ||
            x.DownloadData.Status == DownloadItemStatus.ExtractionCompleted ||
            x.DownloadData.Status == DownloadItemStatus.ExtractionFailed);

        private bool _canMoveItemBefore => _selectedDownloadItem != null &&
            _downloadsList.IndexOf(_selectedDownloadItem) - 1 >= 0;
        private bool _canMoveItemAfter => _selectedDownloadItem != null &&
            _downloadsList.IndexOf(_selectedDownloadItem) + 1 < _downloadsList.Count;

        #region Observable Properties
        public ObservableCollection<DownloadItem> DownloadsList
        {
            get { return _downloadsList; }
            set
            {
                if (_downloadsList != value)
                {
                    _downloadsList = value;
                    OnPropertyChanged(nameof(DownloadsList));
                }
            }
        }

        private ObservableCollection<JastGameWrapper> _libraryGames;

        public ObservableCollection<JastGameWrapper> LibraryGames
        {
            get { return _libraryGames; }
            private set
            {
                _libraryGames = value;
                OnPropertyChanged();
                SelectedGameWrapper = LibraryGames?.FirstOrDefault();
            }
        }

        private JastGameWrapper _selectedGameWrapper;

        public JastGameWrapper SelectedGameWrapper
        {
            get { return _selectedGameWrapper; }
            set
            {
                _selectedGameWrapper = value;
                OnPropertyChanged();
                UpdateActiveGameBindings();
            }
        }

        private string _activeGameName = string.Empty;
        public string ActiveGameName
        {
            get => _activeGameName;
            private set
            {
                if (_activeGameName != value)
                {
                    _activeGameName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _activeGameDevelopers = string.Empty;
        public string ActiveGameDevelopers
        {
            get => _activeGameDevelopers;
            private set
            {
                if (_activeGameDevelopers != value)
                {
                    _activeGameDevelopers = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _activeGamePublishers = string.Empty;
        public string ActiveGamePublishers
        {
            get => _activeGamePublishers;
            private set
            {
                if (_activeGamePublishers != value)
                {
                    _activeGamePublishers = value;
                    OnPropertyChanged();
                }
            }
        }

        private JastAssetWrapper _selectedGameAssetWrapper;

        public JastAssetWrapper SelectedGameAssetWrapper
        {
            get { return _selectedGameAssetWrapper; }
            set
            {
                _selectedGameAssetWrapper = value;
                OnPropertyChanged();
                NotifyPropertyChangedCommands();
            }
        }

        private DownloadItem _selectedDownloadItem;
        private int _remainingSlots => (int)(_settingsViewModel.Settings.MaximumConcurrentDownloads -
            _downloadsList.Count(x => x.DownloadData.Status == DownloadItemStatus.Downloading));

        public DownloadItem SelectedDownloadItem
        {
            get { return _selectedDownloadItem; }
            set
            {
                _selectedDownloadItem = value;
                OnPropertyChanged();
                NotifyPropertyChangedCommands();
            }
        }
        #endregion

        public DownloadsManagerViewModel(JastUsaLibrary plugin, JastUsaAccountClient jastAccountClient, JastUsaLibrarySettingsViewModel settingsViewModel)
        {
            _playniteApi = API.Instance;
            _plugin = plugin;
            _jastAccountClient = jastAccountClient;
            _settingsViewModel = settingsViewModel;
            DownloadsList = new ObservableCollection<DownloadItem>();
            DownloadsList.CollectionChanged += DownloadsList_CollectionChanged;
            Task.Run(async () => await RestorePersistingDownloads()).Wait();
            _persistOnListChanges = true;
            _enableDownloadsOnAdd = true;
        }

        private async void DownloadsList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is DownloadItem downloadItem)
                    {
                        downloadItem.DownloadStatusChanged += DownloadItem_DownloadStatusChanged;
                        downloadItem.DownloadProgressChanged += DownloadItem_DownloadProgressChanged;
                    }
                }
                
                if (_persistOnListChanges)
                {
                    PersistDownloadData();
                }
                
                if (_enableDownloadsOnAdd)
                {
                    await StartDownloadsAsync(false, false);
                }

                NotifyGlobalProgress();
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is DownloadItem downloadItem)
                    {
                        downloadItem.DownloadStatusChanged -= DownloadItem_DownloadStatusChanged;
                        downloadItem.DownloadProgressChanged -= DownloadItem_DownloadProgressChanged;
                    }
                }

                if (_persistOnListChanges)
                {
                    PersistDownloadData();
                }

                NotifyGlobalProgress();
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
                totalDownloadProgress = totalBytesDownloaded.Value * 100 / totalBytesToDownload.Value;
            }

            OnGlobalProgressChanged(totalItems, averageProgress, totalBytesToDownload, totalBytesDownloaded, totalDownloadProgress);
        }

        private async Task RestorePersistingDownloads()
        {
            foreach (var downloadData in _settingsViewModel.Settings.DownloadsData.ToList())
            {
                if (downloadData.Status == DownloadItemStatus.Paused ||
                    downloadData.Status == DownloadItemStatus.Failed ||
                    downloadData.Status == DownloadItemStatus.Canceled)
                {
                    downloadData.Status = DownloadItemStatus.Idle;
                }

                var downloadItem = new DownloadItem(_jastAccountClient, downloadData, this);
                await AddToDownloadsListAsync(downloadItem);
            }
        }

        public void RefreshLibraryGames()
        {
            SelectedGameWrapper = null;
            SelectedGameAssetWrapper = null;
            LibraryGames = _playniteApi.Database.Games
                .Where(g => g.PluginId == _plugin.Id)
                .OrderBy(g => g.Name)
                .Select(game =>
                {
                    var gameAssets = _settingsViewModel.Settings.LibraryCache.TryGetValue(game.GameId, out var cache)
                        ? cache.Assets
                        : new ObservableCollection<JastAssetWrapper>();
                    return new JastGameWrapper(game, gameAssets);
                }).ToObservable();
        }

        public async Task<bool> AddAssetToDownloadAsync(JastAssetWrapper assetWrapper)
        {
            RefreshLibraryGames();
            var assetParentGameWrapper = LibraryGames.FirstOrDefault(x => x.Assets.Contains(assetWrapper));
            if (assetParentGameWrapper is null)
            {
                return false;
            }

            var assetAddedToDownloads = false;
            var downloadItem = CreateNewDownloadItem(assetParentGameWrapper, assetWrapper, false);
            if (downloadItem != null)
            {
                assetAddedToDownloads = await AddToDownloadsListAsync(downloadItem);
            }

            return assetAddedToDownloads;
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

            var extractingItems = _downloadsList.Where(item => item.DownloadData.Status == DownloadItemStatus.Extracting).ToList();
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

            PersistDownloadData();
        }

        private void PersistDownloadData()
        {
            var persistingList = _settingsViewModel.Settings.DownloadsData;
            var shouldSaveSettings = false;
            if (persistingList.HasItems())
            {
                persistingList.Clear();
                shouldSaveSettings = true;
            }

            var downloadDataItems = _downloadsList.Select(item => item.DownloadData).ToList();
            if (downloadDataItems.Count > 0)
            {
                foreach (var downloadData in downloadDataItems)
                {
                    _settingsViewModel.Settings.DownloadsData.Add(downloadData);
                }

                shouldSaveSettings = true;
            }

            if (shouldSaveSettings)
            {
                _plugin.SavePluginSettings();
            }
        }

        public DownloadItem CreateNewDownloadItem(JastGameWrapper selectedGameWrapper, JastAssetWrapper jastAsset, bool silent)
        {
            var downloadAsset = jastAsset.Asset;
            var id = $"{downloadAsset.GameId}-{downloadAsset.GameLinkId}";
            var alreadyInQueue = GetExistsById(id);
            if (alreadyInQueue)
            {
                if (!silent)
                {
                    var errorMessage = string.Format(ResourceProvider.GetString("LOC_JUL_AssetAlreadyInDlListFormat"), downloadAsset.Label);
                    _playniteApi.Dialogs.ShowErrorMessage(errorMessage, ResourceProvider.GetString("LOC_JUL_JastLibraryManager"));
                }

                return null;
            }

            var assetUri = GetAssetUri(downloadAsset, silent);
            if (assetUri is null)
            {
                if (!silent)
                {
                    var errorMessage = string.Format(ResourceProvider.GetString("LOC_JUL_ObtainAssetUrlFailFormat"), downloadAsset.Label);
                    _playniteApi.Dialogs.ShowErrorMessage(errorMessage, ResourceProvider.GetString("LOC_JUL_JastLibraryManager"));
                }

                return null;
            }

            var downloadSettings = GetItemDownloadSettings(jastAsset.Type);
            var baseDownloadDirectory = downloadSettings.DownloadDirectory;
            var satinizedGameDirectoryName = Paths.ReplaceInvalidCharacters(selectedGameWrapper.Game.Name);
            var gameDownloadDirectory = Path.Combine(baseDownloadDirectory, satinizedGameDirectoryName);
            var downloadData = new DownloadData(selectedGameWrapper.Game, id, jastAsset, assetUri, gameDownloadDirectory);
            Paths.ReplaceInvalidCharacters(string.Empty);
            if (FileSystem.FileExists(downloadData.DownloadPath))
            {
                if (!silent)
                {
                    var errorMessage = string.Format(ResourceProvider.GetString("LOC_JUL_AssetExistsInPathFormat"), jastAsset.Asset.Label, downloadData.DownloadPath);
                    _playniteApi.Dialogs.ShowErrorMessage(errorMessage, ResourceProvider.GetString("LOC_JUL_JastLibraryManager"));
                }

                return null;
            }

            return new DownloadItem(_jastAccountClient, downloadData, this);
        }

        public bool RefreshDownloadItemUri(DownloadItem downloadItem, bool silent)
        {
            var uri = GetAssetUri(downloadItem.DownloadData.GameLink, silent);
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

        private Uri GetAssetUri(GameLink downloadAsset, bool silent)
        {
            if (silent)
            {
                return _jastAccountClient.GetAssetDownloadLink(downloadAsset);
            }

            var text = string.Format(ResourceProvider.GetString("LOC_JUL_ObtainingAssetUrlFormat"), downloadAsset.Label);
            var progressOptions = new GlobalProgressOptions(text, false)
            {
                IsIndeterminate = true,
            };

            Uri uri = null;
            _playniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                uri = _jastAccountClient.GetAssetDownloadLink(downloadAsset);
            }, progressOptions);

            return uri;
        }

        public bool GetExistsById(string Id)
        {
            return GetFromDownloadsListById(Id) != null;
        }

        public DownloadItem GetFromDownloadsListById(string Id)
        {
            return DownloadsList.FirstOrDefault(existingItem => existingItem.DownloadData.Id == Id);
        }

        public async Task<bool> AddToDownloadsListAsync(DownloadItem item)
        {
            await _downloadsListAddRemoveSemaphore.WaitAsync();
            var added = false;
            try
            {
                if (!DownloadsList.Any(existingItem => existingItem.DownloadData.Id == item.DownloadData.Id))
                {
                    DownloadsList.Add(item);
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

        private void DownloadItem_DownloadProgressChanged(object sender, DownloadProgressArgs e)
        {
            NotifyGlobalProgress();
        }

        public async Task RemoveFromDownloadsListAsync(DownloadItem item)
        {
            await _downloadsListAddRemoveSemaphore.WaitAsync();
            try
            {
                DownloadsList.Remove(item);
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
                if (item is null  || !_downloadsList.Contains(item))
                {
                    return;
                }

                var currentIndex = _downloadsList.IndexOf(item);
                var newIndex = currentIndex - 1;
                if (newIndex >= 0)
                {
                    _downloadsList.Move(currentIndex, newIndex);
                }
            }
            finally
            {
                _downloadsListAddRemoveSemaphore.Release();
            }

            NotifyPropertyChangedCommands();
        }

        private void ExploreAndSelectGameExecutable(JastGameWrapper gameWrapper)
        {
            var selectedProgram = Programs.SelectExecutable();
            if (selectedProgram is null)
            {
                return;
            }

            ApplyProgramToGameCache(gameWrapper.Game, selectedProgram);
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
                }
            }
            finally
            {
                _downloadsListAddRemoveSemaphore.Release();
            }

            NotifyPropertyChangedCommands();
        }



        private async Task CancelDownloadsAsync()
        {
            foreach (var item in DownloadsList.ToList())
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

        private async Task PauseDownloadsAsync()
        {
            foreach (var item in DownloadsList)
            {
                if (item.DownloadData.Status == DownloadItemStatus.Downloading)
                {
                    await item.PauseDownloadAsync();
                }
            }
        }

        private async Task RemoveCompletedDownloadsAsync()
        {
            _persistOnListChanges = false;
            try
            {
                foreach (var item in DownloadsList.ToList())
                {
                    if (item.DownloadData.Status == DownloadItemStatus.Completed ||
                        item.DownloadData.Status == DownloadItemStatus.ExtractionCompleted ||
                        item.DownloadData.Status == DownloadItemStatus.ExtractionFailed)
                    {
                        await RemoveFromDownloadsListAsync(item);
                    }
                }
            }
            finally
            {
                _persistOnListChanges = true;
            }


            PersistDownloadData();
            NotifyPropertyChangedCommands();
        }

        private async void DownloadItem_DownloadStatusChanged(object sender, DownloadStatusChangedEventArgs e)
        {
            NotifyPropertyChangedCommands();
            var downloadStatus = e.NewStatus;
            var downloadItem = sender as DownloadItem;
            if (downloadStatus == DownloadItemStatus.Completed)
            {
                _ = StartDownloadsAsync(false, false);

                var downloadSettings = GetItemDownloadSettings(downloadItem.DownloadData.AssetType);
                var isExecutable = Path.GetExtension(downloadItem.DownloadData.FileName)
                    .Equals(".exe", StringComparison.OrdinalIgnoreCase);
                var databaseGame = _playniteApi.Database.Games[downloadItem.DownloadData.GameId];
                if (isExecutable && databaseGame != null && !databaseGame.IsInstalled)
                {
                    var program = Programs.GetProgramData(downloadItem.DownloadData.DownloadPath);
                    ApplyProgramToGameCache(databaseGame, program);
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

        private DownloadSettings GetItemDownloadSettings(JastAssetType assetType)
        {
            if (assetType == JastAssetType.Game)
            {
                return _settingsViewModel.Settings.GamesDownloadSettings;
            }
            else if (assetType == JastAssetType.Patch)
            {
                return _settingsViewModel.Settings.PatchesDownloadSettings;
            }

            return _settingsViewModel.Settings.ExtrasDownloadSettings;
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

                var downloadedFilePath = downloadItem.DownloadData.DownloadDirectory;
                var satinizedDirectory = Paths.ReplaceInvalidCharacters(downloadItem.DownloadData.Name);
                extractDirectory = Path.Combine(downloadSettings.ExtractDirectory, satinizedDirectory);
                if (!FileSystem.DirectoryExists(extractDirectory))
                {
                    FileSystem.CreateDirectory(extractDirectory);
                }

                downloadItem.DownloadData.Status = DownloadItemStatus.Extracting;
                if (isZipFile)
                {
                    extractSuccess = Compression.ExtractZipFile(filePath, extractDirectory, _extractionCancellationToken.Token);
                }
                else if (isRarFile)
                {
                    extractSuccess = Compression.ExtractRarFile(filePath, extractDirectory, _extractionCancellationToken.Token);
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
            if (databaseGame is null && databaseGame.IsInstalled)
            {
                return;
            }

            if (!TryFindExecutable(extractDirectory, out var gameExecutablePath))
            {
                return;
            }

            var program = Programs.GetProgramData(gameExecutablePath);
            ApplyProgramToGameCache(databaseGame, program);
        }

        private void ApplyProgramToGameCache(Game databaseGame, Program program)
        {
            if (_settingsViewModel.Settings.LibraryCache.TryGetValue(databaseGame.GameId, out var cache))
            {
                cache.Program = program;
                _plugin.SavePluginSettings();

                databaseGame.InstallDirectory = Path.GetDirectoryName(program.Path);
                databaseGame.IsInstalled = true;
                _playniteApi.Database.Games.Update(databaseGame);
                OnGameInstallationApplied(databaseGame, cache);
            }
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
            foreach (string filePath in Directory.EnumerateFiles(extractDirectory, "*", SearchOption.AllDirectories))
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


        private void UpdateSelectedGameAssets(JastGameWrapper gameWrapper)
        {
            if (gameWrapper is null)
            {
                return;
            }

            var dialogText = "JAST USA Library" + "\n\n" + ResourceProvider.GetString("LOC_JUL_UpdatingGameDownloads");
            var progressOptions = new GlobalProgressOptions(dialogText, false)
            {
                IsIndeterminate = true
            };

            ObservableCollection<JastAssetWrapper> assetsWrappers = null;
            _playniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                var gameTranslations = _plugin.GetGameTranslations(SelectedGameWrapper.Game);
                if (gameTranslations is null)
                {
                    return;
                }

                assetsWrappers = (gameTranslations.GamePathLinks?
                    .Select(x => new JastAssetWrapper(x, JastAssetType.Game)) ?? Enumerable.Empty<JastAssetWrapper>())
                    .Concat(gameTranslations.GameExtraLinks?.Select(x => new JastAssetWrapper(x, JastAssetType.Extra)) ?? Enumerable.Empty<JastAssetWrapper>())
                    .Concat(gameTranslations.GamePatchLinks?.Select(x => new JastAssetWrapper(x, JastAssetType.Patch)) ?? Enumerable.Empty<JastAssetWrapper>())
                    .ToObservable();
            }, progressOptions);

            if (assetsWrappers.HasItems())
            {
                gameWrapper.Assets.Clear();
                foreach (var assetWrapper in assetsWrappers)
                {
                    gameWrapper.Assets.Add(assetWrapper);
                }

                _settingsViewModel.Settings.LibraryCache[gameWrapper.Game.GameId].Assets = assetsWrappers;
                _plugin.SavePluginSettings();
            }
        }

        private void UpdateActiveGameBindings()
        {
            ActiveGameDevelopers = string.Join(", ", _selectedGameWrapper?.Game.Developers?.Select(x => x.Name) ?? Enumerable.Empty<string>());
            ActiveGamePublishers = string.Join(", ", _selectedGameWrapper?.Game.Publishers?.Select(x => x.Name) ?? Enumerable.Empty<string>());
        }

        public async Task StartDownloadsAsync(bool startPaused, bool startCancelled)
        {
            await _bulkStartDownloadsSemaphore.WaitAsync();
            try
            {
                var remainingSlots = _remainingSlots;
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

        private void OpenDirectoryIfExists(string directoryPath)
        {
            if (FileSystem.DirectoryExists(directoryPath))
            {
                ProcessStarter.StartProcess(directoryPath);
            }
        }

        private void NotifyPropertyChangedCommands()
        {
            OnPropertyChanged(nameof(AddSelectedAssetToQueueCommand));
            OnPropertyChanged(nameof(RemoveCompletedDownloadsAsyncCommand));
            OnPropertyChanged(nameof(PauseDownloadsAsyncCommand));
            OnPropertyChanged(nameof(CancelDownloadsAsyncCommand));
            OnPropertyChanged(nameof(MoveSelectedDownloadOnePlaceBeforeAsyncCommand));
            OnPropertyChanged(nameof(MoveSelectedDownloadOnePlaceAfterAsyncCommand));
            OnPropertyChanged(nameof(ExploreAndSelectGameExecutableCommand));
            OnPropertyChanged(nameof(StartDownloadsAsyncCommand));
        }

        public RelayCommand PauseDownloadsAsyncCommand
        {
            get => new RelayCommand(async () =>
            {
                await PauseDownloadsAsync();
            }, () => _canPauseAllDownloads);
        }

        public RelayCommand RemoveCompletedDownloadsAsyncCommand
        {
            get => new RelayCommand(async () =>
            {
                await RemoveCompletedDownloadsAsync();
            }, () => _canRemoveCompletedDownloads);
        }

        public RelayCommand ExploreAndSelectGameExecutableCommand
        {
            get => new RelayCommand(() =>
            {
                ExploreAndSelectGameExecutable(_selectedGameWrapper);
            }, () => _selectedGameWrapper != null);
        }

        public RelayCommand ShowSelectedGameOnLibraryCommand
        {
            get => new RelayCommand(() =>
            {
                _playniteApi.MainView.SelectGame(_selectedGameWrapper.Game.Id);
                _playniteApi.MainView.SwitchToLibraryView();
            }, () => _selectedGameWrapper != null);
        }

        public RelayCommand OpenSelectedGameInstallDirectoryCommand
        {
            get => new RelayCommand(() =>
            {
                var installDirPath = _selectedGameWrapper.Game.InstallDirectory;
                if (FileSystem.DirectoryExists(installDirPath))
                {
                    ProcessStarter.StartProcess(installDirPath);
                }
            }, () => _selectedGameWrapper != null);
        }

        public RelayCommand MoveSelectedDownloadOnePlaceBeforeAsyncCommand
        {
            get => new RelayCommand(async () =>
            {
                await MoveDownloadItemOnePlaceBeforeAsync(SelectedDownloadItem);
            }, () => _canMoveItemBefore);
        }

        public RelayCommand MoveSelectedDownloadOnePlaceAfterAsyncCommand
        {
            get => new RelayCommand(async () =>
            {
                await MoveDownloadItemOnePlaceAfterAsync(SelectedDownloadItem);
            }, () => _canMoveItemAfter);
        }

        public RelayCommand StartDownloadsAsyncCommand
        {
            get => new RelayCommand(async () =>
            {
                await StartDownloadsAsync(true, true);
            }, () => _remainingSlots > 0 &&
            _downloadsList.Any(x => x.DownloadData.Status == DownloadItemStatus.Idle || x.DownloadData.Status == DownloadItemStatus.Paused));
        }

        public RelayCommand CancelDownloadsAsyncCommand
        {
            get => new RelayCommand(async () =>
            {
                await CancelDownloadsAsync();
            }, () => _canCancelAllDownloads);
        }

        public RelayCommand NavigateBackCommand
        {
            get => new RelayCommand(() =>
            {
                _playniteApi.MainView.SwitchToLibraryView();
            });
        }

        public RelayCommand UpdateSelectedGameAssetsCommand
        {
            get => new RelayCommand(() =>
            {
                UpdateSelectedGameAssets(SelectedGameWrapper);
            });
        }

        public RelayCommand OpenSettingsCommand
        {
            get => new RelayCommand(() =>
            {
                _plugin.OpenSettingsView();
            });
        }

        public RelayCommand OpenGamesDownloadsDirectory
        {
            get => new RelayCommand(() =>
            {
                OpenDirectoryIfExists(_settingsViewModel.Settings.GamesDownloadSettings.DownloadDirectory);
            });
        }

        public RelayCommand OpenPatchesDownloadsDirectory
        {
            get => new RelayCommand(() =>
            {
                OpenDirectoryIfExists(_settingsViewModel.Settings.PatchesDownloadSettings.DownloadDirectory);
            });
        }

        public RelayCommand OpenExtrasDownloadsDirectory
        {
            get => new RelayCommand(() =>
            {
                OpenDirectoryIfExists(_settingsViewModel.Settings.ExtrasDownloadSettings.DownloadDirectory);
            });
        }

        public RelayCommand AddSelectedAssetToQueueCommand
        {
            get => new RelayCommand(async () =>
            {
                if (_selectedGameAssetWrapper != null)
                {
                    var downloadItem = CreateNewDownloadItem(SelectedGameWrapper, _selectedGameAssetWrapper, false);
                    if (downloadItem != null)
                    {
                        await AddToDownloadsListAsync(downloadItem);
                    }
                }
            }, () => SelectedGameWrapper != null && _selectedGameAssetWrapper != null && !GetExistsById($"{SelectedGameAssetWrapper.Asset.GameId}-{SelectedGameAssetWrapper.Asset.GameLinkId}"));
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
                DownloadsList.CollectionChanged -= DownloadsList_CollectionChanged;
                foreach (var downloadItem in DownloadsList)
                {
                    downloadItem.DownloadStatusChanged -= DownloadItem_DownloadStatusChanged;
                    downloadItem.DownloadProgressChanged -= DownloadItem_DownloadProgressChanged;
                }

                _isDisposed = true;
            }
        }

    }
}