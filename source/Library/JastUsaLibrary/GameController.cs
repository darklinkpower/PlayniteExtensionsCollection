using JastUsaLibrary.ViewModels;
using JastUsaLibrary.Views;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace JastUsaLibrary
{
    public class JastInstallController : InstallController
    {
        private readonly string _installDir;

        public JastInstallController(Game game, string installDir) : base(game)
        {
            _installDir = installDir;
        }

        public override void Install(InstallActionArgs args)
        {
            var installInfo = new GameInstallationData()
            {
                InstallDirectory = _installDir
            };

            InvokeOnInstalled(new GameInstalledEventArgs(installInfo));
            return;
        }
    }

    public class JastUninstallController : UninstallController
    {
        private readonly Game _game;
        private readonly string _gameExecutablePath;
        private readonly GameCache _gameCache;
        private readonly JastUsaLibrary _plugin;
        private CancellationTokenSource _watcherToken;

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
            }

            var filesDeleted = OpenDeleteItemsWindow();
            if (filesDeleted)
            {
                DeleteGameProgramAndSave();
                InvokeOnUninstalled(new GameUninstalledEventArgs());
            }
            else
            {
                StartUninstallWatcher();
            }
        }

        public async void StartUninstallWatcher()
        {
            _watcherToken = new CancellationTokenSource();
            while (true)
            {
                if (_watcherToken.IsCancellationRequested)
                {
                    return;
                }

                if (!FileSystem.FileExists(_gameExecutablePath))
                {
                    DeleteGameProgramAndSave();
                    InvokeOnUninstalled(new GameUninstalledEventArgs());
                    return;
                }

                await Task.Delay(5_000);
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
            var dataContext = new DirectoryDeleteItemsViewModel(_game, window);
            window.Owner = API.Instance.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            
            window.DataContext = dataContext;
            window.Content = new DirectoryDeleteItemsView();
            window.ShowDialog();

            return dataContext.FilesDeleted;
        }

        public override void Dispose()
        {
            _watcherToken?.Cancel();
            _watcherToken?.Dispose();
        }
    }

}