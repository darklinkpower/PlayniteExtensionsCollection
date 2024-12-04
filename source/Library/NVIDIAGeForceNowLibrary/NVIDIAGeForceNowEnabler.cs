using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using Playnite.SDK.Data;
using System.Diagnostics;
using PluginsCommon;
using PlayniteUtilitiesCommon;
using NVIDIAGeForceNowEnabler.Services;
using NVIDIAGeForceNowEnabler.Models;
using NVIDIAGeForceNowEnabler.Views;
using NVIDIAGeForceNowEnabler.ViewModels;
using System.Windows;
using System.Reflection;

namespace NVIDIAGeForceNowEnabler
{
    public class NVIDIAGeForceNowEnabler : LibraryPlugin
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly string _geforceNowWorkingPath;
        private readonly string _gfnDatabasePath;
        public readonly string _geforceNowExecutablePath;

        private readonly NVIDIAGeForceNowEnablerSettingsViewModel _settings;
        private readonly HashSet<AppStore> _appStoresToMatchByName;

        private Dictionary<Guid, AppStore> _pluginIdToAppStoreMapper;
        private Dictionary<Tuple<AppStore, string>, GeforceNowItemVariant> _detectionDictionary = new Dictionary<Tuple<AppStore, string>, GeforceNowItemVariant>();

        private bool _databaseUpdatedOnGetGames = false;

        public override LibraryClient Client { get; } = new NVIDIAGeForceNowClient();
        public override Guid Id { get; } = Guid.Parse("5f2dfd12-5f13-46fe-bcdd-64eb53ace26a");
        public override string Name => "NVIDIA GeForce NOW";
        public override string LibraryIcon { get; }

        public NVIDIAGeForceNowEnabler(IPlayniteAPI api) : base(api)
        {
            _settings = new NVIDIAGeForceNowEnablerSettingsViewModel(this);
            _geforceNowWorkingPath = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), "NVIDIA Corporation", "GeForceNOW", "CEF");
            _geforceNowExecutablePath = Path.Combine(_geforceNowWorkingPath, "GeForceNOWStreamer.exe");
            _gfnDatabasePath = Path.Combine(GetPluginUserDataPath(), "gfnDatabase.json");

            // For some libraries, game names need to be used because the GameId provided by plugins
            // don't match with the StoreId used in the GeForce Now database

            // For Origin, they are a little different. Probably due to some version and/or regional thing. Examples:
            // Origin: Battlefield 1, GameId: Origin.OFR.50.0004657, StoreId: Origin.OFR.50.000055
            // Origin: Dragon Age Inquisition, GameId: Origin.OFR.50.0000483, StoreId: Origin.OFR.50.0001131

            // For Epic they don't share any similarity and remains to be investigated. Examples:
            // Epic: Pillars of Eternity - Definitive Edition, GameId: bcc75c246fe04e45b0c1f1c3fd52503a, StoreId: bc31288122a7443b818f4e77eed5ce25
            _appStoresToMatchByName = new HashSet<AppStore>
            {
                AppStore.Epic,
                AppStore.EA_APP,
                AppStore.Xbox
            };

            _pluginIdToAppStoreMapper = new Dictionary<Guid, AppStore>
            {
                [Guid.Parse("e3c26a3d-d695-4cb7-a769-5ff7612c7edd")] = AppStore.Battlenet,
                [Guid.Parse("0e2e793e-e0dd-4447-835c-c44a1fd506ec")] = AppStore.Bethesda,
                //[Guid.Parse("99999999-9999-9999-9999-999999999999")] = AppStore.Digital_Extremes,
                [Guid.Parse("00000002-dbd1-46c6-b5d0-b1ba559d10e4")] = AppStore.Epic,
                //[Guid.Parse("99999999-9999-9999-9999-999999999999")] = AppStore.Gazillion,
                [Guid.Parse("aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e")] = AppStore.Gog,
                //[Guid.Parse("99999999-9999-9999-9999-999999999999")] = AppStore.None,
                //[Guid.Parse("99999999-9999-9999-9999-999999999999")] = AppStore.Nvidia,
                //[Guid.Parse("99999999-9999-9999-9999-999999999999")] = AppStore.Nv_Bundle,
                [Guid.Parse("85dd7072-2f20-4e76-a007-41035e390724")] = AppStore.EA_APP,
                [Guid.Parse("88409022-088a-4de8-805a-fdbac291f00a")] = AppStore.Rockstar,
                [Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab")] = AppStore.Steam,
                //[Guid.Parse("99999999-9999-9999-9999-999999999999")] = AppStore.Stove,
                //[Guid.Parse("99999999-9999-9999-9999-999999999999")] = AppStore.Unknown,
                [Guid.Parse("c2f038e5-8b92-4877-91f1-da9094155fc5")] = AppStore.Uplay,
                [Guid.Parse("7e4fbb5e-2ae3-48d4-8ba0-6b30e7a4e287")] = AppStore.Xbox
                //[Guid.Parse("99999999-9999-9999-9999-999999999999")] = AppStore.Wargaming
            };

            LibraryIcon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"icon.png");

            var cachedDatabase = LoadDatabaseFromFile(_gfnDatabasePath);
            SetDetectionDictionary(cachedDatabase);

            Properties = new LibraryPluginProperties
            {
                HasSettings = true
            };
        }

        private static List<GeforceNowItem> LoadDatabaseFromFile(string filePath)
        {
            if (!FileSystem.FileExists(filePath))
            {
                _logger.Debug($"File not found at path '{filePath}'. Returning an empty list.");
                return new List<GeforceNowItem>();
            }

            try
            {
                _logger.Debug($"Loading data from file at '{filePath}'.");
                return Serialization.FromJsonFile<List<GeforceNowItem>>(filePath);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to deserialize file at '{filePath}'. File will be deleted.");
                FileSystem.DeleteFileSafe(filePath);
            }

            return new List<GeforceNowItem>();
        }


        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (_settings.Settings.ExecuteOnStartup)
            {
                UpdateDatabaseAndGamesStatus(false);
            }
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            if (_settings.Settings.ExecuteOnLibraryUpdate)
            {
                var updateDatabase = !_databaseUpdatedOnGetGames;
                UpdateDatabaseAndGamesStatus(false, !updateDatabase);
            }

            _databaseUpdatedOnGetGames = false;
        }

        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            // To prevent downloading the database again during OnLibraryUpdated event
            _databaseUpdatedOnGetGames = false;
            var games = new List<GameMetadata>();
            if(!_settings.Settings.ImportDatabaseAsLibrary)
            {
                
                return games;
            }

            _databaseUpdatedOnGetGames = DownloadAndRefreshGameList(false);
            if (!_detectionDictionary.HasItems())
            {
                return games;
            }

            foreach (var game in PlayniteApi.Database.Games)
            {
                var geforceEntry = GetDatabaseMatchingEntryForGame(game);
                if (geforceEntry == null)
                {
                    continue;
                }

                var newGame = new GameMetadata
                {
                    Name = geforceEntry.Title.RemoveTrademarks(),
                    GameId = geforceEntry.Id.ToString(),
                    Platforms = new HashSet<MetadataProperty> { new MetadataSpecProperty("pc_windows") },
                    Source = new MetadataNameProperty("NVIDIA GeForce NOW"),
                    IsInstalled = true
                };

                games.Add(newGame);
            }

            return games;
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return _settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new NVIDIAGeForceNowEnablerSettingsView();
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCNgfn_Enabler_MenuItemUpdateStatusDescription"),
                    MenuSection = "@NVIDIA GeForce NOW Enabler",
                    Action = _ => {
                        UpdateDatabaseAndGamesStatus(true);
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCNgfn_Enabler_MenuItemOpenDatabaseBrowserDescription"),
                    MenuSection = "@NVIDIA GeForce NOW Enabler",
                    Action = _ => {
                        OpenEditorWindow();
                    }
                }
            };
        }

        private void OpenEditorWindow()
        {
            DownloadAndRefreshGameList(false);
            var window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = true
            });

            window.Height = 700;
            window.Width = 900;
            window.Title = ResourceProvider.GetString("LOCNgfn_Enabler_DatabaseBrowserWindowTitle");

            window.Content = new GfnDatabaseBrowserView();
            window.DataContext = new GfnDatabaseBrowserViewModel(PlayniteApi, _detectionDictionary.Select(x => x.Value).ToList());
            window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            window.ShowDialog();
        }

        public bool DownloadAndRefreshGameList(bool showDialogs)
        {
            var databaseUpdated = false;
            PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                try
                {
                    var downloadedDatabase = GeforceNowService.GetGeforceNowDatabase();
                    if (downloadedDatabase.Count > 0)
                    {
                        FileSystem.WriteStringToFile(_gfnDatabasePath, Serialization.ToJson(downloadedDatabase), true);
                        SetDetectionDictionary(downloadedDatabase);
                        databaseUpdated = true;
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, $"Error downloading database.");
                    if (showDialogs)
                    {
                        PlayniteApi.Dialogs.ShowErrorMessage(e.Message, "NVIDIA GeForce NOW Enabler");
                    }
                }
            }, new GlobalProgressOptions(ResourceProvider.GetString("LOCNgfn_Enabler_DownloadingDatabaseProgressMessage")));

            return databaseUpdated;
        }

        private void SetDetectionDictionary(List<GeforceNowItem> geforceList)
        {
            _detectionDictionary = new Dictionary<Tuple<AppStore, string>, GeforceNowItemVariant>();
            foreach (var geforceNowItem in geforceList)
            {
                if (geforceNowItem.Type != AppType.Game)
                {
                    continue;
                }

                foreach (var itemVariant in geforceNowItem.Variants)
                {
                    if (itemVariant.OsType != OsType.Windows)
                    {
                        continue;
                    }

                    if (_appStoresToMatchByName.Contains(itemVariant.AppStore))
                    {
                        var key = Tuple.Create(itemVariant.AppStore, SatinizeGameName(itemVariant.Title));
                        _detectionDictionary[key] = itemVariant;
                    }
                    else
                    {
                        var key = Tuple.Create(itemVariant.AppStore, itemVariant.StoreId);
                        _detectionDictionary[key] = itemVariant;
                    }
                }
            }
        }

        private string SatinizeGameName(string gameName)
        {
            return gameName.Satinize()
                .Replace("gameoftheyearedition", "")
                .Replace("premiumedition", "")
                .Replace("gameoftheyearedition", "")
                .Replace("gameoftheyear", "")
                .Replace("definitiveedition", "")
                .Replace("battlefieldvdefinitive", "battlefieldv")
                .Replace("battlefield1revolution", "battlefield1");
        }

        public void UpdateDatabaseAndGamesStatus(bool showDialogs, bool updateDatabase = true)
        {
            var featureName = "NVIDIA GeForce NOW";
            var feature = PlayniteApi.Database.Features.Add(featureName);

            if (updateDatabase)
            {
                DownloadAndRefreshGameList(showDialogs);
            }
            if (!_detectionDictionary.HasItems())
            {
                // In case download failed.
                // Also sometimes there are issues with the api and it doesn't return any games in the response
                _logger.Debug($"Supported games were 0 so execution was stopped");
                return;
            }

            int enabledGamesCount = 0;
            int featureAddedCount = 0;
            int featureRemovedCount = 0;
            int setAsInstalledCount = 0;

            var progRes = PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                using (PlayniteApi.Database.BufferedUpdate())
                foreach (var game in PlayniteApi.Database.Games)
                {
                    var supportedGame = GetDatabaseMatchingEntryForGame(game);
                    if (supportedGame == null)
                    {
                        if (PlayniteUtilities.RemoveFeatureFromGame(PlayniteApi, game, feature))
                        {
                            featureRemovedCount++;
                            _logger.Info(string.Format("Feature removed from \"{0}\"", game.Name));
                        }
                    }
                    else
                    {
                        enabledGamesCount++;
                        if (PlayniteUtilities.AddFeatureToGame(PlayniteApi, game, feature))
                        {
                            featureAddedCount++;
                            _logger.Info(string.Format("Feature added to \"{0}\"", game.Name));
                        }

                        if (_settings.Settings.ShowPlayActionsOnLaunch && !game.IsInstalled)
                        {
                            game.IsInstalled = true;
                            setAsInstalledCount++;
                            PlayniteApi.Database.Games.Update(game);
                            _logger.Info(string.Format("Set \"{0}\" as installed", game.Name));
                        }
                    }
                }
            }, new GlobalProgressOptions(ResourceProvider.GetString("LOCNgfn_Enabler_UpdatingProgressMessage")));

            _logger.Info($"Found {enabledGamesCount} enabled games. Added feature to {featureAddedCount} games and removed it from {featureRemovedCount} games. Set {setAsInstalledCount} as installed.");
            if (showDialogs)
            {
                var results = string.Format(ResourceProvider.GetString("LOCNgfn_Enabler_UpdateResults1Message"),
                    enabledGamesCount, featureName, featureAddedCount, featureName, featureRemovedCount);
                if (_settings.Settings.ShowPlayActionsOnLaunch)
                {
                    results += string.Format(ResourceProvider.GetString("LOCNgfn_Enabler_UpdateResults3Message"), setAsInstalledCount);
                }
                PlayniteApi.Dialogs.ShowMessage(results, "NVIDIA GeForce NOW Enabler");
            }
            else if (setAsInstalledCount > 0)
            {
                PlayniteApi.Notifications.Add(new NotificationMessage(Guid.NewGuid().ToString(),
                    string.Format(ResourceProvider.GetString("LOCNgfn_Enabler_NotificationMessageMarkedInstalledResults"), setAsInstalledCount),
                    NotificationType.Info));
            }
        }

        private GeforceNowItemVariant GetDatabaseMatchingEntryForGame(Game game)
        {
            if (_pluginIdToAppStoreMapper.TryGetValue(game.PluginId, out var appStore))
            {
                if (_appStoresToMatchByName.Contains(appStore))
                {
                    var key = Tuple.Create(appStore, SatinizeGameName(game.Name));
                    if (_detectionDictionary.TryGetValue(key, out var itemVariant))
                    {
                        return itemVariant;
                    }
                }
                else
                {
                    var key = Tuple.Create(appStore, game.GameId);
                    if (_detectionDictionary.TryGetValue(key, out var itemVariant))
                    {
                        return itemVariant;
                    }
                }
            }

            return null;
        }

        public override IEnumerable<PlayController> GetPlayActions(GetPlayActionsArgs args)
        {
            var game = args.Game;
            if (game.PluginId == Id)
            {
                return new List<PlayController>()
                {
                    new NVIDIAGeForceNowEnablerPlayController(game, game.GameId, _geforceNowExecutablePath, _geforceNowWorkingPath)
                };
            }

            if (!_settings.Settings.ShowPlayActionsOnLaunch)
            {
                return null;
            }

            // Non library games are not supported so they can be skipped
            if (game.PluginId == Guid.Empty)
            {
                return null;
            }

            // Library plugins set the game installation directory when they are
            // detected as installed. This is used to detect this and not show the Play
            // Action if it is detected as installed by the game library plugin.
            if (_settings.Settings.OnlyShowActionsForNotLibInstalledGames && !game.InstallDirectory.IsNullOrEmpty())
            {
                _logger.Debug("Game install dir was not empty and was skipped");
                return null;
            }

            if (!FileSystem.FileExists(_geforceNowExecutablePath))
            {
                _logger.Debug("Geforce Now Executable was not detected");
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    "gfnExeNotFound",
                    string.Format(ResourceProvider.GetString("LOCNgfn_Enabler_NotificationMessage"), _geforceNowExecutablePath),
                    NotificationType.Error,
                    () => ProcessStarter.StartUrl(@"https://github.com/darklinkpower/PlayniteExtensionsCollection/wiki/NVIDIA-GeForce-NOW-Enabler#nvidia-geforce-now-executable-not-found-error-notification")
                ));
                return null;
            }

            if (!_detectionDictionary.HasItems())
            {
                _logger.Debug("Supported list was not set");
                return null;
            }

            var supportedGame = GetDatabaseMatchingEntryForGame(game);
            if (supportedGame != null)
            {
                _logger.Debug($"Database entry with id {supportedGame.Id} found on startup for game {game.Name}");
                return new List<PlayController>()
                {
                    new NVIDIAGeForceNowEnablerPlayController(game, supportedGame.Id.ToString(), _geforceNowExecutablePath, _geforceNowWorkingPath)
                };
            }
            else
            {
                _logger.Debug($"Database entry with for {game.Name} with pluginId {game.PluginId} not found");
            }

            return null;
        }
    }
}