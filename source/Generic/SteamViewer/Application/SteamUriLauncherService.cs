using PluginsCommon;
using SteamShortcuts.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamShortcuts.Application
{
    public class SteamUriLauncherService
    {
        private bool _launchUrlsInSteamClient = true;

        private static readonly Dictionary<SteamClientGameUriType, string> _steamClientGameUriTemplates = new Dictionary<SteamClientGameUriType, string>()
        {
            { SteamClientGameUriType.Details, "steam://nav/games/details/{0}" },
            { SteamClientGameUriType.SteamInput, "steam://controllerconfig/{0}" },
            { SteamClientGameUriType.GameProperties, "steam://gameproperties/{0}" }
        };

        private static readonly Dictionary<SteamUrlType, string> _steamGameUrlTemplates = new Dictionary<SteamUrlType, string>()
        {
            { SteamUrlType.StorePage, "https://store.steampowered.com/app/{0}/" },
            { SteamUrlType.CommunityHub, "https://steamcommunity.com/app/{0}/" },
            { SteamUrlType.Discussions, "https://steamcommunity.com/app/{0}/discussions/" },
            { SteamUrlType.Guides, "https://steamcommunity.com/app/{0}/guides/" },
            { SteamUrlType.Achievements, "https://steamcommunity.com/stats/{0}/achievements/" },
            { SteamUrlType.News, "https://store.steampowered.com/news/?appids={0}" },
            { SteamUrlType.PointsShop, "https://store.steampowered.com/points/shop/app/{0}" }
        };

        private static readonly Dictionary<SteamComponentType, string> _steamComponentTemplates = new Dictionary<SteamComponentType, string>()
        {
            { SteamComponentType.ActivateProduct, "steam://open/activateproduct" },
            { SteamComponentType.Downloads, "steam://open/downloads" },
            { SteamComponentType.Friends, "steam://open/friends" },
            { SteamComponentType.News, "steam://open/news" },
            { SteamComponentType.Screenshots, "steam://open/screenshots" },
            { SteamComponentType.Settings, "steam://open/settings" }
        };

        public bool LaunchUrlsInSteamClient
        {
            get => _launchUrlsInSteamClient;
            set => _launchUrlsInSteamClient = value;
        }

        public SteamUriLauncherService()
        {

        }

        public void LaunchSteamComponent(SteamComponentType componentType)
        {
            if (_steamComponentTemplates.TryGetValue(componentType, out var uri))
            {
                ProcessStarter.StartUrl(uri);
            }
        }

        public void LaunchSteamClientUri(SteamClientGameUriType uriType, string steamId)
        {
            if (_steamClientGameUriTemplates.TryGetValue(uriType, out var urlTemplate))
            {
                var url = string.Format(urlTemplate, steamId);
                ProcessStarter.StartUrl(url);
            }
        }

        public void LaunchSteamWebUrl(SteamUrlType urlType, string steamId)
        {
            if (_steamGameUrlTemplates.TryGetValue(urlType, out var urlTemplate))
            {
                var url = string.Format(urlTemplate, steamId);
                if (_launchUrlsInSteamClient)
                {
                    ProcessStarter.StartUrl("steam://openurl/" + url);
                }
                else
                {
                    ProcessStarter.StartUrl(url);
                }
            }
        }
    }

}