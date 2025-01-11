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
using System.Reflection;
using Playnite.SDK.Events;
using JastUsaLibrary.DownloadManager.Views;
using JastUsaLibrary.ProgramsHelper;
using PlayniteUtilitiesCommon;
using JastUsaLibrary.JastUsaIntegration.Infrastructure.Persistence;
using JastUsaLibrary.JastUsaIntegration.Application.Services;
using JastUsaLibrary.JastUsaIntegration.Infrastructure.External;
using JastUsaLibrary.DownloadManager.Domain.Events;
using JastUsaLibrary.JastLibraryCacheService.Application;
using JastUsaLibrary.DownloadManager.Presentation;
using JastUsaLibrary.Features.MetadataProvider;
using JastUsaLibrary.Features.DownloadManager.Application;
using JastUsaLibrary.Features.DownloadManager.Infrastructure;
using JastUsaLibrary.Services.JastLibraryCacheService.Entities;
using JastUsaLibrary.Services.GameInstallationManager.Application;
using JastUsaLibrary.Services.GameInstallationManager.Infrastructure;
using JastUsaLibrary.Services.JastLibraryCacheService.Infrastructure;

namespace JastUsaLibrary
{
    public class JastUsaLibrary : LibraryPlugin
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private JastUsaLibrarySettingsViewModel settings { get; set; }

        private readonly IGameInstallationManagerService _gameInstallationManagerService;

        public override Guid Id { get; } = Guid.Parse("d407a620-5953-4ca4-a25c-8194c8559381");
        public override string LibraryIcon { get; }

        private readonly string _sidebarBaseItemTitle;
        private readonly SidebarItem _sidebarLibraryManagerView;

        public override string Name => "JAST USA";
        public override LibraryClient Client { get; } = new JastUsaLibraryClient();
        private readonly JastUsaAccountClient _jastUsaAccountClient;
        private readonly ILibraryCacheService _jastUsaCacheService;
        private readonly DownloadsManager _downloadsManager;
        private DownloadsManagerViewModel _downloadsManagerViewModel;

        public JastUsaLibrary(IPlayniteAPI api) : base(api)
        {
            var apiClient = new JastUsaApiClient(_logger);
            var authenticationPersistence = new AuthenticationPersistence(GetPluginUserDataPath());
            _jastUsaAccountClient = new JastUsaAccountClient(api, apiClient, authenticationPersistence);
            settings = new JastUsaLibrarySettingsViewModel(this, PlayniteApi, _jastUsaAccountClient);
            _gameInstallationManagerService = new GameInstallationManagerService(PlayniteApi, new GameInstallationManagerPersistenceJson(GetPluginUserDataPath(), _logger));
            _jastUsaCacheService = new LibraryCacheService(PlayniteApi, new LibraryCachePersistenceJson(GetPluginUserDataPath(), _logger), Id);
            _downloadsManager = new DownloadsManager(this, _jastUsaAccountClient, settings, _logger, PlayniteApi, new DownloadDataPersistenceJson(_logger, GetPluginUserDataPath()), _jastUsaCacheService, _gameInstallationManagerService);
            Properties = new LibraryPluginProperties
            {
                HasSettings = true
            };

            LibraryIcon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"icon.png");
            _sidebarBaseItemTitle = ResourceProvider.GetString("LOC_JUL_JastLibraryManager");
            _sidebarLibraryManagerView = new SidebarItem
            {
                Title = _sidebarBaseItemTitle,
                Type = SiderbarItemType.View,
                Icon = LibraryIcon,
                ProgressValue = 0,
                ProgressMaximum = 100,
                Opened = () => {
                    _downloadsManagerViewModel = new DownloadsManagerViewModel(
                        this,
                        _jastUsaAccountClient,
                        settings,
                        PlayniteApi,
                        _logger,
                        _jastUsaCacheService,
                        _gameInstallationManagerService,
                        _downloadsManager);
                    return new DownloadsManagerView() { DataContext = _downloadsManagerViewModel };
                },
                Closed = () => {
                    _downloadsManagerViewModel?.Dispose();
                }
            };

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _downloadsManager.GlobalDownloadProgressChanged += OnGlobalProgressChanged;
        }

        private void UnsubscribeFromEvents()
        {
            _downloadsManager.GlobalDownloadProgressChanged -= OnGlobalProgressChanged;
        }

        private void OnGlobalProgressChanged(object sender, GlobalDownloadProgressChangedEventArgs e)
        {
            UpdateSidebarProgress(e);
        }

        private void UpdateSidebarProgress(GlobalDownloadProgressChangedEventArgs e)
        {
            if (e.TotalItems == 0 || !e.TotalDownloadProgress.HasValue)
            {
                _sidebarLibraryManagerView.ProgressValue = 0;
            }
            else
            {
                _sidebarLibraryManagerView.ProgressValue = e.TotalDownloadProgress.Value;
                _sidebarLibraryManagerView.Title = _sidebarBaseItemTitle
                    + Environment.NewLine + Environment.NewLine
                    + $"{e.TotalBytesDownloaded.Value.ToReadableSize()}/{e.TotalBytesToDownload.Value.ToReadableSize()}"
                    + $" ({e.TotalDownloadProgress.Value:F2}%)";
            }

            if (_sidebarLibraryManagerView.ProgressValue == 0 || _sidebarLibraryManagerView.ProgressValue == 100)
            {
                _sidebarLibraryManagerView.Title = _sidebarBaseItemTitle;
            }
        }

#pragma warning disable CS0618 // Disable warning for obsolete elements
        private void MigrateOldGameInstallCache()
        {
            try
            {
                var oldCacheList = settings.Settings.LibraryCache.Values.ToList();
                if (!oldCacheList.HasItems())
                {
                    return;
                }

                var pluginGamesByGameId = PlayniteApi.Database.Games
                    .Where(g => g.PluginId == this.Id)
                    .ToDictionary(g => g.GameId, g => g);

                foreach (var oldCache in oldCacheList)
                {
                    if (oldCache.Program != null && pluginGamesByGameId.TryGetValue(oldCache.GameId, out var matchingGame))
                    {
                        _gameInstallationManagerService.ApplyProgramToGameCache(matchingGame, oldCache.Program);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error during install cache migration");
            }
            finally
            {
                settings.Settings.LibraryCache.Clear();
                settings.SaveSettings();
            }
        }
#pragma warning restore CS0618

        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            var games = new List<GameMetadata>();
            var isLoggedIn = _jastUsaAccountClient.GetIsUserLoggedIn();
            if (!isLoggedIn)
            {
                PlayniteApi.Notifications.Add(
                    new NotificationMessage(
                        "JastNotLoggedIn", "JAST USA Library: " +
                        ResourceProvider.GetString("LOC_JUL_DialogMessageNotAuthenticated"),
                        NotificationType.Error,
                        () => OpenSettingsView()));
                return games;
            }
            else
            {
                PlayniteApi.Notifications.Remove("JastNotLoggedIn");
            }

            var jastUsaGames = Task.Run(() => _jastUsaAccountClient.GetGamesAsync())
                .GetAwaiter().GetResult();
            var pluginGamesByGameId = PlayniteApi.Database.Games
                .Where(g => g.PluginId == this.Id)
                .ToDictionary(g => g.GameId, g => g);
            foreach (var gameData in jastUsaGames)
            {
                var game = new GameMetadata
                {
                    Name = GameNameSanitizer.Satinize(gameData.ProductName),
                    GameId = gameData.GameId.ToString(),
                    Platforms = new HashSet<MetadataProperty> { new MetadataSpecProperty("pc_windows") },
                    Source = new MetadataNameProperty("JAST USA")
                };

                if (pluginGamesByGameId.TryGetValue(game.GameId, out var matchingDatabaseGame))
                {
                    var installationData = _gameInstallationManagerService.GetCacheById(matchingDatabaseGame.Id);
                    if (installationData != null
                        && installationData.Program?.Path.IsNullOrEmpty() == false
                        && FileSystem.FileExists(installationData.Program?.Path))
                    {
                        game.InstallDirectory = Path.GetDirectoryName(installationData.Program.Path);
                        game.IsInstalled = true;
                    }
                }

                games.Add(game);
            }

            foreach (var gameData in jastUsaGames)
            {
                var matchingExistingCache = _jastUsaCacheService.GetCacheById(gameData.GameId);
                if (matchingExistingCache != null)
                {
                    if (matchingExistingCache.JastGameData != gameData)
                    {
                        matchingExistingCache.UpdateJastGameData(gameData);
                        _jastUsaCacheService.SaveCache(matchingExistingCache);
                    }
                }
                else
                {
                    var newCache = new GameCache(gameData.GameId);
                    newCache.UpdateJastGameData(gameData);
                    _jastUsaCacheService.SaveCache(newCache);
                }
            }

            var jastUsaCache = _jastUsaCacheService.GetAllCache();
            foreach (var cache in jastUsaCache)
            {
                if (cache.JastGameData is null || cache.Downloads != null || !cache.JastGameData.EnUsId.HasValue)
                {
                    continue;
                }

                var downloadsId = cache.JastGameData.EnUsId.Value;
                try
                {
                    var gameDownloads = _jastUsaAccountClient.GetGameTranslationsAsync(downloadsId).GetAwaiter().GetResult();
                    if (gameDownloads != null)
                    {
                        cache.UpdateDownloads(gameDownloads);
                        _jastUsaCacheService.SaveCache(cache);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, $"Error during GetGames while obtaining GameTranslations for {cache.JastGameData.ProductName} with id {downloadsId}");
                }
            }

            return games;
        }

        public override LibraryMetadataProvider GetMetadataDownloader()
        {
            return new JastUsaLibraryMetadataProvider(_jastUsaCacheService);
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
            MigrateOldGameInstallCache();
            if (settings.Settings.StartDownloadsOnStartup)
            {
                _ = Task.Run(() => _downloadsManager.StartDownloadsAsync(false, false));
            }
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var game = args.Games.Last();
            if (args.Games.Count == 1 && game.PluginId == Id)
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
                            var selectedProgram = ProgramsService.SelectExecutable();
                            if (selectedProgram != null)
                            {
                                _gameInstallationManagerService.ApplyProgramToGameCache(game, selectedProgram);
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
                var gameCache = _gameInstallationManagerService.GetCacheById(game.Id);
                if (gameCache != null && gameCache.Program != null)
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
                    PlayniteApi.Dialogs.ShowErrorMessage(
                        ResourceProvider.GetString("LOC_JUL_MessageInstallUnavailableFsMode"), "JAST USA Library");
                    return base.GetInstallActions(args);
                }

                var gameCache = _jastUsaCacheService.GetCacheById(Convert.ToInt32(game.GameId));
                if (gameCache?.JastGameData != null)
                {
                    return new List<InstallController>
                    {
                        new JastInstallController(
                            game,
                            gameCache,
                            PlayniteApi,
                            _logger,
                            _downloadsManager,
                            _gameInstallationManagerService)
                    };
                }
            }

            return base.GetInstallActions(args);
        }

        public override IEnumerable<UninstallController> GetUninstallActions(GetUninstallActionsArgs args)
        {
            var game = args.Game;
            if (game.PluginId == Id)
            {
                if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen)
                {
                    PlayniteApi.Dialogs.ShowErrorMessage(
                        ResourceProvider.GetString("LOC_JUL_MessageInstallUnavailableFsMode"), "JAST USA Library");
                    return base.GetUninstallActions(args);
                }

                var installCache = _gameInstallationManagerService.GetCacheById(game.Id);
                if (installCache?.Program != null && FileSystem.FileExists(installCache.Program.Path))
                {
                    return new List<UninstallController>
                    {
                        new JastUninstallController(game, PlayniteApi, _gameInstallationManagerService, installCache, this)
                    };
                }
            }

            return base.GetUninstallActions(args);
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            UnsubscribeFromEvents();
            _downloadsManager.StopDownloadsAndPersistDownloadData();
            _downloadsManager?.Dispose();
            _downloadsManagerViewModel?.Dispose();
        }

        public void SavePluginSettings()
        {
            settings.SaveSettings();
        }

        public override IEnumerable<SidebarItem> GetSidebarItems()
        {
            yield return _sidebarLibraryManagerView;
        }

    }
}