using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GamePassCatalogBrowser.Models;
using GamePassCatalogBrowser.Services;
using GamePassCatalogBrowser.ViewModels;
using GamePassCatalogBrowser.Views;

namespace GamePassCatalogBrowser
{
    public class GamePassCatalogBrowser : Plugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private GamePassCatalogBrowserSettings settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("50c85177-570f-4494-be16-99d6aa5b8a93");

        public GamePassCatalogBrowser(IPlayniteAPI api) : base(api)
        {
            settings = new GamePassCatalogBrowserSettings(this);
        }

        public override List<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs menuArgs)
        {
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = "Browse Game Pass Catalog",
                    MenuSection = "@Game Pass Catalog Browser",
                    Action = args => {
                        MainMethod();
                    }
                }
            };
        }

        public override void OnLibraryUpdated()
        {
            // Add code to be executed when library is updated.
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new GamePassCatalogBrowserSettingsView();
        }

        public void MainMethod()
        {

            var gamePassGamesList = new List<GamePassGame>();
            PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                var service = new GamePassCatalogBrowserService(PlayniteApi, GetPluginUserDataPath());
                gamePassGamesList = service.GetGamePassGamesList();
                service.Dispose();
            }, new GlobalProgressOptions("Updating Game Pass Catalog..."));

            if (gamePassGamesList.Count == 0)
            {
                PlayniteApi.Dialogs.ShowMessage("Could not obtain Game Pass catalog", "Game Pass Catalog Browser");
                return;
            }

            var window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false
            });

            window.Title = "Game Pass Catalog Browser";

            // Set content of a window. Can be loaded from xaml, loaded from UserControl or created from code behind
            window.Content = new CatalogBrowserView();
            window.DataContext = new CatalogBrowserViewModel(gamePassGamesList);

            // Set owner if you need to create modal dialog window
            window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.WindowState = WindowState.Maximized;

            // Use Show or ShowDialog to show the window
            window.ShowDialog();
        }
    }
}