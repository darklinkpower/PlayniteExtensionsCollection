using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using SteamCommon;
using SteamShortcuts.Application;
using SteamShortcuts.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SteamShortcuts
{
    public class SteamShortcuts : GenericPlugin
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private readonly SteamUriLauncherService _steamUriLauncherService;
        private readonly SteamShortcutsSettingsViewModel _settingsViewModel;
        private readonly List<GameMenuItem> _steamComponentMenuItems;
        private static readonly string _menuSection = ResourceProvider.GetString("LOCSteam_Viewer_SteamShortcutsLabel");

        public override Guid Id { get; } = Guid.Parse("0a3edabb-065f-4056-a294-d6bc0656e2ac");

        public SteamShortcuts(IPlayniteAPI api) : base(api)
        {
            _steamUriLauncherService = new SteamUriLauncherService();
            _settingsViewModel = new SteamShortcutsSettingsViewModel(this, _steamUriLauncherService);
            Properties = new GenericPluginProperties { HasSettings = true };
            _steamComponentMenuItems = GetSteamComponentMenuItems();
            _steamUriLauncherService.LaunchUrlsInSteamClient = _settingsViewModel.Settings.LaunchUrlsInSteamClient;
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var game = args.Games[0];
            var menuItems = new List<GameMenuItem>();

            if (args.Games.Count == 1)
            {
                var isSteamGame = Steam.IsGameSteamGame(game);
                if (isSteamGame)
                {
                    AddSteamClientMenuItems(menuItems, game.GameId);
                    AddSeparatorToMenuItems(menuItems, _menuSection);

                    AddSteamWebLinkMenuItems(menuItems, game.GameId);
                    AddSeparatorToMenuItems(menuItems, _menuSection);
                }
                else if (_settingsViewModel.Settings.AddWebLinksForNonSteam)
                {
                    var steamId = Steam.GetSteamIdFromLinks(game);
                    if (!steamId.IsNullOrEmpty())
                    {
                        AddSteamWebLinkMenuItems(menuItems, steamId);
                        AddSeparatorToMenuItems(menuItems, _menuSection);
                    }
                }
            }

            menuItems.AddRange(_steamComponentMenuItems);
            return menuItems;
        }

        private static void AddSeparatorToMenuItems(List<GameMenuItem> menuItems, string menuSection = "")
        {
            menuItems.Add(new GameMenuItem { Description = "-", MenuSection = menuSection });
        }

        private void AddSteamClientMenuItems(List<GameMenuItem> menuItems, string gameId)
        {
            menuItems.AddRange(new List<GameMenuItem>
            {
                CreateSteamClientMenuItem("LOCSteam_Viewer_MenuItemGameLibraryDetails", SteamClientGameUriType.Details, gameId, '\uEF65'),
                CreateSteamClientMenuItem("LOCSteam_Viewer_MenuItemGameSteamInput", SteamClientGameUriType.SteamInput, gameId, '\uEA30'),
                CreateSteamClientMenuItem("LOCSteam_Viewer_MenuItemGameProperties", SteamClientGameUriType.GameProperties, gameId, '\uEF75')
            });
        }

        private void AddSteamWebLinkMenuItems(List<GameMenuItem> menuItems, string steamId)
        {
            menuItems.AddRange(new List<GameMenuItem>
            {
                CreateSteamWebLinkMenuItem("LOCSteam_Viewer_MenuItemGameStorePageDescription", SteamUrlType.StorePage, steamId, '\uEFE7'),
                CreateSteamWebLinkMenuItem("LOCSteam_Viewer_MenuItemGameCommunityHubDescription", SteamUrlType.CommunityHub, steamId),
                CreateSteamWebLinkMenuItem("LOCSteam_Viewer_MenuItemGameDiscussionsDescription", SteamUrlType.Discussions, steamId, '\uED44'),
                CreateSteamWebLinkMenuItem("LOCSteam_Viewer_MenuItemGameGuidesDescription", SteamUrlType.Guides, steamId, '\uEF8B'),
                CreateSteamWebLinkMenuItem("LOCSteam_Viewer_MenuItemGameAchievementsDescription", SteamUrlType.Achievements, steamId, '\uEDD7'),
                CreateSteamWebLinkMenuItem("LOCSteam_Viewer_MenuItemGameNewsDescription", SteamUrlType.News, steamId, '\uEFA7'),
                CreateSteamWebLinkMenuItem("LOCSteam_Viewer_MenuItemGamePointsShopDescription", SteamUrlType.PointsShop, steamId, '\uEFE7')
            });
        }

        private List<GameMenuItem> GetSteamComponentMenuItems()
        {
            return new List<GameMenuItem>
            {
                CreateMenuItem("LOCSteam_Viewer_MenuItemComponentActivateProductDescription", SteamComponentType.ActivateProduct, '\uE963'),
                CreateMenuItem("LOCSteam_Viewer_MenuItemComponentDownloadsDescription", SteamComponentType.Downloads, '\uEF08'),
                CreateMenuItem("LOCSteam_Viewer_MenuItemComponentFriendsDescription", SteamComponentType.Friends, '\uECF9'),
                CreateMenuItem("LOCSteam_Viewer_MenuItemComponentNewsDescription", SteamComponentType.News, '\uEFA7'),
                CreateMenuItem("LOCSteam_Viewer_MenuItemComponentScreenshotsDescription", SteamComponentType.Screenshots, '\uEF4B'),
                CreateMenuItem("LOCSteam_Viewer_MenuItemComponentSettingsDescription", SteamComponentType.Settings, '\uEFE1')
            };
        }

        private GameMenuItem CreateMenuItem(string descriptionKey, SteamComponentType componentType, char icoChar = '\uE93E')
        {
            return new GameMenuItem
            {
                Description = ResourceProvider.GetString(descriptionKey),
                Icon = PlayniteUtilities.GetIcoFontGlyphResource(icoChar),
                MenuSection = $"{_menuSection}",
                Action = _ => _steamUriLauncherService.LaunchSteamComponent(componentType)
            };
        }

        private GameMenuItem CreateSteamClientMenuItem(string descriptionKey, SteamClientGameUriType type, string steamId, char icoChar = '\uE93E')
        {
            return new GameMenuItem
            {
                Description = ResourceProvider.GetString(descriptionKey),
                Icon = PlayniteUtilities.GetIcoFontGlyphResource(icoChar),
                Action = _ => _steamUriLauncherService.LaunchSteamClientUri(type, steamId),
                MenuSection = _menuSection
            };
        }

        private GameMenuItem CreateSteamWebLinkMenuItem(string descriptionKey, SteamUrlType type, string steamId, char icoChar = '\uE93E')
        {
            return new GameMenuItem
            {
                Description = ResourceProvider.GetString(descriptionKey),
                Icon = PlayniteUtilities.GetIcoFontGlyphResource(icoChar),
                Action = _ => _steamUriLauncherService.LaunchSteamWebUrl(type, steamId),
                MenuSection = _menuSection
            };
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return _settingsViewModel;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new SteamShortcutsSettingsView();
        }
    }

}