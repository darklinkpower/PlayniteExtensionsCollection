using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using SteamCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SearchCollection
{
    public class SearchCollection : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly string pluginInstallPath;
        private readonly string iconsDirectory;
        private readonly string userIconsDirectory;
        private readonly string searchMenuDescription;

        private SearchCollectionSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("9fcade68-ba3e-428d-a9f9-ca2ee5acaee1");

        public SearchCollection(IPlayniteAPI api) : base(api)
        {
            pluginInstallPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            iconsDirectory = Path.Combine(pluginInstallPath, "Resources", "Icons");
            userIconsDirectory = Path.Combine(GetPluginUserDataPath(), "Icons");

            // I don't want to paste this long string everywhere :)
            searchMenuDescription = ResourceProvider.GetString("LOCSearch_Collection_MenuItemSearchSection");
            settings = new SearchCollectionSettingsViewModel(this, PlayniteApi, userIconsDirectory, pluginInstallPath);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var menuItems = new List<GameMenuItem>();

            if (settings.Settings.SearchIsEnabledIgdb)
            {
                menuItems.Add(GetGenericDefaultSearchItem("IGDB", "IGDB.png", @"https://www.igdb.com/search?utf8=✓&type=1&q={0}"));
            }
            if (settings.Settings.SearchIsEnabledMetacritic)
            {
                menuItems.Add(GetGenericDefaultSearchItem("Metacritic", "Metacritic.png", @"https://www.metacritic.com/search/game/{0}/results"));
            }
            if (settings.Settings.SearchIsEnabledPcgw)
            {
                menuItems.Add(GetPcGamingWikiMenuItem());
            }
            if (settings.Settings.SearchIsEnabledSteam)
            {
                menuItems.Add(GetSteamMenuItem());
            }
            if (settings.Settings.SearchIsEnabledSteamDb)
            {
                menuItems.Add(GetSteamDbMenuItem());
            }
            if (settings.Settings.SearchIsEnabledSteamGridDB)
            {
                menuItems.Add(GetGenericDefaultSearchItem("SteamGridDB", "SteamGridDB.png", @"https://www.steamgriddb.com/search/grids?term={0}"));
            }
            if (settings.Settings.SearchIsEnabledTwitch)
            {
                menuItems.Add(GetGenericDefaultSearchItem("Twitch", "Twitch.png", @"https://www.twitch.tv/search?term={0}"));
            }
            if (settings.Settings.SearchIsEnabledVndb)
            {
                menuItems.Add(GetGenericDefaultSearchItem("VNDB", "Vndb.png", @"https://vndb.org/v/all?q={0}"));
            }
            if (settings.Settings.SearchIsEnabledYoutube)
            {
                menuItems.Add(GetGenericDefaultSearchItem("YouTube", "Youtube.png", @"https://www.youtube.com/results?search_query={0}"));
            }

            foreach (var searchDefinition in settings.Settings.SearchDefinitions)
            {
                if (!searchDefinition.IsEnabled)
                {
                    continue;
                }

                menuItems.Add(
                    new GameMenuItem
                    {
                        Description = searchDefinition.Name,
                        MenuSection = searchMenuDescription,
                        Icon = Path.Combine(userIconsDirectory, searchDefinition.Icon),
                        Action = _ =>
                        {
                            InvokeSearchDefinition(searchDefinition, args.Games);
                        }
                    });
            }

            return menuItems;
        }

        private GameMenuItem GetSteamDbMenuItem()
        {
            return new GameMenuItem
            {
                Description = "SteamDB",
                Icon = Path.Combine(iconsDirectory, @"SteamDb.png"),
                MenuSection = searchMenuDescription,
                Action = _ =>
                {
                    InvokeSteamDbSearch();
                }
            };
        }

        private void InvokeSteamDbSearch()
        {
            foreach (var game in PlayniteApi.MainView.SelectedGames.Distinct())
            {
                var steamId = Steam.GetGameSteamId(game, true);
                if (!steamId.IsNullOrEmpty())
                {
                    var searchUrl = string.Format(@"https://steamdb.info/app/{0}", steamId);
                    ProcessStarter.StartUrl(searchUrl);
                }
                else if (PlayniteUtilities.IsGamePcGame(game))
                {
                    var searchUrl = string.Format(@"https://steamdb.info/search/?a=app&q={0}&&type=1&category=0", game.Name.UrlEncode());
                    ProcessStarter.StartUrl(searchUrl);
                }
            }
        }

        private GameMenuItem GetSteamMenuItem()
        {
            return new GameMenuItem
            {
                Description = "Steam",
                Icon = Path.Combine(iconsDirectory, @"Steam.png"),
                MenuSection = searchMenuDescription,
                Action = _ =>
                {
                    InvokeSteamSearch();
                }
            };
        }

        private void InvokeSteamSearch()
        {
            foreach (var game in PlayniteApi.MainView.SelectedGames.Distinct())
            {
                var steamId = Steam.GetGameSteamId(game, true);
                if (!steamId.IsNullOrEmpty())
                {
                    var searchUrl = string.Format(@"https://store.steampowered.com/app/{0}", steamId);
                    ProcessStarter.StartUrl(searchUrl);
                }
                else if (PlayniteUtilities.IsGamePcGame(game))
                {
                    var searchUrl = string.Format(@"https://store.steampowered.com/search/?term={0}&ignore_preferences=1&category1=998", game.Name.UrlEncode());
                    ProcessStarter.StartUrl(searchUrl);
                }
            }
        }

        private GameMenuItem GetPcGamingWikiMenuItem()
        {
            return new GameMenuItem
            {
                Description = "PCGamingWiki",
                Icon = Path.Combine(iconsDirectory, @"Pcgw.png"),
                MenuSection = searchMenuDescription,
                Action = _ =>
                {
                    InvokePcGamingWikiSearch();
                }
            };
        }

        private void InvokePcGamingWikiSearch()
        {
            foreach (var game in PlayniteApi.MainView.SelectedGames.Distinct())
            {
                if (!PlayniteUtilities.IsGamePcGame(game))
                {
                    continue;
                }

                var steamId = Steam.GetGameSteamId(game, true);
                if (!steamId.IsNullOrEmpty())
                {
                    var searchUrl = string.Format(@"https://pcgamingwiki.com/api/appid.php?appid={0}", steamId);
                    ProcessStarter.StartUrl(searchUrl);
                }
                else
                {
                    var searchUrl = string.Format(@"http://pcgamingwiki.com/w/index.php?search={0}", game.Name.UrlEncode());
                    ProcessStarter.StartUrl(searchUrl);
                }
            }
        }

        private GameMenuItem GetGenericDefaultSearchItem(string description, string iconName, string searchTemplate)
        {
            return new GameMenuItem
            {
                Description = description,
                Icon = Path.Combine(iconsDirectory, iconName),
                MenuSection = $"Search",
                Action = _ =>
                {
                    InvokeGenericDefaultSearch(searchTemplate);
                }
            };
        }

        private void InvokeGenericDefaultSearch(string searchTemplate)
        {
            foreach (var game in PlayniteApi.MainView.SelectedGames.Distinct())
            {
                var searchUrl = string.Format(searchTemplate, game.Name.UrlEncode());
                ProcessStarter.StartUrl(searchUrl);
            }
        }

        private void InvokeSearchDefinition(Models.SearchDefinition searchDefinition, List<Game> games)
        {
            foreach (var game in games.Distinct())
            {
                var searchUrl = searchDefinition.SearchTemplate.Replace($"%s", game.Name.UrlEncode());
                ProcessStarter.StartUrl(searchUrl);
            }
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            var defaultIcon = Path.Combine(iconsDirectory, "Default.png");
            var targetDefaultIcon = Path.Combine(userIconsDirectory, "Default.png");
            if (!FileSystem.FileExists(targetDefaultIcon))
            {
                FileSystem.CopyFile(defaultIcon, targetDefaultIcon);
            }
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new SearchCollectionSettingsView();
        }
    }
}