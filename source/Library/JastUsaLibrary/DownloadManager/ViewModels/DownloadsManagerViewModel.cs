using JastUsaLibrary.DownloadManager.Enums;
using JastUsaLibrary.DownloadManager.Models;
using JastUsaLibrary.Models;
using JastUsaLibrary.ProgramsHelper;
using JastUsaLibrary.ProgramsHelper.Models;
using JastUsaLibrary.Services;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using WebCommon;
using WebCommon.Builders;
using WebCommon.Enums;
using WebCommon.HttpRequestClient;
using WebCommon.HttpRequestClient.Events;

namespace JastUsaLibrary.DownloadManager.ViewModels
{
    public class DownloadsManagerViewModel : INotifyPropertyChanged, IDisposable
    {
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
        private readonly SemaphoreSlim _downloadsListSemaphore = new SemaphoreSlim(1);
        private bool _isDisposed = false;
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

        private JastAssetWrapper _selectedGameAsset;

        public JastAssetWrapper SelectedGameAsset
        {
            get { return _selectedGameAsset; }
            set
            {
                _selectedGameAsset = value;
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
            _ = RestorePersistingDownloads();
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
                await AddToDownloadsListAsync(downloadItem, false, false);
            }
        }

        public void RefreshLibraryGames()
        {
            SelectedGameWrapper = null;
            SelectedGameAsset = null;
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

        public void CreateDownloadItem(JastGameWrapper selectedGameWrapper, JastAssetWrapper jastAsset, bool silent)
        {
            var downloadAsset = jastAsset.Asset;
            var id = $"{downloadAsset.GameId}-{downloadAsset.GameLinkId}";
            var alreadyInQueue = GetExistsById(id);
            if (alreadyInQueue)
            {
                var errorMessage = string.Format(ResourceProvider.GetString("LOC_JUL_AssetAlreadyInDlListFormat"), downloadAsset.Label);
                _playniteApi.Dialogs.ShowErrorMessage(errorMessage, ResourceProvider.GetString("LOC_JUL_JastLibraryManager"));
                return;
            }

            var assetUri = GetAssetUri(downloadAsset, silent);
            if (assetUri is null)
            {
                var errorMessage = string.Format(ResourceProvider.GetString("LOC_JUL_ObtainAssetUrlFailFormat"), downloadAsset.Label);
                _playniteApi.Dialogs.ShowErrorMessage(errorMessage, ResourceProvider.GetString("LOC_JUL_JastLibraryManager"));
                return;
            }

            string baseDownloadDirectory;
            switch (jastAsset.Type)
            {
                case JastAssetType.Game:
                    baseDownloadDirectory = _settingsViewModel.Settings.GameDownloadsPath;
                    break;
                case JastAssetType.Extra:
                    baseDownloadDirectory = _settingsViewModel.Settings.ExtrasDownloadsPath;
                    break;
                case JastAssetType.Patch:
                    baseDownloadDirectory = _settingsViewModel.Settings.PatchDownloadsPath;;
                    break;
                default:
                    baseDownloadDirectory = _settingsViewModel.Settings.GameDownloadsPath;
                    break;
            }

            var satinizedGameDirectoryName = Paths.ReplaceInvalidCharacters(selectedGameWrapper.Game.Name);
            var gameDownloadDirectory = Path.Combine(baseDownloadDirectory, satinizedGameDirectoryName);
            var downloadData = new DownloadData(selectedGameWrapper.Game, id, jastAsset, assetUri, gameDownloadDirectory);
            Paths.ReplaceInvalidCharacters(string.Empty);
            if (FileSystem.FileExists(downloadData.DownloadPath))
            {
                var errorMessage = string.Format(ResourceProvider.GetString("LOC_JUL_AssetExistsInPathFormat"), jastAsset.Asset.Label, downloadData.DownloadPath);
                _playniteApi.Dialogs.ShowErrorMessage(errorMessage, ResourceProvider.GetString("LOC_JUL_JastLibraryManager"));
                return;
            }

            var downloadItem = new DownloadItem(_jastAccountClient, downloadData, this);
            _ = AddToDownloadsListAsync(downloadItem, false, true);
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

        public async Task AddToDownloadsListAsync(DownloadItem item, bool startDownload, bool persistData)
        {
            await _downloadsListSemaphore.WaitAsync();
            try
            {
                if (!DownloadsList.Any(existingItem => existingItem.DownloadData.Id == item.DownloadData.Id))
                {
                    DownloadsList.Add(item);
                    if (persistData)
                    {
                        PersistDownloadData();
                    }

                    item.DownloadStatusChanged += DownloadItem_DownloadStatusChanged;
                    PersistDownloadData();
                    if (startDownload)
                    {
                        _ = item.StartDownloadAsync();
                    }
                }
                else
                {
                    item.Dispose();
                }
            }
            finally
            {
                _downloadsListSemaphore.Release();
            }

            await StartDownloadsAsync();
        }

        public async Task RemoveFromDownloadsListAsync(DownloadItem item, bool persistChanges)
        {
            await _downloadsListSemaphore.WaitAsync();
            try
            {
                if (DownloadsList.Remove(item) && persistChanges)
                {
                    PersistDownloadData();
                }
            }
            finally
            {
                _downloadsListSemaphore.Release();
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
            item.DownloadStatusChanged -= DownloadItem_DownloadStatusChanged;
            await StartDownloadsAsync();
        }

        public async Task MoveDownloadItemOnePlaceBeforeAsync(DownloadItem item)
        {
            await _downloadsListSemaphore.WaitAsync();
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
                _downloadsListSemaphore.Release();
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
            await _downloadsListSemaphore.WaitAsync();
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
                _downloadsListSemaphore.Release();
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
            var persistChanges = false;
            foreach (var item in DownloadsList.ToList())
            {
                if (item.DownloadData.Status == DownloadItemStatus.Completed ||
                    item.DownloadData.Status == DownloadItemStatus.ExtractionCompleted ||
                    item.DownloadData.Status == DownloadItemStatus.ExtractionFailed)
                {
                    await RemoveFromDownloadsListAsync(item, false);
                    persistChanges = true;
                }
            }

            await Task.Delay(1500);
            if (persistChanges)
            {
                PersistDownloadData();
            }

            NotifyPropertyChangedCommands();
        }

        private async void DownloadItem_DownloadStatusChanged(object sender, DownloadStatusChangedEventArgs e)
        {
            NotifyPropertyChangedCommands();
            var downloadStatus = e.NewStatus;
            var downloadItem = sender as DownloadItem;
            if (downloadStatus != DownloadItemStatus.Completed)
            {
                return;
            }

            _ = StartDownloadsAsync();
            var isExecutable = Path.GetExtension(downloadItem.DownloadData.FileName)
                .Equals(".exe", StringComparison.OrdinalIgnoreCase);
            var databaseGame = _playniteApi.Database.Games[downloadItem.DownloadData.GameId];
            if (isExecutable && databaseGame != null && !databaseGame.IsInstalled)
            {
                var program = Programs.GetProgramData(downloadItem.DownloadData.DownloadPath);
                ApplyProgramToGameCache(databaseGame, program);
            }
            else if (_settingsViewModel.Settings.ExtractFilesOnDownload)
            {
                await Task.Run(() => ExtractZipFile(downloadItem));
            }
        }

        private void ExtractZipFile(DownloadItem downloadItem)
        {
            var downloadPath = downloadItem.DownloadData.DownloadPath;
            var extractDirectory = string.Empty;
            try
            {
                if (!FileSystem.FileExists(downloadPath))
                {
                    _logger.Warn($"File not found: {downloadPath}");
                    return;
                }

                var fileName = Path.GetFileName(downloadPath);
                var isZipFile = Path.GetExtension(fileName).Equals(".zip", StringComparison.OrdinalIgnoreCase);
                if (!isZipFile)
                {
                    _logger.Warn($"Not a zip file: {downloadPath}");
                    return;
                }

                var downloadedFilePath = downloadItem.DownloadData.DownloadDirectory;
                var satinizedDirectory = Paths.ReplaceInvalidCharacters(downloadItem.DownloadData.Name);
                extractDirectory = Path.Combine(downloadedFilePath, satinizedDirectory);

                if (!FileSystem.DirectoryExists(extractDirectory))
                {
                    FileSystem.CreateDirectory(extractDirectory);
                }

                _logger.Info($"Decompressing database zip file {downloadPath} to {extractDirectory}...");
                downloadItem.DownloadData.Status = DownloadItemStatus.Extracting;

                using (ZipArchive archive = ZipFile.OpenRead(downloadPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        // For some reason I found a zip that uses UNIX slash '/' separator character in directories
                        var entryFullName = entry.FullName
                            .Replace('/', Path.DirectorySeparatorChar)
                            .Replace('\\', Path.DirectorySeparatorChar);
                        var destinationFilePath = Path.Combine(extractDirectory, entryFullName);

                        var isDirectory = entry.FullName.EndsWith("/");
                        if (isDirectory)
                        {
                            // No idea why but there's an error when extracting directories
                            // using ExtractToFile so it's needed to create them manually instead
                            if (!FileSystem.DirectoryExists(destinationFilePath))
                            {
                                FileSystem.CreateDirectory(destinationFilePath);
                            }
                        }
                        else
                        {
                            if (FileSystem.FileExists(destinationFilePath))
                            {
                                continue;
                            }

                            var destinationDirectory = Path.GetDirectoryName(destinationFilePath);
                            if (!FileSystem.DirectoryExists(destinationDirectory))
                            {
                                FileSystem.CreateDirectory(destinationDirectory);
                            }

                            entry.ExtractToFile(destinationFilePath);
                        }
                    }
                }
                downloadItem.DownloadData.Status = DownloadItemStatus.ExtractionCompleted;
                _logger.Info("Finish decompressing database zip file");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error while extracting zip file: {ex.Message}");
                downloadItem.DownloadData.Status = DownloadItemStatus.ExtractionFailed;
                return;
            }

            if (_settingsViewModel.Settings.DeleteFilesOnExtract)
            {
                try
                {
                    _logger.Info($"Deleting zip file {downloadPath} after extraction.");
                    FileSystem.DeleteFileSafe(downloadPath);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Failed to delete zip file {ex.Message}");
                }
            }

            if (downloadItem.DownloadData.AssetType == JastAssetType.Game)
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


        private void UpdateSelectedGameAssets()
        {
            if (SelectedGameWrapper != null)
            {
                return;
            }

            var gameTranslations = _plugin.GetGameTranslations(SelectedGameWrapper.Game);
            if (gameTranslations is null)
            {
                return;
            }

            var assetsWrappers = (
                gameTranslations.GamePathLinks?.Select(x => new JastAssetWrapper(x, JastAssetType.Game)) ?? Enumerable.Empty<JastAssetWrapper>())
                .Concat(gameTranslations.GameExtraLinks?.Select(x => new JastAssetWrapper(x, JastAssetType.Extra)) ?? Enumerable.Empty<JastAssetWrapper>())
                .Concat(gameTranslations.GamePatchLinks?.Select(x => new JastAssetWrapper(x, JastAssetType.Patch)) ?? Enumerable.Empty<JastAssetWrapper>())
                .ToObservable();

            SelectedGameWrapper.Assets.Clear();
            foreach (var assetWrapper in assetsWrappers)
            {
                SelectedGameWrapper.Assets.Add(assetWrapper);
            }

            _settingsViewModel.Settings.LibraryCache[SelectedGameWrapper.Game.GameId].Assets = assetsWrappers;
            _plugin.SavePluginSettings();
        }

        private void UpdateActiveGameBindings()
        {
            ActiveGameDevelopers = string.Join(", ", _selectedGameWrapper?.Game.Developers?.Select(x => x.Name) ?? Enumerable.Empty<string>());
            ActiveGamePublishers = string.Join(", ", _selectedGameWrapper?.Game.Publishers?.Select(x => x.Name) ?? Enumerable.Empty<string>());
        }

        public async Task StartDownloadsAsync()
        {
            var remainingSlots = _remainingSlots;
            if (remainingSlots <= 0)
            {
                return;
            }

            var downloadTasks = new List<Task>();
            foreach (var item in _downloadsList.ToList())
            {
                if (remainingSlots == 0)
                {
                    break;
                }

                var status = item.DownloadData.Status;
                if (status == DownloadItemStatus.Paused)
                {
                    downloadTasks.Add(item.ResumeDownloadAsync());
                    remainingSlots--;
                }
                else if (status == DownloadItemStatus.Idle ||
                         status == DownloadItemStatus.Canceled ||
                         status == DownloadItemStatus.Failed)
                {
                    downloadTasks.Add(item.StartDownloadAsync());
                    remainingSlots--;
                }
            }

            await Task.WhenAll(downloadTasks);
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
                await StartDownloadsAsync();
            }, () => _remainingSlots > 0);
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
                UpdateSelectedGameAssets();
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
                OpenDirectoryIfExists(_settingsViewModel.Settings.GameDownloadsPath);
            });
        }

        public RelayCommand OpenPatchesDownloadsDirectory
        {
            get => new RelayCommand(() =>
            {
                OpenDirectoryIfExists(_settingsViewModel.Settings.PatchDownloadsPath);
            });
        }

        public RelayCommand OpenExtrasDownloadsDirectory
        {
            get => new RelayCommand(() =>
            {
                OpenDirectoryIfExists(_settingsViewModel.Settings.ExtrasDownloadsPath);
            });
        }

        public RelayCommand AddSelectedAssetToQueueCommand
        {
            get => new RelayCommand(() =>
            {
                if (_selectedGameAsset != null)
                {
                    CreateDownloadItem(SelectedGameWrapper, _selectedGameAsset, false);
                }
            }, () => SelectedGameWrapper != null && _selectedGameAsset != null && !GetExistsById($"{SelectedGameAsset.Asset.GameId}-{SelectedGameAsset.Asset.GameLinkId}"));
        }

        public void Dispose()
        {
            lock (_disposeLock)
            {
                if (_isDisposed)
                {
                    return;
                }

                _downloadsListSemaphore?.Dispose();
                _isDisposed = true;
            }
        }

    }
}