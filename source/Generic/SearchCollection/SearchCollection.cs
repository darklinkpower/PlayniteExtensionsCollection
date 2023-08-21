using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using SearchCollection.BaseClasses;
using SearchCollection.Interfaces;
using SearchCollection.Models;
using SearchCollection.SearchDefinitions;
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
        public readonly string iconsDirectory;
        private readonly string userIconsDirectory;
        private readonly string searchMenuDescription;
        private readonly List<ISearchDefinition> defaultSearches;

        private SearchCollectionSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("9fcade68-ba3e-428d-a9f9-ca2ee5acaee1");

        public SearchCollection(IPlayniteAPI api) : base(api)
        {
            pluginInstallPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            iconsDirectory = Path.Combine(pluginInstallPath, "Resources", "Icons");
            userIconsDirectory = Path.Combine(GetPluginUserDataPath(), "Icons");

            defaultSearches = new List<ISearchDefinition>
            {
                new IGDB(),
                new Metacritic(),
                new PCGamingWiki(),
                new SteamSearch(),
                new SteamDB(),
                new SteamGridDB(),
                new Twitch(),
                new VNDB(),
                new YouTube()
            };

            searchMenuDescription = ResourceProvider.GetString("LOCSearch_Collection_MenuItemSearchSection");
            settings = new SearchCollectionSettingsViewModel(this, PlayniteApi, userIconsDirectory, pluginInstallPath, defaultSearches);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var menuItems = new List<GameMenuItem>();
            foreach (var searchDefinition in defaultSearches)
            {
                if (settings.Settings.DefaultSearchesSettings.TryGetValue(searchDefinition.Name, out bool isEnabled) && isEnabled)
                {
                    menuItems.Add(new GameMenuItem
                    {
                        Description = searchDefinition.Name,
                        Icon = Path.Combine(iconsDirectory, searchDefinition.Icon),
                        MenuSection = searchMenuDescription,
                        Action = _ =>
                        {
                            foreach (var game in args.Games)
                            {
                                var url = searchDefinition.GetSearchUrl(game);
                                if (!url.IsNullOrEmpty())
                                {
                                    ProcessStarter.StartUrl(url);
                                }
                            }
                        }
                    });
                }
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
                            foreach (var game in args.Games)
                            {
                                var url = searchDefinition.GetSearchUrl(game.Name);
                                if (!url.IsNullOrEmpty())
                                {
                                    ProcessStarter.StartUrl(url);
                                }
                            }
                        }
                    });
            }

            menuItems.Sort((x, y) => x.Description.CompareTo(y.Description));
            return menuItems;
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