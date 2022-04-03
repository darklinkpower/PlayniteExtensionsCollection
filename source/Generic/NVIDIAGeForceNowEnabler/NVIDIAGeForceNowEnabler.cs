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
using PluginsCommon.Web;
using PlayniteUtilitiesCommon;
using NVIDIAGeForceNowEnabler.Services;
using NVIDIAGeForceNowEnabler.Models;

namespace NVIDIAGeForceNowEnabler
{
    public class NVIDIAGeForceNowEnabler : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly string geforceNowWorkingPath;
        private readonly string geforceNowExecutablePath;
        private readonly string gfnDatabasePath;
        private List<GeforceNowItem> supportedList;
        private Dictionary<Guid, AppStore> pluginIdToAppStore;

        private NVIDIAGeForceNowEnablerSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("5f2dfd12-5f13-46fe-bcdd-64eb53ace26a");

        public NVIDIAGeForceNowEnabler(IPlayniteAPI api) : base(api)
        {
            settings = new NVIDIAGeForceNowEnablerSettingsViewModel(this);
            geforceNowWorkingPath = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), "NVIDIA Corporation", "GeForceNOW", "CEF");
            geforceNowExecutablePath = Path.Combine(geforceNowWorkingPath, "GeForceNOWStreamer.exe");
            gfnDatabasePath = Path.Combine(GetPluginUserDataPath(), "gfnDatabase.json");
            LoadDatabaseFromFile();
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            SetEnumsDictionary();
        }

        private void SetEnumsDictionary()
        {
            pluginIdToAppStore = new Dictionary<Guid, AppStore>
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
                [Guid.Parse("85dd7072-2f20-4e76-a007-41035e390724")] = AppStore.Origin,
                [Guid.Parse("88409022-088a-4de8-805a-fdbac291f00a")] = AppStore.Rockstar,
                [Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab")] = AppStore.Steam,
                //[Guid.Parse("99999999-9999-9999-9999-999999999999")] = AppStore.Stove,
                //[Guid.Parse("99999999-9999-9999-9999-999999999999")] = AppStore.Unknown,
                [Guid.Parse("c2f038e5-8b92-4877-91f1-da9094155fc5")] = AppStore.Uplay,
                //[Guid.Parse("99999999-9999-9999-9999-999999999999")] = AppStore.Wargaming
            };
        }

        private void LoadDatabaseFromFile()
        {
            if (!FileSystem.FileExists(gfnDatabasePath))
            {
                logger.Debug($"Database in {gfnDatabasePath} not found on startup");
                supportedList = new List<GeforceNowItem>();
                return;
            }

            supportedList = Serialization.FromJsonFile<List<GeforceNowItem>>(gfnDatabasePath);
            SatinizeSupportedListTitles();
            logger.Debug($"Deserialized database in {gfnDatabasePath} with {supportedList.Count} entries on startup");
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (settings.Settings.ExecuteOnStartup)
            {
                UpdateDatabaseAndGamesStatus(false);
            }
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            if (settings.Settings.ExecuteOnLibraryUpdate)
            {
                UpdateDatabaseAndGamesStatus(false);
            }
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
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
                    Action = o => {
                        UpdateDatabaseAndGamesStatus(true);
                    }
                }
            };
        }

        public void DownloadAndRefreshGameList(bool showDialogs)
        {
            PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                try
                {
                    var downloadedDatabase = GeforceNowService.GetGeforceNowDatabase();
                    if (downloadedDatabase.Count > 0)
                    {
                        supportedList = downloadedDatabase;
                        FileSystem.WriteStringToFile(gfnDatabasePath, Serialization.ToJson(supportedList));
                        SatinizeSupportedListTitles();
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Error downloading database.");
                    if (showDialogs)
                    {
                        PlayniteApi.Dialogs.ShowErrorMessage(e.Message, "NVIDIA GeForce NOW Enabler");
                    }
                }
            }, new GlobalProgressOptions(ResourceProvider.GetString("LOCNgfn_Enabler_DownloadingDatabaseProgressMessage")));
        }

        private void SatinizeSupportedListTitles()
        {
            foreach (var geforceNowItem in supportedList)
            {
                foreach (var variant in geforceNowItem.Variants)
                {
                    variant.Title = SatinizeGameName(variant.Title);
                }
            }
        }

        private string SatinizeGameName(string gameName)
        {
            return gameName.GetMatchModifiedName()
                .Replace("gameoftheyearedition", "")
                .Replace("premiumedition", "")
                .Replace("gameoftheyearedition", "")
                .Replace("gameoftheyear", "")
                .Replace("definitiveedition", "")
                .Replace("battlefieldvdefinitive", "battlefieldv")
                .Replace("battlefield1revolution", "battlefield1");
        }

        public void UpdateDatabaseAndGamesStatus(bool showDialogs)
        {
            var featureName = "NVIDIA GeForce NOW";
            var feature = PlayniteApi.Database.Features.Add(featureName);

            DownloadAndRefreshGameList(showDialogs);
            if (supportedList.Count == 0)
            {
                // In case download failed.
                // Also sometimes there are issues with the api and it doesn't return any games in the response
                logger.Debug($"Supported games were 0 so execution was stopped");
                return;
            }

            int enabledGamesCount = 0;
            int featureAddedCount = 0;
            int featureRemovedCount = 0;
            int setAsInstalledCount = 0;

            var progRes = PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                foreach (var game in PlayniteApi.Database.Games)
                {
                    var supportedGame = GetDatabaseMatchingEntryForGame(game);
                    if (supportedGame == null)
                    {
                        if (PlayniteUtilities.RemoveFeatureFromGame(PlayniteApi, game, feature))
                        {
                            featureRemovedCount++;
                            logger.Info(string.Format("Feature removed from \"{0}\"", game.Name));
                        }
                    }
                    else
                    {
                        enabledGamesCount++;
                        if (PlayniteUtilities.AddFeatureToGame(PlayniteApi, game, feature))
                        {
                            featureAddedCount++;
                            logger.Info(string.Format("Feature added to \"{0}\"", game.Name));
                        }

                        if ((settings.Settings.SetEnabledGamesAsInstalled || settings.Settings.ShowPlayActionsOnLaunch)
                            && game.IsInstalled == false)
                        {
                            game.IsInstalled = true;
                            setAsInstalledCount++;
                            PlayniteApi.Database.Games.Update(game);
                            logger.Info(string.Format("Set \"{0}\" as installed", game.Name));
                        }
                    }
                }
            }, new GlobalProgressOptions(ResourceProvider.GetString("LOCNgfn_Enabler_UpdatingProgressMessage")));

            logger.Info($"Found {enabledGamesCount} enabled games. Added feature to {featureAddedCount} games and removed it from {featureRemovedCount} games. Set {setAsInstalledCount} as installed.");
            if (showDialogs)
            {
                var results = string.Format(ResourceProvider.GetString("LOCNgfn_Enabler_UpdateResults1Message"),
                    enabledGamesCount, featureName, featureAddedCount, featureName, featureRemovedCount);
                if (settings.Settings.SetEnabledGamesAsInstalled == true)
                {
                    results += string.Format(ResourceProvider.GetString("LOCNgfn_Enabler_UpdateResults3Message"), setAsInstalledCount);
                }
                PlayniteApi.Dialogs.ShowMessage(results, "NVIDIA GeForce NOW Enabler");
            }
            else if (setAsInstalledCount > 0)
            {
                PlayniteApi.Notifications.Add(new NotificationMessage(new Guid().ToString(),
                    string.Format(ResourceProvider.GetString("LOCNgfn_Enabler_NotificationMessageMarkedInstalledResults"), setAsInstalledCount),
                    NotificationType.Info));
            }
        }

        private GeforceNowItemVariant GetDatabaseMatchingEntryForGame(Game game)
        {
            if (pluginIdToAppStore.TryGetValue(game.PluginId, out var appStore))
            {
                // For some libraries, names need to be used because the GameId provided by plugins
                // don't match with the StoreId used in the GeForce Now database

                // For Origin, they are a little different. Probably due to some version and/or regional thing. Examples:
                // Origin: Battlefield 1, GameId: Origin.OFR.50.0004657, StoreId: Origin.OFR.50.000055
                // Origin: Dragon Age Inquisition, GameId: Origin.OFR.50.0000483, StoreId: Origin.OFR.50.0001131

                // For Epic they don't share any similarity and remains to be investigated. Examples:
                // Epic: Pillars of Eternity - Definitive Edition, GameId: bcc75c246fe04e45b0c1f1c3fd52503a, StoreId: bc31288122a7443b818f4e77eed5ce25
                var useNameMatching = false;
                var matchingName = string.Empty;
                if (appStore == AppStore.Epic || appStore == AppStore.Origin)
                {
                    matchingName = SatinizeGameName(game.Name);
                    useNameMatching = true;
                }

                foreach (var geforceNowItem in supportedList)
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
                        
                        if (useNameMatching)
                        {
                            if (itemVariant.AppStore == appStore && itemVariant.Title == matchingName)
                            {
                                return itemVariant;
                            }
                        }
                        else if (itemVariant.AppStore == appStore && itemVariant.StoreId == game.GameId)
                        {
                            return itemVariant;
                        }
                    }
                }
            }

            return null;
        }

        public override IEnumerable<PlayController> GetPlayActions(GetPlayActionsArgs args)
        {
            if (!settings.Settings.ShowPlayActionsOnLaunch)
            {
                return null;
            }
            
            var game = args.Game;
            // Library plugins set the game installation directory when they are
            // detected as installed. This is used to detect this and not show the Play
            // Action if it is detected as installed by the game library plugin.
            if (!game.InstallDirectory.IsNullOrEmpty())
            {
                logger.Debug("Game install dir was not empty and was skipped");
                return null;
            }

            if (!FileSystem.FileExists(geforceNowExecutablePath))
            {
                logger.Debug("Geforce Now Executable was not detected");
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    "gfnExeNotFound",
                    string.Format(ResourceProvider.GetString("LOCNgfn_Enabler_NotificationMessage"), geforceNowExecutablePath),
                    NotificationType.Error,
                    () => ProcessStarter.StartUrl(@"https://github.com/darklinkpower/PlayniteExtensionsCollection/wiki/NVIDIA-GeForce-NOW-Enabler#nvidia-geforce-now-executable-not-found-error-notification")
                ));
                return null;
            }

            if (supportedList.Count == 0)
            {
                logger.Debug("Supported list was not set");
                return null;
            }

            var supportedGame = GetDatabaseMatchingEntryForGame(game);
            if (supportedGame != null)
            {
                logger.Debug($"Database entry with id {supportedGame.Id} found on startup for game {game.Name}");
                return new List<PlayController>()
                {
                    new NVIDIAGeForceNowEnablerPlayController(game, supportedGame.Id.ToString(), geforceNowExecutablePath, geforceNowWorkingPath)
                };
            }

            return null;
        }
    }
}