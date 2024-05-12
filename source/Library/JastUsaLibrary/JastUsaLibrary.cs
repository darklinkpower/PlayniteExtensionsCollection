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
using System.Collections.ObjectModel;
using JastUsaLibrary.ProgramsHelper;
using PlayniteUtilitiesCommon;

namespace JastUsaLibrary
{
    public class JastUsaLibrary : LibraryPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private JastUsaLibrarySettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("d407a620-5953-4ca4-a25c-8194c8559381");
        public override string LibraryIcon { get; }

        private readonly SidebarItem _sidebarLibraryManagerView;

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
            _sidebarLibraryManagerView = new SidebarItem
            {
                Title = ResourceProvider.GetString("LOC_JUL_JastLibraryManager"),
                Type = SiderbarItemType.View,
                Icon = LibraryIcon,
                ProgressValue = 0,
                ProgressMaximum = 100,
                Opened = () => {
                    _downloadsManagerViewModel.RefreshLibraryGames();
                    return new DownloadsManagerView { DataContext = _downloadsManagerViewModel };
                },
                Closed = () => {

                }
            };

            _downloadsManagerViewModel.GlobalProgressChanged += DownloadsManagerViewModel_GlobalProgressChanged;
        }

        private void DownloadsManagerViewModel_GlobalProgressChanged(object sender, GlobalProgressChangedEventArgs e)
        {
            if (e.TotalItems == 0 || !e.TotalDownloadProgress.HasValue)
            {
                _sidebarLibraryManagerView.ProgressValue = 0;
            }
            else
            {
                _sidebarLibraryManagerView.ProgressValue = e.TotalDownloadProgress.Value;
            }
        }

        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            var games = new List<GameMetadata>();
            var authenticationToken = _accountClient.GetAuthenticationToken();
            if (authenticationToken is null) // User is not logged in
            {
                PlayniteApi.Notifications.Add(new NotificationMessage("JastNotLoggedIn", "JAST USA Library: " + ResourceProvider.GetString("LOC_JUL_DialogMessageNotAuthenticated"), NotificationType.Error, () => OpenSettingsView()));
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

                var assets = new ObservableCollection<JastAssetWrapper>();
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
                _ = Task.Run(() => _downloadsManagerViewModel.StartDownloadsAsync(false, false));
            }
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var game = args.Games.Last();
            if (args.Games.Count == 1 && game.PluginId == Id && settings.Settings.LibraryCache.TryGetValue(game.GameId, out var gameCache))
            {
                const string menuSection = "JAST USA Library";
                return new List<GameMenuItem>
                {
                    new GameMenuItem
                    {
                        Description = ResourceProvider.GetString("LOC_JUL_DialogMessageBrowseForGameOption"),
                        MenuSection = menuSection,
                        Icon = PlayniteUtilities.GetIcoFontGlyphResource('\uEC5B'),
                        Action = a =>
                        {
                            var selectedProgram = Programs.SelectExecutable();
                            if (selectedProgram != null)
                            {
                                gameCache.Program = selectedProgram;
                                SavePluginSettings();
                                game.IsInstalled = true;
                                game.InstallDirectory = Path.GetDirectoryName(gameCache.Program.Path);
                                PlayniteApi.Database.Games.Update(game);
                            }
                        }
                    }
                };
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
            var game = args.Game;
            if (args.Game.PluginId == Id)
            {
                if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen)
                {
                    PlayniteApi.Dialogs.ShowErrorMessage(ResourceProvider.GetString("LOC_JUL_MessageInstallUnavailableFsMode"), "JAST USA Library");
                    return base.GetInstallActions(args);
                }

                if (settings.Settings.LibraryCache.TryGetValue(game.GameId, out var gameCache) && gameCache.Product != null)
                {
                    return new List<InstallController> { new JastInstallController(game, gameCache, _downloadsManagerViewModel, this) };
                }
            }

            return base.GetInstallActions(args);
        }

        public override IEnumerable<UninstallController> GetUninstallActions(GetUninstallActionsArgs args)
        {
            var game = args.Game;
            if (args.Game.PluginId == Id)
            {
                if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen)
                {
                    PlayniteApi.Dialogs.ShowErrorMessage(ResourceProvider.GetString("LOC_JUL_MessageInstallUnavailableFsMode"), "JAST USA Library");
                    return base.GetUninstallActions(args);
                }

                if (settings.Settings.LibraryCache.TryGetValue(game.GameId, out var gameCache) &&
                    gameCache.Product != null && FileSystem.FileExists(gameCache.Program.Path))
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
                PlayniteApi.Notifications.Add(new NotificationMessage("JastNotLoggedIn", "JAST USA Library: " + ResourceProvider.GetString("LOC_JUL_DialogMessageNotAuthenticated"), NotificationType.Error, () => OpenSettingsView()));
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
            yield return _sidebarLibraryManagerView;
        }

    }
}