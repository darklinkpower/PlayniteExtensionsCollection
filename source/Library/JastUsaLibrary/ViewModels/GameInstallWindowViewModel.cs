using JastUsaLibrary.DownloadManager.Domain.Entities;
using JastUsaLibrary.DownloadManager.Domain.Enums;
using JastUsaLibrary.DownloadManager.Domain.Interfaces;
using JastUsaLibrary.ProgramsHelper;
using JastUsaLibrary.ProgramsHelper.Models;
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
        public IEnumerable<JastAssetWrapper> GameAssets { get; }

        private readonly Window _window;
        private readonly IDownloadService _downloadsManager;
        public JastAssetWrapper SelectedGameAsset { get; set; }

        public JastAssetWrapper AddedGameAsset { get; private set; }

        public Program BrowsedProgram { get; private set; }

        public GameInstallWindowViewModel(Game game, GameCache gameCache, Window window, IDownloadService downloadsManager)
        {
            Game = game;
            GameAssets = gameCache.Assets.Where(x => x.Type == JastAssetType.Game).OrderBy(x => x.Asset.Label);
            SelectedGameAsset = GameAssets.FirstOrDefault();
            _window = window;
            _downloadsManager = downloadsManager;
        }

        private bool BrowseAndSelectProgram()
        {
            BrowsedProgram = Programs.SelectExecutable();
            if (BrowsedProgram != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> StartAssetGameInstallation()
        {
            var addedAssetToDownloads = await _downloadsManager.AddAssetToDownloadAsync(SelectedGameAsset);
            if (addedAssetToDownloads)
            {
                AddedGameAsset = SelectedGameAsset;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void CloseWindow()
        {
            _window.Close();
        }

        public RelayCommand AddSelectedAssetToDownloadsAndClose
        {
            get => new RelayCommand(async () =>
            {
                var success = await StartAssetGameInstallation();
                if (success)
                {
                    CloseWindow();
                }                
            }, () => SelectedGameAsset != null);
        }

        public RelayCommand CancelCommand
        {
            get => new RelayCommand(() =>
            {
                CloseWindow();
            });
        }

        public RelayCommand BrowseAndSelectProgramCommand
        {
            get => new RelayCommand(() =>
            {
                var success = BrowseAndSelectProgram();
                if (success)
                {
                    CloseWindow();
                }
            });
        }
    }
}