using EventsCommon;
using FlowHttp.Events;
using JastUsaLibrary.DownloadManager.Application;
using JastUsaLibrary.DownloadManager.Domain.Entities;
using JastUsaLibrary.DownloadManager.Domain.Enums;
using JastUsaLibrary.DownloadManager.Domain.Events;
using JastUsaLibrary.DownloadManager.Domain.Exceptions;
using JastUsaLibrary.Features.DownloadManager.Application;
using JastUsaLibrary.Features.DownloadManager.Domain.Events;
using JastUsaLibrary.JastLibraryCacheService.Interfaces;
using JastUsaLibrary.JastUsaIntegration.Application.Services;
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

namespace JastUsaLibrary.DownloadManager.Presentation
{
    public class DownloadsManagerViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IDownloadService _downloadService;
        private readonly IPlayniteAPI _playniteApi;
        private readonly ILibraryCacheService _libraryCacheService;
        private readonly JastUsaLibrary _plugin;
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
            JastUsaLibrarySettingsViewModel settingsViewModel,
            IPlayniteAPI playniteApi,
            ILibraryCacheService libraryCacheService,
            IDownloadService downloadService)
        {
            _playniteApi = playniteApi;
            _libraryCacheService = libraryCacheService;
            _downloadService = downloadService;
            _plugin = plugin;
            _settingsViewModel = settingsViewModel;
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
                    var gameCache = _libraryCacheService.GetCacheById(game.GameId);
                    var gameAssets = gameCache != null
                        ? gameCache.Assets
                        : new ObservableCollection<JastAssetWrapper>();
                    return new JastGameWrapper(game, gameAssets);
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

            ObservableCollection<JastAssetWrapper> assetsWrappers = null;
            _playniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                var gameTranslations = _plugin.GetGameTranslations(gameWrapper.Game, a.CancelToken); // works?
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

                _libraryCacheService.ApplyAssetsToCache(gameWrapper.Game.GameId, assetsWrappers);
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

            _libraryCacheService.ApplyProgramToGameCache(gameWrapper.Game, selectedProgram);
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

                var text = string.Format(ResourceProvider.GetString("LOC_JUL_ObtainingAssetUrlFormat"), _selectedGameAssetWrapper.Asset.Label);
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
                        var errorMessage = string.Format(ResourceProvider.GetString("LOC_JUL_AssetAlreadyInDlListFormat"), e.GameLink.Label);
                        _playniteApi.Dialogs.ShowErrorMessage(errorMessage, ResourceProvider.GetString("LOC_JUL_JastLibraryManager"));
                    }
                    catch (AssetAlreadyDownloadedException e)
                    {
                        var errorMessage = string.Format(ResourceProvider.GetString("LOC_JUL_AssetExistsInPathFormat"), e.GameLink.Label, e.DownloadPath);
                        _playniteApi.Dialogs.ShowErrorMessage(errorMessage, ResourceProvider.GetString("LOC_JUL_JastLibraryManager"));
                    }
                    catch (Exception e)
                    {
                        var errorMessage = string.Format(ResourceProvider.GetString("LOC_JUL_ObtainAssetUrlFailFormat"), _selectedGameAssetWrapper.Asset.Label);
                        _playniteApi.Dialogs.ShowErrorMessage(errorMessage + $"\n\n{e.Message}", ResourceProvider.GetString("LOC_JUL_JastLibraryManager"));
                    }

                }, progressOptions);


            }, () => SelectedGameWrapper != null && _selectedGameAssetWrapper != null && !_downloadService.GetExistsById($"{SelectedGameAssetWrapper.Asset.GameId}-{SelectedGameAssetWrapper.Asset.GameLinkId}"));
        }
    }
}