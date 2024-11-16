using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PluginsCommon;
using SteamViewer.Application;
using SteamViewer.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SteamViewer
{
    public class SteamViewer : GenericPlugin
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private static readonly Guid _steamLibraryPluginId = Guid.Parse("CB91DFC9-B977-43BF-8E70-55F46E410FAB");
        private readonly SteamViewerSettingsViewModel _settingsViewModel;
        private readonly List<GameMenuItem> _steamComponentMenuItems;
        private const string _menuSection = "Steam Viewer";
        public override Guid Id { get; } = Guid.Parse("0a3edabb-065f-4056-a294-d6bc0656e2ac");

        public SteamViewer(IPlayniteAPI api) : base(api)
        {
            _settingsViewModel = new SteamViewerSettingsViewModel(this);
            Properties = new GenericPluginProperties { HasSettings = false };
            _steamComponentMenuItems = GetSteamComponentMenuItems();
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var game = args.Games[0];
            var menuItems = new List<GameMenuItem>();
            if (args.Games.Count == 1 && game.PluginId == _steamLibraryPluginId)
            {
                menuItems.AddRange(new List<GameMenuItem>
                {
                    CreateSteamClientMenuItem("LOCSteam_Viewer_MenuItemGameLibraryDetails", SteamClientGameUriType.Details, game),
                    CreateSteamClientMenuItem("LOCSteam_Viewer_MenuItemGameSteamInput", SteamClientGameUriType.SteamInput, game),
                    CreateSteamClientMenuItem("LOCSteam_Viewer_MenuItemGameProperties", SteamClientGameUriType.GameProperties, game)
                });

                AddSeparatorToMenuItems(menuItems, _menuSection);
                menuItems.AddRange(new List<GameMenuItem>
                {
                    CreateSteamWebLinkMenuItem("LOCSteam_Viewer_MenuItemGameStorePageDescription", SteamUrlType.StorePage, game),
                    CreateSteamWebLinkMenuItem("LOCSteam_Viewer_MenuItemGameCommunityHubDescription", SteamUrlType.CommunityHub, game),
                    CreateSteamWebLinkMenuItem("LOCSteam_Viewer_MenuItemGameDiscussionsDescription", SteamUrlType.Discussions, game),
                    CreateSteamWebLinkMenuItem("LOCSteam_Viewer_MenuItemGameGuidesDescription", SteamUrlType.Guides, game),
                    CreateSteamWebLinkMenuItem("LOCSteam_Viewer_MenuItemGameAchievementsDescription", SteamUrlType.Achievements, game),
                    CreateSteamWebLinkMenuItem("LOCSteam_Viewer_MenuItemGameNewsDescription", SteamUrlType.News, game),
                    CreateSteamWebLinkMenuItem("LOCSteam_Viewer_MenuItemGamePointsShopDescription", SteamUrlType.PointsShop, game)
                });

                AddSeparatorToMenuItems(menuItems, _menuSection);
            }

            menuItems.AddRange(_steamComponentMenuItems);
            return menuItems;
        }

        private void AddSeparatorToMenuItems(List<GameMenuItem> menuItems, string menuSection = "")
        {
            menuItems.Add(new GameMenuItem { Description = "-", MenuSection = menuSection });
        }

        private List<GameMenuItem> GetSteamComponentMenuItems()
        {
            var subSection = ResourceProvider.GetString("LOCSteam_Viewer_MenuItemComponentsSection");
            return new List<GameMenuItem>
            {
                CreateMenuItem("LOCSteam_Viewer_MenuItemComponentActivateProductDescription", SteamComponentType.ActivateProduct, subSection),
                CreateMenuItem("LOCSteam_Viewer_MenuItemComponentDownloadsDescription", SteamComponentType.Downloads, subSection),
                CreateMenuItem("LOCSteam_Viewer_MenuItemComponentFriendsDescription", SteamComponentType.Friends, subSection),
                CreateMenuItem("LOCSteam_Viewer_MenuItemComponentNewsDescription", SteamComponentType.News, subSection),
                CreateMenuItem("LOCSteam_Viewer_MenuItemComponentScreenshotsDescription", SteamComponentType.Screenshots, subSection),
                CreateMenuItem("LOCSteam_Viewer_MenuItemComponentSettingsDescription", SteamComponentType.Settings, subSection)
            };
        }

        private GameMenuItem CreateMenuItem(string descriptionKey, SteamComponentType componentType, string section)
        {
            return new GameMenuItem
            {
                Description = ResourceProvider.GetString(descriptionKey),
                MenuSection = $"{_menuSection}",
                Action = _ => SteamLauncher.LaunchSteamComponent(componentType)
            };
        }

        private GameMenuItem CreateSteamClientMenuItem(string descriptionKey, SteamClientGameUriType type, Game game)
        {
            return new GameMenuItem
            {
                Description = ResourceProvider.GetString(descriptionKey),
                Action = _ => SteamLauncher.LaunchSteamClientUri(type, game.GameId),
                MenuSection = _menuSection
            };
        }

        private GameMenuItem CreateSteamWebLinkMenuItem(string descriptionKey, SteamUrlType type, Game game)
        {
            return new GameMenuItem
            {
                Description = ResourceProvider.GetString(descriptionKey),
                Action = _ => SteamLauncher.LaunchSteamWebUrl(type, game.GameId),
                MenuSection = _menuSection
            };
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new SteamViewerSettingsView();
        }
    }

}