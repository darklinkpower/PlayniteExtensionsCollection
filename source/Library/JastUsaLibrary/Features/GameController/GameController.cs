using EventsCommon;
using JastUsaLibrary.DownloadManager.Domain.Entities;
using JastUsaLibrary.DownloadManager.Domain.Events;
using JastUsaLibrary.Features.DownloadManager.Application;
using JastUsaLibrary.ProgramsHelper.Models;
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
        private bool _subscribedToEvents = false;
        private JastAssetWrapper _downloadingAsset;

        public JastInstallController(
            Game game,
            GameCache gameCache,
            IPlayniteAPI playniteApi,
            ILogger logger,
            IDownloadService downloadsManager,
            JastUsaLibrary jastUsaLibrary) : base(game)
        {
            _playniteApi = playniteApi;
            _logger = logger;
            _game = game;
            _gameCache = gameCache;
            _plugin = jastUsaLibrary;
            _downloadsManager = downloadsManager;
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
            var cache = args.Cache;
            if (_game.Id != eventGame.Id)
            {
                return;
            }

            var installInfo = new GameInstallationData()
            {
                InstallDirectory = Path.GetDirectoryName(cache.Program.Path)
            };

            InvokeOnInstalled(new GameInstalledEventArgs(installInfo));
        }

        private void OnDownloadsListItemsRemoved(object sender, DownloadsListItemsRemovedEventArgs args)
        {
            foreach (var removedItem in args.Items)
            {
                if (removedItem.DownloadData.GameLink.Equals(_downloadingAsset.Asset))
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
            _gameCache.Program = browsedProgram;
            _plugin.SavePluginSettings();
            var installInfo = new GameInstallationData()
            {
                InstallDirectory = Path.GetDirectoryName(browsedProgram.Path)
            };

            InvokeOnInstalled(new GameInstalledEventArgs(installInfo));
        }

        public GameInstallWindowViewModel OpenGameInstallWindow()
        {
            var window = API.Instance.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = true
            });

            window.Height = 800;
            window.Width = 500;
            window.Title = ResourceProvider.GetString("LOC_JUL_WindowTitleJastLibraryUninstaller");
            var dataContext = new GameInstallWindowViewModel(_game, _gameCache, window, _playniteApi, _downloadsManager, _logger);
            window.Owner = API.Instance.Dialogs.GetCurrentAppWindow();
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
        private readonly IPlayniteAPI _playniteAPI = API.Instance;
        private readonly Game _game;
        private readonly string _gameExecutablePath;
        private readonly GameCache _gameCache;
        private readonly JastUsaLibrary _plugin;

        public JastUninstallController(Game game) : base(game)
        {
            Name = ResourceProvider.GetString("LOC_JUL_UninstallJastLibGame");
        }

        public JastUninstallController(Game game, GameCache gameCache, JastUsaLibrary jastUsaLibrary) : this(game)
        {
            _game = game;
            _gameExecutablePath = gameCache.Program.Path;
            _gameCache = gameCache;
            _plugin = jastUsaLibrary;
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
                _playniteAPI.Database.Games.Update(_game);
            }
        }

        private void DeleteGameProgramAndSave()
        {
            _gameCache.Program = null;
            _plugin.SavePluginSettings();
        }

        public bool OpenDeleteItemsWindow()
        {
            var window = API.Instance.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = true
            });

            window.Height = 800;
            window.Width = 500;
            window.Title = ResourceProvider.GetString("LOC_JUL_WindowTitleJastLibraryUninstaller");
            var dataContext = new GameUninstallWindowViewModel(_game, window);
            window.Owner = API.Instance.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            
            window.DataContext = dataContext;
            window.Content = new GameUninstallWindowView();
            window.ShowDialog();

            return dataContext.FilesDeleted;
        }

    }

}