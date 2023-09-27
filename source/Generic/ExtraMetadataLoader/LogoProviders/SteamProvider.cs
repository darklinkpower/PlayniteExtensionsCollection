using ExtraMetadataLoader.Helpers;
using ExtraMetadataLoader.Interfaces;
using Playnite.SDK;
using Playnite.SDK.Models;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using SteamCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebCommon;

namespace ExtraMetadataLoader.LogoProviders
{
    public class SteamProvider : ILogoProvider
    {
        private readonly IPlayniteAPI playniteApi;
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly ExtraMetadataLoaderSettings settings;
        private const string steamLogoUriTemplate = @"https://steamcdn-a.akamaihd.net/steam/apps/{0}/logo.png";
        public string Id => "steamProvider";

        public SteamProvider(IPlayniteAPI playniteApi, ExtraMetadataLoaderSettings settings)
        {
            this.playniteApi = playniteApi;
            this.settings = settings;
        }

        public string GetLogoUrl(Game game, bool isBackgroundDownload, CancellationToken cancelToken = default)
        {
            var steamId = string.Empty;
            if (Steam.IsGameSteamGame(game) || (!settings.SteamDlOnlyProcessPcGames || PlayniteUtilities.IsGamePcGame(game)))
            {
                steamId = Steam.GetGameSteamId(game, true);
            }

            if (steamId.IsNullOrEmpty())
            {
                return null;
            }

            return string.Format(steamLogoUriTemplate, steamId);

        }
    }
}
