using Playnite.SDK;
using Playnite.SDK.Events;
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
    public class GamePassCatalogBrowser : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private GamePassCatalogBrowserSettings settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("50c85177-570f-4494-be16-99d6aa5b8a93");

        public GamePassCatalogBrowser(IPlayniteAPI api) : base(api)
        {
            settings = new GamePassCatalogBrowserSettings(this);
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = "Browse Game Pass Catalog",
                    MenuSection = "@Game Pass Catalog Browser",
                    Action = o => {
                        InvokeViewWindow();
                    }
                },
                new MainMenuItem
                {
                    Description = "Add all Catalog to the Playnite library",
                    MenuSection = "@Game Pass Catalog Browser",
                    Action = o => {
                        AddAllGamePassCatalog();
                    }
                },
                new MainMenuItem
                {
                    Description = "Reset Cache",
                    MenuSection = "@Game Pass Catalog Browser",
                    Action = o => {
                        ResetCache();
                    }
                }
            };
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
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

        public void ResetCache()
        {
            UpdateGamePassCatalog(true);
            PlayniteApi.Dialogs.ShowMessage("Game Pass catalog has been reset", "Game Pass Catalog Browser");
        }

        public void AddAllGamePassCatalog()
        {
            var choice = PlayniteApi.Dialogs.ShowMessage("Are you sure you want to add all the Game Pass catalog to your library?", "Game Catalog Importer", MessageBoxButton.YesNo);
            if (choice == MessageBoxResult.Yes)
            {
                
                PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
                {
                    var gamePassGamesList = new List<GamePassGame>();
                    var service = new GamePassCatalogBrowserService(PlayniteApi, GetPluginUserDataPath(), settings.NotifyCatalogUpdates,  settings.AddExpiredTagToGames, settings.AddNewGames, settings.RemoveExpiredGames, settings.RegionCode);
                    gamePassGamesList = service.GetGamePassGamesList();
                    if (gamePassGamesList.Count == 0)
                    {
                        PlayniteApi.Dialogs.ShowMessage("Could not obtain Game Pass catalog", "Game Pass Catalog Browser");
                    }
                    else
                    {
                        var addedGames = service.xboxLibraryHelper.AddGamePassListToLibrary(gamePassGamesList);
                        PlayniteApi.Dialogs.ShowMessage($"Added {addedGames} new games to the Playnite library", "Game Pass Catalog Browser");
                    }
                    service.Dispose();
                }, new GlobalProgressOptions("Updating Game Pass Catalog and adding games..."));
            }
        }

        public List<GamePassGame> UpdateGamePassCatalog(bool resetCache)
        {
            var gamePassGamesList = new List<GamePassGame>();
            PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                var service = new GamePassCatalogBrowserService(PlayniteApi, GetPluginUserDataPath(), settings.NotifyCatalogUpdates, settings.AddExpiredTagToGames, settings.AddNewGames, settings.RemoveExpiredGames, settings.RegionCode);
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