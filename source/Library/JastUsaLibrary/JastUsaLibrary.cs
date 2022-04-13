using JastUsaLibrary.Models;
using JastUsaLibrary.Services;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace JastUsaLibrary
{
    public class JastUsaLibrary : LibraryPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private JastUsaLibrarySettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("d407a620-5953-4ca4-a25c-8194c8559381");

        // Change to something more appropriate
        public override string Name => "JAST USA";

        // Implementing Client adds ability to open it via special menu in playnite.
        public override LibraryClient Client { get; } = new JastUsaLibraryClient();
        public JastUsaAccountClient AccountClient;
        private readonly string userGamesCachePath;
        private readonly string tokensPath;
        private const string jastMediaUrlTemplate = @"https://app.jastusa.com/media/image/{0}";

        public JastUsaLibrary(IPlayniteAPI api) : base(api)
        {
            userGamesCachePath = Path.Combine(GetPluginUserDataPath(), "userGamesCache.json");
            tokensPath = Path.Combine(GetPluginUserDataPath(), "tokens.json");
            AccountClient = new JastUsaAccountClient(api, tokensPath);
            settings = new JastUsaLibrarySettingsViewModel(this, PlayniteApi, AccountClient);
            Properties = new LibraryPluginProperties
            {
                HasSettings = true
            };
        }

        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            var games = new List<GameMetadata>();
            if (!AccountClient.GetIsUserLoggedIn())
            {
                PlayniteApi.Notifications.Add(new NotificationMessage("JastNotLoggedIn", "User is not logged in", NotificationType.Error, () => OpenSettingsView()));
                return games;
            }

            var jastProducts = AccountClient.GetGames();
            if (jastProducts.Count > 0)
            {
                FileSystem.WriteStringToFile(userGamesCachePath, Serialization.ToJson(jastProducts), true);
            }

            foreach (var jastProduct in jastProducts)
            {
                if (!jastProduct.ProductVariant.Platforms.Any(x => x.Any(y => y.Value == "Windows")))
                {
                    continue;
                }

                games.Add(new GameMetadata
                {
                    Name = jastProduct.ProductVariant.ProductName.RemoveTrademarks(),
                    GameId = jastProduct.ProductVariant.GameId.ToString(),
                    BackgroundImage = new MetadataFile(string.Format(jastMediaUrlTemplate, jastProduct.ProductVariant.ProductImageBackground)),
                    CoverImage = new MetadataFile(string.Format(jastMediaUrlTemplate, jastProduct.ProductVariant.ProductImage)),
                    Platforms = new HashSet<MetadataProperty> { new MetadataSpecProperty("pc_windows") }
                });
            }

            return games;
        }

        public override LibraryMetadataProvider GetMetadataDownloader()
        {
            return new JastUsaLibraryMetadataProvider(userGamesCachePath);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new JastUsaLibrarySettingsView();
        }
    }
}