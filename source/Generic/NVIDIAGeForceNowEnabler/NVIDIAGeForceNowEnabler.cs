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
using Newtonsoft.Json;

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
            if (!File.Exists(gfnDatabasePath))
            {
                supportedList = new List<GeforceGame>();
                return;
            }

            supportedList = JsonConvert.DeserializeObject<List<GeforceGame>>(File.ReadAllText(gfnDatabasePath));
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
            if (settings.Settings.ExecuteOnLibraryUpdate == true)
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

        public bool RemoveFeature(Game game, GameFeature feature)
        {
            if (game.FeatureIds != null)
            {
                if (game.FeatureIds.Contains(feature.Id))
                {
                    game.FeatureIds.Remove(feature.Id);
                    PlayniteApi.Database.Games.Update(game);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool AddFeature(Game game, GameFeature feature)
        {
            if (game.FeatureIds == null)
            {
                game.FeatureIds = new List<Guid> { feature.Id };
                PlayniteApi.Database.Games.Update(game);
                return true;
            }
            else if (game.FeatureIds.Contains(feature.Id) == false)
            {
                game.FeatureIds.AddMissing(feature.Id);
                PlayniteApi.Database.Games.Update(game);
                return true;
            }
            else
            {
                return false;
            }
        }

        private string SatinizeString(string str)
        {
            var satinizedString = str.Replace("Game of the Year Edition", "Game of the Year");
            satinizedString = Regex.Replace(satinizedString, @"[^\p{L}\p{Nd}]", "").ToLower();
            return satinizedString;
        }

        public List<GeforceGame> DownloadGameList(bool showDialogs)
        {
            var supportedGames = new List<GeforceGame>();

            var progRes = PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                using (var webClient = new WebClient())
                {
                    try
                    {
                        webClient.Encoding = Encoding.UTF8;
                        string downloadedString = webClient.DownloadString(@"https://static.nvidiagrid.net/supported-public-game-list/gfnpc.json");
                        supportedGames = JsonConvert.DeserializeObject<List<GeforceGame>>(downloadedString);
                        if (supportedGames.Count >= 0)
                        {
                            foreach (var supportedGame in supportedGames)
                            {
                                supportedGame.Title = SatinizeString(supportedGame.Title);
                            }
                            File.WriteAllText(gfnDatabasePath, JsonConvert.SerializeObject(supportedGames));
                            supportedList = supportedGames;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, e.Message);
                        if (showDialogs == true)
                        {
                            PlayniteApi.Dialogs.ShowErrorMessage(e.Message, "NVIDIA GeForce NOW Enabler");
                        }
                    }
                }
            }, new GlobalProgressOptions(ResourceProvider.GetString("LOCNgfn_Enabler_DownloadingDatabaseProgressMessage")));

            return supportedGames;
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

            var gameDatabase = PlayniteApi.Database.Games.Where(g => supportedLibraries.Contains(g.PluginId));
            return gameDatabase;
        }

        public void MainMethod(bool showDialogs)
        {
            string featureName = "NVIDIA GeForce NOW";
            GameFeature feature = PlayniteApi.Database.Features.Add(featureName);

            var supportedGames = DownloadGameList(showDialogs);
            if (supportedGames.Count() == 0)
            {
                // In case download failed.
                // Also sometimes there are issues with the api and it doesn't return any games in the response
                return;
            }
            var supportedSteamGames = supportedGames.Where(g => g.Store == "Steam");
            var supportedEpicGames = supportedGames.Where(g => g.Store == "Epic");
            var supportedOriginGames = supportedGames.Where(g => g.Store == "Origin");
            var supportedUplayGames = supportedGames.Where(g => g.Store == "Ubisoft Connect");
            var supportedGogGames = supportedGames.Where(g => g.Store == "GOG");

            int enabledGamesCount = 0;
            int featureAddedCount = 0;
            int featureRemovedCount = 0;
            int setAsInstalledCount = 0;
            int setAsUninstalledCount = 0;

            var progRes = PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {

                var gameDatabase = GetGamesSupportedLibraries();
                foreach (var game in gameDatabase)
                {
                    var gameName = SatinizeString(game.Name);
                    GeforceGame supportedGame = null;
                    switch (game.PluginId.ToString())
                    {
                        case "cb91dfc9-b977-43bf-8e70-55f46e410fab":
                            //Steam
                            var steamUrl = String.Format("https://store.steampowered.com/app/{0}", game.GameId);
                            supportedGame = supportedSteamGames.FirstOrDefault(g => g.SteamUrl == steamUrl);
                            break;
                        case "00000002-dbd1-46c6-b5d0-b1ba559d10e4":
                            //Epic
                            supportedGame = supportedEpicGames.FirstOrDefault(g => g.Title == gameName);
                            break;
                        case "85dd7072-2f20-4e76-a007-41035e390724":
                            //Origin
                            supportedGame = supportedOriginGames.FirstOrDefault(g => g.Title == gameName);
                            break;
                        case "c2f038e5-8b92-4877-91f1-da9094155fc5":
                            //Uplay
                            supportedGame = supportedUplayGames.FirstOrDefault(g => g.Title == gameName);
                            break;
                        case "aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e":
                            //GOG
                            supportedGame = supportedGogGames.FirstOrDefault(g => g.Title == gameName);
                            break;
                        default:
                            break;
                    }

                    if (supportedGame == null)
                    {
                        bool featureRemoved = RemoveFeature(game, feature);
                        if (featureRemoved == true)
                        {
                            featureRemovedCount++;
                            logger.Info(string.Format("Feature removed from \"{0}\"", game.Name));

                            if (settings.Settings.SetEnabledGamesAsInstalled == true && game.IsInstalled == true)
                            {
                                game.IsInstalled = true;
                                setAsUninstalledCount++;
                                PlayniteApi.Database.Games.Update(game);
                                logger.Info(string.Format("Set \"{0}\" as uninstalled", game.Name));
                            }
                        }
                    }
                    else
                    {
                        enabledGamesCount++;
                        bool featureAdded = AddFeature(game, feature);
                        if (featureAdded == true)
                        {
                            featureAddedCount++;
                            logger.Info(string.Format("Feature added to \"{0}\"", game.Name));
                        }
                        if (settings.Settings.SetEnabledGamesAsInstalled == true && game.IsInstalled == false)
                        {
                            game.IsInstalled = true;
                            setAsInstalledCount++;
                            PlayniteApi.Database.Games.Update(game);
                            logger.Info(string.Format("Set \"{0}\" as installed", game.Name));
                        }
                    }
                }
            }, new GlobalProgressOptions(ResourceProvider.GetString("LOCNgfn_Enabler_UpdatingProgressMessage")));

            if (showDialogs == true)
            {
                string results = string.Format(ResourceProvider.GetString("LOCNgfn_Enabler_UpdateResults1Message"),
                    enabledGamesCount, featureName, featureAddedCount, featureName, featureRemovedCount);
                if (settings.Settings.SetEnabledGamesAsInstalled == true)
                {
                    results += string.Format(ResourceProvider.GetString("LOCNgfn_Enabler_UpdateResults3Message"), setAsInstalledCount, setAsUninstalledCount);
                }
                PlayniteApi.Dialogs.ShowMessage(results, "NVIDIA GeForce NOW Enabler");
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
            string results = string.Format(ResourceProvider.GetString("LOCNgfn_Enabler_GameActionsRemoveResultsMessage"), playActionRemovedCount);
            PlayniteApi.Dialogs.ShowMessage(results, "NVIDIA GeForce NOW Enabler");
        }

        public override IEnumerable<PlayController> GetPlayActions(GetPlayActionsArgs args)
        {
            if (!settings.Settings.ShowPlayActionsOnLaunch)
            {
                return null;
            }
            
            if (supportedList.Count == 0)
            {
                logger.Debug("Supported list was no set");
                return null;
            }

            if (!File.Exists(geforceNowExecutablePath))
            {
                logger.Debug("Geforce Now Executable was not detected");
                return null;
            }

            var game = args.Game;
            if (!string.IsNullOrEmpty(game.InstallDirectory))
            {
                logger.Debug("Game install dir was not empty and was skipped");
                return null;
            }

            var storeName = string.Empty;
            switch (game.PluginId.ToString())
            {

                //Steam
                case "cb91dfc9-b977-43bf-8e70-55f46e410fab":
                    storeName = "Steam";
                    break;
                //Epic
                case "00000002-dbd1-46c6-b5d0-b1ba559d10e4":
                    storeName = "Epic";
                    break;
                //Origin
                case "85dd7072-2f20-4e76-a007-41035e390724":
                    storeName = "Origin";
                    break;
                //Uplay
                case "c2f038e5-8b92-4877-91f1-da9094155fc5":
                    storeName = "Ubisoft Connect";
                    break;
                //GOG
                case "aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e":
                    storeName = "GOG";
                    break;
                default:
                    break;
            }

            if (storeName == string.Empty)
            {
                return null;
            }
            
            GeforceGame supportedGame;
            if (storeName == "Steam")
            {
                var steamUrl = string.Format("https://store.steampowered.com/app/{0}", game.GameId);
                supportedGame = supportedList.FirstOrDefault(g => g.Store == storeName && g.SteamUrl == steamUrl);
            }
            else
            {
                var matchingName = SatinizeString(game.Name);
                supportedGame = supportedList.FirstOrDefault(g => g.Store == storeName && g.Title == matchingName);
            }

            if (supportedGame != null)
            {
                return new List<PlayController>()
                {
                    new NVIDIAGeForceNowEnablerPlayController(game, supportedGame.Id.ToString(), geforceNowExecutablePath, geforceNowWorkingPath)
                };

            }

            return null;
        }


    }
}