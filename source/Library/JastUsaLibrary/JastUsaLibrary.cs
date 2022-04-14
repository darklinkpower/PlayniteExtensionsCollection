using JastUsaLibrary.Models;
using JastUsaLibrary.Services;
using JastUsaLibrary.ViewModels;
using JastUsaLibrary.Views;
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
using System.Windows;
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
        private readonly string authenticationPath;
        private const string jastMediaUrlTemplate = @"https://app.jastusa.com/media/image/{0}";

        public JastUsaLibrary(IPlayniteAPI api) : base(api)
        {
            userGamesCachePath = Path.Combine(GetPluginUserDataPath(), "userGamesCache.json");
            authenticationPath = Path.Combine(GetPluginUserDataPath(), "authentication.json");
            AccountClient = new JastUsaAccountClient(api, authenticationPath);
            settings = new JastUsaLibrarySettingsViewModel(this, PlayniteApi, AccountClient);
            Properties = new LibraryPluginProperties
            {
                HasSettings = true
            };
        }

        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            var games = new List<GameMetadata>();
            var authenticationToken = AccountClient.GetAuthenticationToken();
            if (authenticationToken == null) // User is not logged in
            {
                PlayniteApi.Notifications.Add(new NotificationMessage("JastNotLoggedIn", "User is not logged in", NotificationType.Error, () => OpenSettingsView()));
                return games;
            }

            var jastProducts = AccountClient.GetGames(authenticationToken);
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

        public override IEnumerable<InstallController> GetInstallActions(GetInstallActionsArgs args)
        {
            if (args.Game.PluginId != Id)
            {
                return null;
            }

            var options = new List<MessageBoxOption>
            {
                new MessageBoxOption("Download"),
                new MessageBoxOption("Select installed game"),
                new MessageBoxOption("Cancel", false, true),
            };

            var game = args.Game;
            var selected = PlayniteApi.Dialogs.ShowMessage("Select action", "JAST USA Library", System.Windows.MessageBoxImage.None, options);
            if (!selected.IsCancel)
            {
                if (selected == options[0]) // Download option
                {
                    OpenGameDownloadsWindow(game);
                }
                else if (selected == options[1]) // Select install option
                {

                }
            }


            return null;
        }

        private List<InstallController> GetFakeController(Game game)
        {
            return new List<InstallController> { new FakeInstallController(game) };
        }

        private GameTranslationsResponse GetGameTranslations(Game game)
        {
            if (!FileSystem.FileExists(userGamesCachePath))
            {
                return null;
            }

            var authenticationToken = AccountClient.GetAuthenticationToken();
            if (authenticationToken == null) // User is not logged in
            {
                PlayniteApi.Notifications.Add(new NotificationMessage("JastNotLoggedIn", "User is not logged in", NotificationType.Error, () => OpenSettingsView()));
                return null;
            }

            var cache = Serialization.FromJsonFile<List<JastProduct>>(userGamesCachePath);
            var gameVariant = cache.FirstOrDefault(x => x.ProductVariant.GameId.ToString() == game.GameId);
            if (gameVariant == null)
            {
                return null;
            }

            var gameTranslations = gameVariant.ProductVariant.Game.Translations.Where(x => x.Key == "en_US");
            if (gameTranslations.Count() == 0)
            {
                return null;
            }

            return AccountClient.GetGameTranslations(authenticationToken, gameTranslations.First().Value.Id);
        }

        private void OpenGameDownloadsWindow(Game game)
        {
            var gameTranslations = GetGameTranslations(game);
            if (gameTranslations == null)
            {
                return;
            }

            var window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = true
            });

            window.Height = 700;
            window.Width = 900;
            window.Title = "JAST USA Downloader";

            window.Content = new GameDownloadsView();
            window.DataContext = new GameDownloadsViewModel(game, gameTranslations, AccountClient);
            window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            window.ShowDialog();
        }
    }


}