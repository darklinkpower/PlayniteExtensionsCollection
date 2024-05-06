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

        private Visibility _activeGameVisibility = Visibility.Hidden;
        public Visibility ActiveGameVisibility
        {
            get => _activeGameVisibility;
            private set
            {
                if (_activeGameVisibility != value)
                {
                    _activeGameVisibility = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _activeGameCoverImage = string.Empty;
        public string ActiveGameCoverImage
        {
            get => _activeGameCoverImage;
            private set
            {
                if (_activeGameCoverImage != value)
                {
                    _activeGameCoverImage = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _activeGameBackgroundImage = string.Empty;
        public string ActiveGameBackgroundImage
        {
            get => _activeGameBackgroundImage;
            private set
            {
                if (_activeGameBackgroundImage != value)
                {
                    _activeGameBackgroundImage = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<JastAssetWrapper> _activeGameAssets;
        public ObservableCollection<JastAssetWrapper> ActiveGameAssets
        {
            get => _activeGameAssets;
            set
            {
                if (_activeGameAssets != value)
                {
                    _activeGameAssets = value;
                    OnPropertyChanged();
                    SelectedGameAsset = ActiveGameAssets?.FirstOrDefault();
                    NotifyPropertyChangedCommands();
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
                await AddToQueueAsync(downloadItem, false);
            }
        }

        private void NotifyPropertyChangedCommands()
        {
            OnPropertyChanged(nameof(AddSelectedAssetToQueueCommand));
        }

        public void RefreshLibraryGames()
        {
            ActiveGameAssets = null;
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
                var progressOptions = new GlobalProgressOptions(ResourceProvider.GetString("Stopping downloads..."), true)
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

        public void CreateDownloadItem(JastGameWrapper selectedGameWrapper, JastAssetWrapper jastAsset)
        {
            var downloadAsset = jastAsset.Asset;
            var id = $"{downloadAsset.GameId}-{downloadAsset.GameLinkId}";
            var alreadyInQueue = GetExistsById(id);
            if (alreadyInQueue)
            {
                _playniteApi.Dialogs.ShowErrorMessage($"{downloadAsset.Label} is already in queue.", "JAST USA Download Manager");
                return;
            }

            var assetUri = GetAssetUri(downloadAsset);
            if (assetUri is null)
            {
                _playniteApi.Dialogs.ShowErrorMessage($"Could not obtain download link for {downloadAsset.Label}.", "JAST USA Download Manager");
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
                _playniteApi.Dialogs.ShowErrorMessage($"Download file already exists in {downloadData.DownloadPath}", "JAST USA Library Manager");
                return;
            }

            var downloadItem = new DownloadItem(_jastAccountClient, downloadData, this);
            _ = AddToQueueAsync(downloadItem, false);
        }

        public bool RefreshDownloadItemUri(DownloadItem downloadItem)
        {
            var uri = GetAssetUri(downloadItem.DownloadData.GameLink);
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

        private Uri GetAssetUri(GameLink downloadAsset)
        {
            return  _jastAccountClient.GetAssetDownloadLink(downloadAsset);
        }

        public bool GetExistsById(string Id)
        {
            return GetFromDownloadsListById(Id) != null;
        }

        public DownloadItem GetFromDownloadsListById(string Id)
        {
            return DownloadsList.FirstOrDefault(existingItem => existingItem.DownloadData.Id == Id);
        }

        public async Task AddToQueueAsync(DownloadItem item, bool startDownload)
        {
            await _downloadsListSemaphore.WaitAsync();
            try
            {
                if (!DownloadsList.Any(existingItem => existingItem.DownloadData.Id == item.DownloadData.Id))
                {
                    DownloadsList.Add(item);
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
        }

        public async Task RemoveFromDownloadsListAsync(DownloadItem item)
        {
            await _downloadsListSemaphore.WaitAsync();
            try
            {
                DownloadsList.Remove(item);
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
            foreach (var item in DownloadsList.ToList())
            {
                if (item.DownloadData.Status != DownloadItemStatus.Completed &&
                    item.DownloadData.Status != DownloadItemStatus.ExtractionCompleted &&
                    item.DownloadData.Status != DownloadItemStatus.ExtractionFailed)
                {
                    await RemoveFromDownloadsListAsync(item);
                }
            }
        }

        private async void DownloadItem_DownloadStatusChanged(object sender, DownloadStatusChangedEventArgs e)
        {
            var downloadStatus = e.NewStatus;
            var downloadItem = sender as DownloadItem;
            if (downloadStatus != DownloadItemStatus.Completed)
            {
                return;
            }

            if (_settingsViewModel.Settings.ExtractDownloadedZips)
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

            if (_settingsViewModel.Settings.DeleteDownloadedZips)
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

            if (_settingsViewModel.Settings.LibraryCache.TryGetValue(databaseGame.GameId, out var cache)
                && TryFindExecutable(extractDirectory, out var gameExecutablePath))
            {
                var program = Programs.GetProgramData(gameExecutablePath);
                cache.Program = program;
                _plugin.SavePluginSettings();

                databaseGame.InstallDirectory = Path.GetDirectoryName(gameExecutablePath);
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
            ActiveGameBackgroundImage = _selectedGameWrapper?.Game.BackgroundImage;
            ActiveGameCoverImage = _selectedGameWrapper?.Game.CoverImage;
            ActiveGameName = _selectedGameWrapper?.Game.Name;
            ActiveGameDevelopers = string.Join(", ", _selectedGameWrapper?.Game.Developers?.Select(x => x.Name) ?? Enumerable.Empty<string>());
            ActiveGamePublishers = string.Join(", ", _selectedGameWrapper?.Game.Publishers?.Select(x => x.Name) ?? Enumerable.Empty<string>());
            ActiveGameAssets = _selectedGameWrapper?.Assets;
            ActiveGameVisibility = _selectedGameWrapper != null ? Visibility.Visible : Visibility.Hidden;
        }

        public RelayCommand AddSelectedAssetToQueueCommand
        {
            get => new RelayCommand(() =>
            {
                if (_selectedGameAsset != null)
                {
                    CreateDownloadItem(SelectedGameWrapper, _selectedGameAsset);
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

        private string GetTimeReadable(TimeSpan timeSpan)
        {
            var timeSeconds = Math.Ceiling(timeSpan.TotalSeconds);
            if (timeSeconds > 3600)
            {
                return string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderTimeHoursMinsFormat"), timeSeconds / 3600, timeSeconds % 3600);
            }
            if (timeSeconds > 60)
            {
                return string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderTimeMinsSecondsFormat"), timeSeconds / 60, timeSeconds % 60);
            }
            else
            {
                return string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderTimeSecondsFormat"), timeSeconds);
            }
        }
    }
}