using JastUsaLibrary.DownloadManager.Domain.Entities;
using JastUsaLibrary.DownloadManager.Domain.Events;
using JastUsaLibrary.Features.DownloadManager.Application;
using JastUsaLibrary.JastLibraryCacheService.Interfaces;
using JastUsaLibrary.ProgramsHelper.Models;
using JastUsaLibrary.Services.JastLibraryCacheService.Entities;
using JastUsaLibrary.Services.JastUsaIntegration.Domain.Entities;
using JastUsaLibrary.ViewModels;
using JastUsaLibrary.Views;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace JastUsaLibrary
{
    public class JastInstallController : InstallController
    {
        private readonly IPlayniteAPI _playniteApi;
        private readonly ILogger _logger;
        private readonly Game _game;
        private readonly GameCache _gameCache;
        private readonly JastUsaLibrary _plugin;
        private readonly IDownloadService _downloadsManager;
        private readonly ILibraryCacheService _libraryCacheService;
        private bool _subscribedToEvents = false;
        private JastGameDownloadData _downloadingAsset;

        public JastInstallController(
            Game game,
            GameCache gameCache,
            IPlayniteAPI playniteApi,
            ILogger logger,
            IDownloadService downloadsManager,
            ILibraryCacheService libraryCacheService,
            JastUsaLibrary jastUsaLibrary) : base(game)
        {
            _playniteApi = Guard.Against.Null(playniteApi);
            _logger = Guard.Against.Null(logger);
            _game = Guard.Against.Null(game);
            _gameCache = Guard.Against.Null(gameCache);
            _plugin = Guard.Against.Null(jastUsaLibrary);
            _downloadsManager = Guard.Against.Null(downloadsManager);
            _libraryCacheService = Guard.Against.Null(libraryCacheService);
        }

        private void SubscribeToEvents()
        {
            _downloadsManager.GameInstallationApplied += OnGameInstallationApplied;
            _downloadsManager.DownloadsListItemsRemoved += OnDownloadsListItemsRemoved;
        }

        private void UnsubscribeFromEvents()
        {
            _downloadsManager.GameInstallationApplied -= OnGameInstallationApplied;
            _downloadsManager.DownloadsListItemsRemoved -= OnDownloadsListItemsRemoved;
        }

        private void OnGameInstallationApplied(object sender, GameInstallationAppliedEventArgs args)
        {
            var eventGame = args.Game;
            if (_game.Id != eventGame.Id)
            {
                return;
            }

            var installInfo = new GameInstallationData()
            {
                InstallDirectory = Path.GetDirectoryName(args.Program.Path)
            };

            InvokeOnInstalled(new GameInstalledEventArgs(installInfo));
        }

        private void OnDownloadsListItemsRemoved(object sender, DownloadsListItemsRemovedEventArgs args)
        {
            foreach (var removedItem in args.Items)
            {
                if (removedItem.DownloadData.JastGameDownloadData.GameId == _downloadingAsset.GameId
                    && removedItem.DownloadData.JastGameDownloadData.GameLinkId == _downloadingAsset.GameLinkId)
                {
                    StopInstallationProcess();
                    break;
                }
            }
        }

        public override void Install(InstallActionArgs args)
        {
            var gameInstallViewModel = OpenGameInstallWindow();
            if (gameInstallViewModel.BrowsedProgram != null)
            {
                AddGameProgramAndSave(gameInstallViewModel.BrowsedProgram);
            }
            else if (gameInstallViewModel.AddedGameAsset != null)
            {
                _downloadingAsset = gameInstallViewModel.AddedGameAsset;
                SubscribeToEvents();
                _subscribedToEvents = true;
            }
            else
            {
                StopInstallationProcess();
            }
        }

        private void StopInstallationProcess()
        {
            InvokeOnInstalled(new GameInstalledEventArgs());
            _game.IsInstalled = false;
            _playniteApi.Database.Games.Update(_game);
        }

        private void AddGameProgramAndSave(Program browsedProgram)
        {
            _libraryCacheService.ApplyProgramToGameCache(Game, _gameCache, browsedProgram);
            var installInfo = new GameInstallationData()
            {
                InstallDirectory = Path.GetDirectoryName(browsedProgram.Path)
            };

            InvokeOnInstalled(new GameInstalledEventArgs(installInfo));
        }

        public GameInstallWindowViewModel OpenGameInstallWindow()
        {
            var window = _playniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = true
            });

            window.Height = 800;
            window.Width = 500;
            window.Title = ResourceProvider.GetString("LOC_JUL_WindowTitleJastLibraryUninstaller");
            var dataContext = new GameInstallWindowViewModel(_game, _gameCache, window, _playniteApi, _downloadsManager, _logger);
            window.Owner = _playniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            window.DataContext = dataContext;
            window.Content = new GameInstallWindowView();
            window.ShowDialog();

            return dataContext;
        }

        public override void Dispose()
        {
            if (_subscribedToEvents)
            {
                UnsubscribeFromEvents();
            }
        }
    }

    public class JastUninstallController : UninstallController
    {
        private readonly IPlayniteAPI _playniteApi;
        private readonly ILibraryCacheService _libraryCacheService;
        private readonly Game _game;
        private readonly string _gameExecutablePath;
        private readonly GameCache _gameCache;
        private readonly JastUsaLibrary _plugin;

        public JastUninstallController(
            Game game,
            IPlayniteAPI playniteApi,
            ILibraryCacheService libraryCacheService,
            GameCache gameCache,
            JastUsaLibrary jastUsaLibrary) : base(game)
        {
            _playniteApi = Guard.Against.Null(playniteApi);
            _libraryCacheService = Guard.Against.Null(libraryCacheService);
            _game = Guard.Against.Null(game);
            _gameExecutablePath = Guard.Against.NullOrEmpty(gameCache.Program.Path);
            _gameCache = Guard.Against.Null(gameCache);
            _plugin = Guard.Against.Null(jastUsaLibrary);
            Name = ResourceProvider.GetString("LOC_JUL_UninstallJastLibGame");
        }

        public override void Uninstall(UninstallActionArgs args)
        {
            if (!FileSystem.FileExists(_gameExecutablePath))
            {
                DeleteGameProgramAndSave();
                InvokeOnUninstalled(new GameUninstalledEventArgs());
                return;
            }

            var filesDeleted = OpenDeleteItemsWindow();
            if (filesDeleted)
            {
                DeleteGameProgramAndSave();
                InvokeOnUninstalled(new GameUninstalledEventArgs());
            }
            else
            {
                InvokeOnUninstalled(new GameUninstalledEventArgs());

                // Restore game installation state since it wasn't uninstalled
                _game.IsInstalled = true;
                _game.InstallDirectory = Path.GetDirectoryName(_gameExecutablePath);
                _playniteApi.Database.Games.Update(_game);
            }
        }

        private void DeleteGameProgramAndSave()
        {
            _libraryCacheService.ApplyProgramToGameCache(Game, _gameCache, null);
            _plugin.SavePluginSettings();
        }

        public bool OpenDeleteItemsWindow()
        {
            var window = _playniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = true
            });

            window.Height = 800;
            window.Width = 500;
            window.Title = ResourceProvider.GetString("LOC_JUL_WindowTitleJastLibraryUninstaller");
            var dataContext = new GameUninstallWindowViewModel(_game, window);
            window.Owner = _playniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            
            window.DataContext = dataContext;
            window.Content = new GameUninstallWindowView();
            window.ShowDialog();

            return dataContext.FilesDeleted;
        }

    }

}