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

namespace NVIDIAGeForceNowEnabler
{
    public class NVIDIAGeForceNowEnabler : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly string geforceNowWorkingPath;
        private readonly string geforceNowExecutablePath;
        private readonly string gfnDatabasePath;
        private List<GeforceGame> supportedList;

        private NVIDIAGeForceNowEnablerSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("5f2dfd12-5f13-46fe-bcdd-64eb53ace26a");

        public NVIDIAGeForceNowEnabler(IPlayniteAPI api) : base(api)
        {
            settings = new NVIDIAGeForceNowEnablerSettingsViewModel(this);
            geforceNowWorkingPath = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), "NVIDIA Corporation", "GeForceNOW", "CEF");
            geforceNowExecutablePath = Path.Combine(geforceNowWorkingPath, "GeForceNOWStreamer.exe");
            gfnDatabasePath = Path.Combine(GetPluginUserDataPath(), "gfnpc.json");
            UpdateDatabase();
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        private void UpdateDatabase()
        {
            if (!FileSystem.FileExists(gfnDatabasePath))
            {
                logger.Debug($"Database in {gfnDatabasePath} not found on startup");
                supportedList = new List<GeforceGame>();
                return;
            }

            supportedList = Serialization.FromJsonFile<List<GeforceGame>>(gfnDatabasePath);
            logger.Debug($"Deserialized database in {gfnDatabasePath} with {supportedList.Count} entries on startup");
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (settings.Settings.ExecuteOnStartup == true)
            {
                MainMethod(false);
            }
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            if (settings.Settings.ExecuteOnLibraryUpdate)
            {
                MainMethod(false);
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
                        MainMethod(true);
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCNgfn_Enabler_MenuItemRemoveGameActionDescription"),
                    MenuSection = "@NVIDIA GeForce NOW Enabler",
                    Action = o => {
                        LibraryRemoveAllPlayActions();
                    }
                },
            };
        }

        private string SatinizeString(string str)
        {
            var satinizedString = str.Replace("Game of the Year Edition", "Game of the Year");
            return Regex.Replace(satinizedString, @"[^\p{L}\p{Nd}]", "").ToLower();
        }

        public void RefreshGameList(bool showDialogs)
        {
            PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                try
                {
                    var downloadedString = HttpDownloader.DownloadString(@"https://static.nvidiagrid.net/supported-public-game-list/gfnpc.json");
                    logger.Debug($"Downloaded nvidia database");
                    var supportedList = Serialization.FromJson<List<GeforceGame>>(downloadedString);
                    logger.Debug($"Deserialized database with {supportedList.Count} supported games");
                    if (supportedList.Count >= 0)
                    {
                        foreach (var supportedGame in supportedList)
                        {
                            supportedGame.Title = SatinizeOriginGameName(SatinizeString(supportedGame.Title));
                        }

                        FileSystem.WriteStringToFile(gfnDatabasePath, Serialization.ToJson(supportedList));
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

        public IEnumerable<Game> GetGamesSupportedLibraries()
        {
            var supportedLibraries = new List<Guid>()
            {
                // Steam
                Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab"),
                // Epic
                Guid.Parse("00000002-dbd1-46c6-b5d0-b1ba559d10e4"),
                // Origin
                Guid.Parse("85dd7072-2f20-4e76-a007-41035e390724"),
                // Uplay
                Guid.Parse("c2f038e5-8b92-4877-91f1-da9094155fc5"),
                // GOG
                Guid.Parse("aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e")
            };

            return PlayniteApi.Database.Games.Where(g => supportedLibraries.Contains(g.PluginId));
        }

        private string SatinizeOriginGameName(string gameName)
        {
            return gameName.Replace("gameoftheyearedition", "")
                .Replace("premiumedition", "")
                .Replace("gameoftheyear", "")
                .Replace("definitiveedition", "")
                .Replace("battlefieldvdefinitive", "battlefieldv")
                .Replace("battlefield1revolution", "battlefield1");
        }

        public void MainMethod(bool showDialogs)
        {
            var featureName = "NVIDIA GeForce NOW";
            var feature = PlayniteApi.Database.Features.Add(featureName);

            RefreshGameList(showDialogs);
            if (supportedList.Count() == 0)
            {
                // In case download failed.
                // Also sometimes there are issues with the api and it doesn't return any games in the response
                logger.Debug($"Supported games were 0 so execution was stopped");
                return;
            }


            // Entries in the database no longer have the Store value in their database
            // It remains to be seen if this is permanent or temporary
            //var supportedSteamGames = supportedGames.Where(g => g.Store == "Steam");
            //var supportedEpicGames = supportedGames.Where(g => g.Store == "Epic");
            //var supportedOriginGames = supportedGames.Where(g => g.Store == "Origin");
            //var supportedUplayGames = supportedGames.Where(g => g.Store == "Ubisoft Connect");
            //var supportedGogGames = supportedGames.Where(g => g.Store == "GOG");

            int enabledGamesCount = 0;
            int featureAddedCount = 0;
            int featureRemovedCount = 0;
            int setAsInstalledCount = 0;

            var progRes = PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                var gameDatabase = GetGamesSupportedLibraries();
                logger.Debug($"Starting detection of {gameDatabase.Count()} games from supported libraries");
                foreach (var game in gameDatabase)
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

        public void LibraryRemoveAllPlayActions()
        {
            int playActionRemovedCount = 0;
            var gameDatabase = GetGamesSupportedLibraries();
            foreach (var game in gameDatabase)
            {
                GameAction geforceNowAction = null;
                if (game.GameActions != null)
                {
                    geforceNowAction = game.GameActions
                        .Where(x => x.Arguments != null)
                        .Where(x => Regex.IsMatch(x.Arguments, @"--url-route=""#\?cmsId=\d+&launchSource=External&shortName=game_gfn_pc&parentGameId="""))
                        .FirstOrDefault();
                }

                if (geforceNowAction != null)
                {
                    game.GameActions.Remove(geforceNowAction);
                    PlayniteApi.Database.Games.Update(game);
                    playActionRemovedCount++;
                    logger.Info(string.Format("Play Action removed from \"{0}\"", game.Name));
                }
            }

            var results = string.Format(ResourceProvider.GetString("LOCNgfn_Enabler_GameActionsRemoveResultsMessage"), playActionRemovedCount);
            PlayniteApi.Dialogs.ShowMessage(results, "NVIDIA GeForce NOW Enabler");
        }

        private GeforceGame GetDatabaseMatchingEntryForGame(Game game)
        {
            var gameName = SatinizeOriginGameName(SatinizeString(game.Name));
            switch (game.PluginId.ToString())
            {
                case "cb91dfc9-b977-43bf-8e70-55f46e410fab":
                    //Steam
                    var steamUrl = string.Format("https://store.steampowered.com/app/{0}", game.GameId);
                    return supportedList.FirstOrDefault(g => g.SteamUrl == steamUrl);
                case "00000002-dbd1-46c6-b5d0-b1ba559d10e4":
                    //Epic
                    return supportedList.FirstOrDefault(g => g.SteamUrl.IsNullOrEmpty() && g.Title == gameName);
                case "85dd7072-2f20-4e76-a007-41035e390724":
                    //Origin
                    return supportedList.FirstOrDefault(g => g.SteamUrl.IsNullOrEmpty() && g.Title == gameName);
                case "c2f038e5-8b92-4877-91f1-da9094155fc5":
                    //Uplay
                    return supportedList.FirstOrDefault(g => g.SteamUrl.IsNullOrEmpty() && g.Title == gameName);
                case "aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e":
                    //GOG
                    return supportedList.FirstOrDefault(g => g.SteamUrl.IsNullOrEmpty() && g.Title == gameName);
                default:
                    return null;
            }
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

            if (FileSystem.FileExists(geforceNowExecutablePath))
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