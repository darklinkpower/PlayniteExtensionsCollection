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

        private NVIDIAGeForceNowEnablerSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("5f2dfd12-5f13-46fe-bcdd-64eb53ace26a");

        public NVIDIAGeForceNowEnabler(IPlayniteAPI api) : base(api)
        {
            settings = new NVIDIAGeForceNowEnablerSettingsViewModel(this);
            geforceNowWorkingPath = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), "NVIDIA Corporation", "GeForceNOW", "CEF");
            geforceNowExecutablePath = Path.Combine(geforceNowWorkingPath, "GeForceNOWStreamer.exe");
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
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
                    Description = "Update enabled status of games",
                    MenuSection = "@NVIDIA GeForce NOW Enabler",
                    Action = o => {
                        MainMethod(true);
                    }
                },
                new MainMenuItem
                {
                    Description = "Remove Play Action from all games",
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

        public string UpdateNvidiaAction(Game game, GeforceGame supportedGame)
        {
            GameAction geforceNowAction = null;
            if (game.GameActions != null)
            {
                geforceNowAction = game.GameActions
                    .Where(x => x.Arguments != null)
                    .Where(x => Regex.IsMatch(x.Arguments, @"--url-route=""#\?cmsId=\d+&launchSource=External&shortName=game_gfn_pc&parentGameId="""))
                    .FirstOrDefault();
            }

            if (supportedGame == null && geforceNowAction != null)
            {
                game.GameActions.Remove(geforceNowAction);
                PlayniteApi.Database.Games.Update(game);
                return "ActionRemoved";
            }
            else if (supportedGame != null && geforceNowAction == null)
            {
                GameAction nvidiaGameAction = new GameAction()
                {
                    Name = "Launch in Nvidia GeForce NOW",
                    Arguments = String.Format("--url-route=\"#?cmsId={0}&launchSource=External&shortName=game_gfn_pc&parentGameId=\"", supportedGame.Id),
                    Path = geforceNowExecutablePath,
                    WorkingDir = geforceNowWorkingPath,
                    IsPlayAction = settings.Settings.SetActionsAsPlayAction,
                    TrackingMode = TrackingMode.Process
                };

                if (game.GameActions == null)
                {
                    game.GameActions = new System.Collections.ObjectModel.ObservableCollection<GameAction> { nvidiaGameAction };
                }
                else
                {
                    //TODO figure why error happens: This type of CollectionView does not admit changes to the SourceCollection of a subprocess other than the Dispatcher subprocess.
                    //Error: Game has a GameAction- > Delete it from Edit Window -> Try to add the GameAction with extension -> Triggers the error
                    //No error: Game has a GameAction -> Delete it from Edit Window -> Reboot Playnite -> Try to add the GameAction with extension -> Error doesn't trigger

                    //game.GameActions.Add(nvidiaGameAction);

                    //The workaround fix is to create a new collection, add all the current game GameActions
                    // and the new GameAction and finally set this new collection to the game GameActions
                    var newCollection = new System.Collections.ObjectModel.ObservableCollection<GameAction> { };
                    foreach (GameAction gameAction in game.GameActions)
                    {
                        newCollection.Add(gameAction);
                    }
                    newCollection.Add(nvidiaGameAction);
                    game.GameActions = newCollection;
                    PlayniteApi.Database.Games.Update(game);
                    return "ActionAdded";
                }
            }
            
            return null;
        }

        public List<GeforceGame> DownloadGameList(string uri, bool showDialogs)
        {
            var supportedGames = new List<GeforceGame>();

            var progRes = PlayniteApi.Dialogs.ActivateGlobalProgress((a) => {
                using (var webClient = new WebClient())
                {
                    try
                    {
                        webClient.Encoding = Encoding.UTF8;
                        string downloadedString = webClient.DownloadString(uri);
                        supportedGames = JsonConvert.DeserializeObject<List<GeforceGame>>(downloadedString);
                        foreach (var supportedGame in supportedGames)
                        {
                            supportedGame.Title = Regex.Replace(supportedGame.Title, @"[^\p{L}\p{Nd}]", "").ToLower();
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
            }, new GlobalProgressOptions("Downloading NVIDIA GeForce Now database..."));
            
            return supportedGames;
        }

        public IEnumerable<Game> GetGamesSupportedLibraries()
        {
            List<Guid> supportedLibraries = new List<Guid>()
            {
                BuiltinExtensions.GetIdFromExtension(BuiltinExtension.EpicLibrary),
                BuiltinExtensions.GetIdFromExtension(BuiltinExtension.OriginLibrary),
                BuiltinExtensions.GetIdFromExtension(BuiltinExtension.SteamLibrary),
                BuiltinExtensions.GetIdFromExtension(BuiltinExtension.UplayLibrary),
                BuiltinExtensions.GetIdFromExtension(BuiltinExtension.GogLibrary)
            };

            var gameDatabase = PlayniteApi.Database.Games.Where(g => supportedLibraries.Contains(g.PluginId));
            return gameDatabase;
        }

        public void MainMethod(bool showDialogs)
        {
            string featureName = "NVIDIA GeForce NOW";
            GameFeature feature = PlayniteApi.Database.Features.Add(featureName);

            var supportedGames = DownloadGameList("https://static.nvidiagrid.net/supported-public-game-list/gfnpc.json", showDialogs);
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
            int playActionAddedCount = 0;
            int playActionRemovedCount = 0;
            int setAsInstalledCount = 0;
            int setAsUninstalledCount = 0;

            var progRes = PlayniteApi.Dialogs.ActivateGlobalProgress((a) => {

            var gameDatabase = GetGamesSupportedLibraries();
            foreach (var game in gameDatabase)
            {
                var gameName = Regex.Replace(game.Name, @"[^\p{L}\p{Nd}]", "").ToLower();
                GeforceGame supportedGame = null;
                switch (BuiltinExtensions.GetExtensionFromId(game.PluginId))
                {
                    case BuiltinExtension.SteamLibrary:
                        var steamUrl = String.Format("https://store.steampowered.com/app/{0}", game.GameId);
                        supportedGame = supportedSteamGames.Where(g => g.SteamUrl == steamUrl).FirstOrDefault();
                        break;
                    case BuiltinExtension.EpicLibrary:
                        supportedGame = supportedEpicGames.Where(g => g.Title == gameName).FirstOrDefault();
                        break;
                    case BuiltinExtension.OriginLibrary:
                        supportedGame = supportedOriginGames.Where(g => g.Title == gameName).FirstOrDefault();
                        break;
                    case BuiltinExtension.UplayLibrary:
                        supportedGame = supportedUplayGames.Where(g => g.Title == gameName).FirstOrDefault();
                        break;
                    case BuiltinExtension.GogLibrary:
                        supportedGame = supportedGogGames.Where(g => g.Title == gameName).FirstOrDefault();
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
                        logger.Info(String.Format("Feature removed from \"{0}\"", game.Name));

                        if (settings.Settings.SetEnabledGamesAsInstalled == true && game.IsInstalled == true)
                        {
                            game.IsInstalled = true;
                            setAsUninstalledCount++;
                            PlayniteApi.Database.Games.Update(game);
                            logger.Info(String.Format("Set \"{0}\" as uninstalled", game.Name));
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
                        logger.Info(String.Format("Feature added to \"{0}\"", game.Name));
                    }
                    if (settings.Settings.SetEnabledGamesAsInstalled == true && game.IsInstalled == false)
                    {
                        game.IsInstalled = true;
                        setAsInstalledCount++;
                        PlayniteApi.Database.Games.Update(game);
                        logger.Info(String.Format("Set \"{0}\" as installed", game.Name));
                    }
                }

                if (settings.Settings.UpdatePlayActions == true)
                {
                    var updatePlayAction = UpdateNvidiaAction(game, supportedGame);
                    if (updatePlayAction == "ActionAdded")
                    {
                        playActionAddedCount++;
                        logger.Info(String.Format("Play Action added to \"{0}\"", game.Name));
                    }
                    else if (updatePlayAction == "ActionRemoved")
                    {
                        playActionRemovedCount++; 
                        logger.Info(String.Format("Play Action removed from \"{0}\"", game.Name));
                    }
                }
            } }, new GlobalProgressOptions("Updating NVIDIA GeForce NOW Enabled games"));

            if (showDialogs == true)
            {
                string results = String.Format("NVIDIA GeForce NOW enabled games in library: {0}\n\nAdded \"{1}\" feature to {2} games.\nRemoved \"{3}\" feature from {4} games.",
                    enabledGamesCount, featureName, featureAddedCount, featureName, featureRemovedCount);
                if (settings.Settings.UpdatePlayActions == true)
                {
                    results += String.Format("\n\nPlay Action added to {0} games.\nPlay Action removed from {1} games.",
                        playActionAddedCount, playActionRemovedCount);
                }
                if (settings.Settings.SetEnabledGamesAsInstalled == true)
                {
                    results += String.Format("\n\nSet {0} games as Installed.\nSet {1} games as uninstalled.", setAsInstalledCount, setAsUninstalledCount);
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
                    logger.Info(String.Format("Play Action removed from \"{0}\"", game.Name));
                }
            }
            string results = String.Format("Play Action removed from {0} games", playActionRemovedCount);
            PlayniteApi.Dialogs.ShowMessage(results, "NVIDIA GeForce NOW Enabler");
        }
    }
}