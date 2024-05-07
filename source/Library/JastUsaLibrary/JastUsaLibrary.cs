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
using JastUsaLibrary.ProgramsHelper.Models;
using System.Reflection;
using Playnite.SDK.Events;
using JastUsaLibrary.DownloadManager.ViewModels;
using JastUsaLibrary.DownloadManager.Models;
using JastUsaLibrary.DownloadManager.Enums;
using System.IO.Compression;
using JastUsaLibrary.DownloadManager.Views;

namespace JastUsaLibrary
{
    public class JastUsaLibrary : LibraryPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private JastUsaLibrarySettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("d407a620-5953-4ca4-a25c-8194c8559381");
        public override string LibraryIcon { get; }
        public override string Name => "JAST USA";
        public override LibraryClient Client { get; } = new JastUsaLibraryClient();
        private readonly JastUsaAccountClient _accountClient;
        private readonly DownloadsManagerViewModel _downloadsManagerViewModel;
        private readonly string _userGamesCachePath;
        public string UserGamesCachePath => _userGamesCachePath;
        private readonly string _authenticationPath;

        public JastUsaLibrary(IPlayniteAPI api) : base(api)
        {
            _userGamesCachePath = Path.Combine(GetPluginUserDataPath(), "userGamesCache.json");
            _authenticationPath = Path.Combine(GetPluginUserDataPath(), "authentication.json");
            _accountClient = new JastUsaAccountClient(api, _authenticationPath);
            settings = new JastUsaLibrarySettingsViewModel(this, PlayniteApi, _accountClient);
            _downloadsManagerViewModel = new DownloadsManagerViewModel(this, _accountClient, settings);
            Properties = new LibraryPluginProperties
            {
                HasSettings = true
            };

            LibraryIcon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"icon.png");
        }


        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            var games = new List<GameMetadata>();
            var authenticationToken = _accountClient.GetAuthenticationToken();
            if (authenticationToken is null) // User is not logged in
            {
                PlayniteApi.Notifications.Add(new NotificationMessage("JastNotLoggedIn", "JAST USA Library: " + ResourceProvider.GetString("LOCJast_Usa_Library_DialogMessageNotAuthenticated"), NotificationType.Error, () => OpenSettingsView()));
                return games;
            }

            var jastProducts = _accountClient.GetGames(authenticationToken);
            if (jastProducts.Count > 0)
            {
                FileSystem.WriteStringToFile(_userGamesCachePath, Serialization.ToJson(jastProducts), true);
            }

            var libraryCache = settings.Settings.LibraryCache;
            var saveSettings = false;
            foreach (var kv in libraryCache)
            {
                var gameVariant = jastProducts.FirstOrDefault(x => x.ProductVariant.GameId.ToString() == kv.Value.GameId);
                if (gameVariant != null && (kv.Value.Product is null || Serialization.ToJson(kv.Value.Product) != Serialization.ToJson(gameVariant))) // TODO Do this properly
                {
                    kv.Value.Product = gameVariant;
                    saveSettings = true;
                }
            }

            foreach (var jastProduct in jastProducts)
            {
                if (!jastProduct.ProductVariant.Platforms.Any(x => x.Any(y => y.Value == JastPlatform.Windows)))
                {
                    continue;
                }

                var game = new GameMetadata
                {
                    Name = jastProduct.ProductVariant.ProductName.RemoveTrademarks(),
                    GameId = jastProduct.ProductVariant.GameId.ToString(),
                    Platforms = new HashSet<MetadataProperty> { new MetadataSpecProperty("pc_windows") },
                    Source = new MetadataNameProperty("JAST USA")
                };

                if (libraryCache.TryGetValue(game.GameId, out var existingCache))
                {
                    if (existingCache.Program?.Path.IsNullOrEmpty() == false && FileSystem.FileExists(existingCache.Program?.Path))
                    {
                        game.InstallDirectory = Path.GetDirectoryName(existingCache.Program.Path);
                        game.IsInstalled = true;
                    }
                }
                else
                {
                    var newLibraryCache = new GameCache
                    {
                        GameId = game.GameId,
                        Product = jastProduct
                    };

                    libraryCache[game.GameId] = newLibraryCache;
                    saveSettings = true;
                }

                games.Add(game);
            }

            foreach (var cache in libraryCache.Values)
            {
                if (cache.Assets != null)
                {
                    continue;
                }

                var assets = new List<JastAssetWrapper>().ToObservable();
                var gameTranslations = cache.Product.ProductVariant.Game.Translations.Where(x => x.Key == Locale.En_Us);
                if (gameTranslations.HasItems())
                {
                    var response = _accountClient.GetGameTranslations(authenticationToken, gameTranslations.First().Value.Id);
                    if (response != null)
                    {
                        assets = (new[]
                        {
                            response.GamePathLinks?.Select(x => new JastAssetWrapper(x, JastAssetType.Game)) ?? Enumerable.Empty<JastAssetWrapper>(),
                            response.GameExtraLinks?.Select(x => new JastAssetWrapper(x, JastAssetType.Extra)) ?? Enumerable.Empty<JastAssetWrapper>(),
                            response.GamePatchLinks?.Select(x => new JastAssetWrapper(x, JastAssetType.Patch)) ?? Enumerable.Empty<JastAssetWrapper>()
                        })
                        .SelectMany(x => x)
                        .ToObservable();
                    }
                }

                cache.Assets = assets;
                saveSettings = true;
            }

            if (saveSettings)
            {
                SavePluginSettings();
            }

            return games;
        }

        public override LibraryMetadataProvider GetMetadataDownloader()
        {
            return new JastUsaLibraryMetadataProvider(_userGamesCachePath);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new JastUsaLibrarySettingsView();
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            settings.UpgradeSettings();
            if (settings.Settings.StartDownloadsOnStartup)
            {
                _ = Task.Run(() => _downloadsManagerViewModel.StartDownloadsAsync());
            }
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var game = args.Games.Last();
            if (game.PluginId == Id)
            {

            }

            return base.GetGameMenuItems(args);
        }

        public override IEnumerable<PlayController> GetPlayActions(GetPlayActionsArgs args)
        {
            var game = args.Game;
            if (game.PluginId == Id)
            {
                if (settings.Settings.LibraryCache.TryGetValue(game.GameId, out var gameCache) && gameCache.Product != null)
                {
                    return new List<PlayController>
                    {
                        new AutomaticPlayController(game)
                        {
                            Name = gameCache.Program.Name,
                            Path = gameCache.Program.Path,
                            Arguments = gameCache.Program.Arguments,
                            WorkingDir = gameCache.Program.WorkDir,
                            TrackingMode = TrackingMode.Default
                        }
                    };
                }
            }

            return base.GetPlayActions(args);
        }

        public override IEnumerable<InstallController> GetInstallActions(GetInstallActionsArgs args)
        {
            if (args.Game.PluginId != Id)
            {
                return base.GetInstallActions(args);
            }

            var options = new List<MessageBoxOption>
            {
                new MessageBoxOption(ResourceProvider.GetString("LOCJast_Usa_Library_DialogMessageDownloadOption")),
                new MessageBoxOption(ResourceProvider.GetString("LOCJast_Usa_Library_DialogMessageBrowseForGameOption")),
                //new MessageBoxOption(ResourceProvider.GetString("LOCJast_Usa_Library_DialogMessageSelectInstalledOption")),
                new MessageBoxOption(ResourceProvider.GetString("LOCJast_Usa_Library_DialogMessageCancelOption"), false, true)
            };

            var game = args.Game;
            var selected = PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCJast_Usa_Library_DialogMessageSelectActionLabel"), "JAST USA Library", MessageBoxImage.None, options);
            if (!selected.IsCancel)
            {
                if (selected == options[0]) // Download option
                {
                    // OpenGameDownloadsWindow(game);
                }
                else if (selected == options[1]) // Browse for executable option
                {
                    var selectedProgram = ProgramsHelper.Programs.SelectExecutable();
                    if (selectedProgram is null)
                    {
                        return null;
                    }

                    //return AddGameToCache(game, selectedProgram);
                }
                //else if (selected == options[2]) // Select install option
                //{

                //}
            }


            return base.GetInstallActions(args);
        }

        public override IEnumerable<UninstallController> GetUninstallActions(GetUninstallActionsArgs args)
        {
            var game = args.Game;
            if (args.Game.PluginId == Id && settings.Settings.LibraryCache.TryGetValue(game.GameId, out var gameCache))
            {
                if (gameCache.Product != null && FileSystem.FileExists(gameCache.Program.Path))
                {
                    return new List<UninstallController> { new JastUninstallController(game, gameCache, this) };
                }
            }

            return base.GetUninstallActions(args);
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            _downloadsManagerViewModel.StopDownloadsAndPersistDownloadData();
            _downloadsManagerViewModel.Dispose();
        }

        private List<InstallController> GetFakeController(Game game, string installDir)
        {
            return new List<InstallController> { new JastInstallController(game, installDir) };
        }

        public GameTranslationsResponse GetGameTranslations(Game game)
        {
            if (!FileSystem.FileExists(_userGamesCachePath))
            {
                return null;
            }

            var authenticationToken = _accountClient.GetAuthenticationToken();
            PlayniteApi.Notifications.Remove("JastNotLoggedIn");
            if (authenticationToken is null) // User is not logged in
            {
                PlayniteApi.Notifications.Add(new NotificationMessage("JastNotLoggedIn", "JAST USA Library: " + ResourceProvider.GetString("LOCJast_Usa_Library_DialogMessageNotAuthenticated"), NotificationType.Error, () => OpenSettingsView()));
                return null;
            }

            var cache = Serialization.FromJsonFile<List<JastProduct>>(_userGamesCachePath);
            var gameVariant = cache.FirstOrDefault(x => x.ProductVariant.GameId.ToString() == game.GameId);
            if (gameVariant is null)
            {
                return null;
            }

            var gameTranslations = gameVariant.ProductVariant.Game.Translations.Where(x => x.Key == Locale.En_Us);
            if (!gameTranslations.HasItems())
            {
                return null;
            }

            return _accountClient.GetGameTranslations(authenticationToken, gameTranslations.First().Value.Id);
        }

        public void SavePluginSettings()
        {
            SavePluginSettings(settings.Settings);
        }

        public override IEnumerable<SidebarItem> GetSidebarItems()
        {
            var iconPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"icon.png");
            yield return new SidebarItem
            {
                Title = ResourceProvider.GetString("LOCJast_Usa_Library_JastLibraryManager"),
                Type = SiderbarItemType.View,
                Icon = iconPath,
                Opened = () => {
                    _downloadsManagerViewModel.RefreshLibraryGames();
                    return new DownloadsManagerView { DataContext = _downloadsManagerViewModel };
                },
                Closed = () => {

                }
            };
        }



    }


}