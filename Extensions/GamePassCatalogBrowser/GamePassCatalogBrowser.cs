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
                        InvokeViewWindow();
                    }
                },
                new MainMenuItem
                {
                    Description = "Refresh Cache",
                    MenuSection = "@Game Pass Catalog Browser",
                    Action = args => {
                        RefreshCache();
                    }
                }
            };
        }

        public override void OnLibraryUpdated()
        {
            if (settings.UpdateCatalogOnLibraryUpdate == true)
            {
                UpdateGamePassCatalog(false);
            }
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new GamePassCatalogBrowserSettingsView();
        }

        public void RefreshCache()
        {
            UpdateGamePassCatalog(true);
            PlayniteApi.Dialogs.ShowMessage("Game Pass catalog refreshed", "Game Pass Catalog Browser");
        }

        public List<GamePassGame> UpdateGamePassCatalog(bool resetCache)
        {
            var gamePassGamesList = new List<GamePassGame>();
            PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                var service = new GamePassCatalogBrowserService(PlayniteApi, GetPluginUserDataPath(), settings.NotifyCatalogUpdates);
                if (resetCache == true)
                {
                    service.DeleteCache();
                }
                gamePassGamesList = service.GetGamePassGamesList();
                service.Dispose();
            }, new GlobalProgressOptions("Updating Game Pass Catalog..."));

            return gamePassGamesList;
        }

        public void InvokeViewWindow()
        {
            var gamePassGamesList = UpdateGamePassCatalog(false);

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
            window.Content = new CatalogBrowserView();
            CatalogBrowserViewModel catalogBrowserViewModel = new CatalogBrowserViewModel(gamePassGamesList, PlayniteApi);
            window.DataContext = catalogBrowserViewModel;
            window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.WindowState = WindowState.Maximized;

            window.ShowDialog();

            catalogBrowserViewModel.Dispose();
        }
    }
}