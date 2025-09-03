using JastUsaLibrary.DownloadManager.Domain.Entities;
using JastUsaLibrary.DownloadManager.Domain.Enums;
using JastUsaLibrary.DownloadManager.Domain.Events;
using JastUsaLibrary.DownloadManager.Domain.Exceptions;
using JastUsaLibrary.Features.DownloadManager.Application;
using JastUsaLibrary.Features.DownloadManager.Domain.Events;
using JastUsaLibrary.JastLibraryCacheService.Application;
using JastUsaLibrary.JastUsaIntegration.Application.Services;
using JastUsaLibrary.ProgramsHelper;
using JastUsaLibrary.Services.GameInstallationManager.Application;
using JastUsaLibrary.Services.JastUsaIntegration.Domain.Entities;
using Playnite.SDK;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JastUsaLibrary.DownloadManager.Presentation
{
    public class DownloadsManagerViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IDownloadService _downloadService;
        private readonly IPlayniteAPI _playniteApi;
        private readonly ILogger _logger;
        private readonly ILibraryCacheService _libraryCacheService;
        private readonly IGameInstallationManagerService _gameInstallationManagerService;
        private readonly JastUsaLibrary _plugin;
        private readonly JastUsaAccountClient _jastUsaAccountClient;
        private bool _isDisposed = false;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly JastUsaLibrarySettingsViewModel _settingsViewModel;

        #region Observable Properties
        public ObservableCollection<DownloadItem> DownloadsList => _downloadService.DownloadsList.ToObservable();

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

        private JastGameDownloadData _selectedGameAssetWrapper;

        public JastGameDownloadData SelectedGameAssetWrapper
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

        public DownloadsManagerViewModel(
            JastUsaLibrary plugin,
            JastUsaAccountClient jastUsaAccountClient,
            JastUsaLibrarySettingsViewModel settingsViewModel,
            IPlayniteAPI playniteApi,
            ILogger logger,
            ILibraryCacheService libraryCacheService,
            IGameInstallationManagerService gameInstallationManagerService,
            IDownloadService downloadService)
        {
            _jastUsaAccountClient = Guard.Against.Null(jastUsaAccountClient);
            _playniteApi = Guard.Against.Null(playniteApi);
            _logger = Guard.Against.Null(logger);
            _libraryCacheService = Guard.Against.Null(libraryCacheService);
            _gameInstallationManagerService = Guard.Against.Null(gameInstallationManagerService);
            _downloadService = Guard.Against.Null(downloadService);
            _plugin = Guard.Against.Null(plugin);
            _settingsViewModel = Guard.Against.Null(settingsViewModel);
            SubscribeToEvents();
            RefreshLibraryGames();
        }

        private void SubscribeToEvents()
        {
            _downloadService.DownloadItemMoved += OnDownloadItemMoved;
            _downloadService.DownloadItemStatusChanged += OnDownloadStatusChanged;
            _downloadService.DownloadsListItemsAdded += OnDownloadsListItemsAdded;
            _downloadService.DownloadsListItemsRemoved += OnDownloadsListItemsRemoved;
        }

        private void UnsubscribeFromEvents()
        {
            _downloadService.DownloadItemMoved -= OnDownloadItemMoved;
            _downloadService.DownloadItemStatusChanged -= OnDownloadStatusChanged;
            _downloadService.DownloadsListItemsAdded -= OnDownloadsListItemsAdded;
            _downloadService.DownloadsListItemsRemoved -= OnDownloadsListItemsRemoved;
        }

        private void OnDownloadStatusChanged(object sender, DownloadItemStatusChangedEventArgs args)
        {
            NotifyPropertyChangedCommands();
        }

        private void OnDownloadItemMoved(object sender, DownloadItemMovedEventArgs args)
        {
            OnPropertyChanged(nameof(DownloadsList));
            NotifyPropertyChangedCommands();
        }

        private void OnDownloadsListItemsRemoved(object sender, DownloadsListItemsRemovedEventArgs args)
        {
            OnPropertyChanged(nameof(DownloadsList));
            NotifyPropertyChangedCommands();
        }

        private void OnDownloadsListItemsAdded(object sender, DownloadsListItemsAddedEventArgs args)
        {
            OnPropertyChanged(nameof(DownloadsList));
            NotifyPropertyChangedCommands();
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
                    var gameCache = _libraryCacheService.GetCacheById(Convert.ToInt32(game.GameId));
                    return new JastGameWrapper(game, gameCache);
                }).ToObservable();
        }

        private void UpdateSelectedGameAssets(JastGameWrapper gameWrapper)
        {
            if (gameWrapper is null)
            {
                return;
            }

            var dialogText = "JAST USA Library" + "\n\n" + ResourceProvider.GetString("LOC_JUL_UpdatingGameDownloads");
            var progressOptions = new GlobalProgressOptions(dialogText, true)
            {
                IsIndeterminate = true
            };

            JastGameDownloads gameDownloads = null;
            _playniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                var downloadsId = gameWrapper?.GameCache?.JastGameData?.EnUsId;
                if (!downloadsId.HasValue)
                {
                    return;
                }

                try
                {
                    gameDownloads = _jastUsaAccountClient.GetGameTranslationsAsync(downloadsId.Value)
                        .GetAwaiter()
                        .GetResult();
                }
                catch (Exception e)
                {
                    _logger.Error(e, $"Error fetching GameTranslations for {gameWrapper.GameCache.JastGameData.ProductName} with id {downloadsId}");
                }
            }, progressOptions);

            if (gameDownloads != null)
            {
                gameWrapper.GameCache.UpdateDownloads(gameDownloads);
                _libraryCacheService.SaveCache(gameWrapper.GameCache);
                gameWrapper.UpdateDownloads();
            }
        }

        private void OpenDirectoryIfExists(string directoryPath)
        {
            if (FileSystem.DirectoryExists(directoryPath))
            {
                ProcessStarter.StartProcess(directoryPath);
            }
        }

        private void ExploreAndSelectGameExecutable(JastGameWrapper gameWrapper)
        {
            var selectedProgram = ProgramsService.SelectExecutable();
            if (selectedProgram is null)
            {
                return;
            }

            _gameInstallationManagerService.ApplyProgramToGameCache(gameWrapper.Game, selectedProgram);
        }

        private bool CanPauseAllDownloads()
        {
            return DownloadsList.Any(x => x.DownloadData.Status == DownloadItemStatus.Downloading);
        }

        private bool CanCancelAllDownloads()
        {
            return DownloadsList.Any(x => x.DownloadData.Status == DownloadItemStatus.Downloading);
        }

        private bool CanRemoveCompletedDownloads()
        {
            return DownloadsList.Any(x => x.DownloadData.Status == DownloadItemStatus.Completed ||
                x.DownloadData.Status == DownloadItemStatus.ExtractionCompleted ||
                x.DownloadData.Status == DownloadItemStatus.ExtractionFailed);
        }

        private bool CanMoveItemBefore(DownloadItem downloadItem)
        {
            return downloadItem != null && DownloadsList.IndexOf(downloadItem) - 1 >= 0;
        }

        private bool CanMoveItemAfter(DownloadItem downloadItem)
        {
            return downloadItem != null && DownloadsList.IndexOf(downloadItem) + 1 < DownloadsList.Count;
        }

        private void UpdateActiveGameBindings()
        {
            ActiveGameDevelopers = string.Join(", ", _selectedGameWrapper?.Game.Developers?.Select(x => x.Name) ?? Enumerable.Empty<string>());
            ActiveGamePublishers = string.Join(", ", _selectedGameWrapper?.Game.Publishers?.Select(x => x.Name) ?? Enumerable.Empty<string>());
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

        public void Dispose()
        {
            if (!_isDisposed)
            {
                UnsubscribeFromEvents();
            }
        }

        public RelayCommand PauseDownloadsAsyncCommand
        {
            get => new RelayCommand(async () =>
            {
                await _downloadService.PauseDownloadsAsync();
            }, () => CanPauseAllDownloads());
        }

        public RelayCommand RemoveCompletedDownloadsAsyncCommand
        {
            get => new RelayCommand(async () =>
            {
                await _downloadService.RemoveCompletedDownloadsAsync();
            }, () => CanRemoveCompletedDownloads());
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
                await _downloadService.MoveDownloadItemOnePlaceBeforeAsync(SelectedDownloadItem);
            }, () => CanMoveItemBefore(_selectedDownloadItem));
        }

        public RelayCommand MoveSelectedDownloadOnePlaceAfterAsyncCommand
        {
            get => new RelayCommand(async () =>
            {
                await _downloadService.MoveDownloadItemOnePlaceAfterAsync(SelectedDownloadItem);
            }, () => CanMoveItemAfter(_selectedDownloadItem));
        }

        public RelayCommand StartDownloadsAsyncCommand
        {
            get => new RelayCommand(async () =>
            {
                await _downloadService.StartDownloadsAsync(true, true);
            }, () => _downloadService.AvailableDownloadSlots > 0 &&
            DownloadsList.Any(x => x.DownloadData.Status == DownloadItemStatus.Idle || x.DownloadData.Status == DownloadItemStatus.Paused));
        }

        public RelayCommand CancelDownloadsAsyncCommand
        {
            get => new RelayCommand(async () =>
            {
                await _downloadService.CancelDownloadsAsync();
            }, () => CanCancelAllDownloads());
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
            get => new RelayCommand(() =>
            {
                if (_selectedGameAssetWrapper is null)
                {
                    return;
                }

                var text = string.Format(ResourceProvider.GetString("LOC_JUL_ObtainingAssetUrlFormat"), _selectedGameAssetWrapper.Label);
                var progressOptions = new GlobalProgressOptions(text, true)
                {
                    IsIndeterminate = true
                };

                _playniteApi.Dialogs.ActivateGlobalProgress(async (a) =>
                {
                    try
                    {
                        await _downloadService.AddAssetToDownloadAsync(_selectedGameAssetWrapper);
                    }
                    catch (DownloadAlreadyInQueueException e)
                    {
                        var errorMessage = string.Format(ResourceProvider.GetString("LOC_JUL_AssetAlreadyInDlListFormat"), e.DownloadData.Label);
                        _playniteApi.Dialogs.ShowErrorMessage(errorMessage, ResourceProvider.GetString("LOC_JUL_JastLibraryManager"));
                    }
                    catch (AssetAlreadyDownloadedException e)
                    {
                        var errorMessage = string.Format(ResourceProvider.GetString("LOC_JUL_AssetExistsInPathFormat"), e.DownloadData.Label, e.DownloadPath);
                        _playniteApi.Dialogs.ShowErrorMessage(errorMessage, ResourceProvider.GetString("LOC_JUL_JastLibraryManager"));
                    }
                    catch (Exception e)
                    {
                        var errorMessage = string.Format(ResourceProvider.GetString("LOC_JUL_ObtainAssetUrlFailFormat"), _selectedGameAssetWrapper.Label);
                        _playniteApi.Dialogs.ShowErrorMessage(errorMessage + $"\n\n{e.Message}", ResourceProvider.GetString("LOC_JUL_JastLibraryManager"));
                    }

                }, progressOptions);


            }, () => SelectedGameWrapper != null && _selectedGameAssetWrapper != null && !_downloadService.GetExistsById($"{SelectedGameAssetWrapper.GameId}-{SelectedGameAssetWrapper.GameLinkId}"));
        }
    }
}