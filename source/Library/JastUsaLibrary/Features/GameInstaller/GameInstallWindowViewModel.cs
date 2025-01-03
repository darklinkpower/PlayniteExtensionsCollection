using JastUsaLibrary.DownloadManager.Domain.Entities;
using JastUsaLibrary.DownloadManager.Domain.Enums;
using JastUsaLibrary.DownloadManager.Domain.Exceptions;
using JastUsaLibrary.Features.DownloadManager.Application;
using JastUsaLibrary.ProgramsHelper;
using JastUsaLibrary.ProgramsHelper.Models;
using JastUsaLibrary.Services.JastLibraryCacheService.Entities;
using JastUsaLibrary.Services.JastUsaIntegration.Domain.Entities;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JastUsaLibrary.ViewModels
{
    public class GameInstallWindowViewModel
    {
        public Game Game { get; }
        public IEnumerable<JastGameDownloadData> GameAssets { get; }

        private readonly Window _window;
        private readonly IDownloadService _downloadsManager;
        private readonly ILogger _logger;

        public JastGameDownloadData SelectedGameAsset { get; }

        private readonly IPlayniteAPI _playniteApi;

        public JastGameDownloadData AddedGameAsset { get; private set; }
        public Program BrowsedProgram { get; private set; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand BrowseAndSelectProgramCommand { get; }
        public RelayCommand AddSelectedAssetToDownloadsAndClose { get; }

        public GameInstallWindowViewModel(
            Game game,
            GameCache gameCache,
            Window window,
            IPlayniteAPI playniteApi,
            IDownloadService downloadsManager,
            ILogger logger)
        {
            Game = game;
            GameAssets = gameCache?.Downloads.GameDownloads.OrderBy(x => x.Label);
            SelectedGameAsset = GameAssets.FirstOrDefault();
            _playniteApi = playniteApi;
            _window = window;
            _downloadsManager = downloadsManager;
            _logger = logger;

            CancelCommand = new RelayCommand(CloseWindow);

            BrowseAndSelectProgramCommand = new RelayCommand(() =>
            {
                var success = BrowseAndSelectProgram();
                if (success)
                {
                    CloseWindow();
                }
            });

            AddSelectedAssetToDownloadsAndClose = new RelayCommand(() =>
            {
                var success = StartAssetGameInstallation(SelectedGameAsset);
                if (success)
                {
                    AddedGameAsset = SelectedGameAsset;
                    CloseWindow();
                }
            }, () => SelectedGameAsset != null);
        }

        private bool BrowseAndSelectProgram()
        {
            BrowsedProgram = ProgramsService.SelectExecutable();
            if (BrowsedProgram != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool StartAssetGameInstallation(JastGameDownloadData jastAssetWrapper)
        {
            var text = string.Format(ResourceProvider.GetString("LOC_JUL_ObtainingAssetUrlFormat"), jastAssetWrapper.Label);
            var progressOptions = new GlobalProgressOptions(text, true)
            {
                IsIndeterminate = true
            };

            var success = false;
            _playniteApi.Dialogs.ActivateGlobalProgress(async (a) =>
            {
                try
                {
                    success = await _downloadsManager.AddAssetToDownloadAsync(jastAssetWrapper);
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
                    var errorMessage = string.Format(ResourceProvider.GetString("LOC_JUL_ObtainAssetUrlFailFormat"), SelectedGameAsset.Label);
                    _playniteApi.Dialogs.ShowErrorMessage(errorMessage + $"\n\n{e.Message}", ResourceProvider.GetString("LOC_JUL_JastLibraryManager"));
                }

            }, progressOptions);

            return success;
        }

        private void CloseWindow()
        {
            _window.Close();
        }

    }
}