using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteControlLocker.Models;
using PlayState.Enums;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PlayniteControlLocker
{
    public class PlayniteControlLocker : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private ExecutionModes currentPlayniteMode = ExecutionModes.FullMode;
        private List<RestoreItem> gamesToRestore = new List<RestoreItem>();
        private Dictionary<Guid, OriginalItem> startGameValues = new Dictionary<Guid, OriginalItem>();
        private DispatcherTimer timerRestoreGames;

        private PlayniteControlLockerSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("e7c39fe7-bec1-4691-a818-b9ba1470ad21");

        public PlayniteControlLocker(IPlayniteAPI api) : base(api)
        {
            settings = new PlayniteControlLockerSettingsViewModel(this, PlayniteApi);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
            SetSubscriptions();

            timerRestoreGames = new DispatcherTimer();
            timerRestoreGames.Interval = TimeSpan.FromMilliseconds(500);
            timerRestoreGames.Tick += new EventHandler(RestoreGames_Tick);
        }

        private void RestoreGames_Tick(object sender, EventArgs e)
        {
            timerRestoreGames.Stop();
            foreach (var removedItem in gamesToRestore.ToList())
            {
                var existingGame = PlayniteApi.Database.Games[removedItem.Game.Id];
                if (existingGame != null)
                {
                    logger.Error($"Game {removedItem.Game.Name} with Id {removedItem.Game.Id} still existed when adding");
                    gamesToRestore.Remove(removedItem);
                    continue;
                }
                
                PlayniteApi.Database.Games.Add(removedItem.Game);
                var addedGame = PlayniteApi.Database.Games[removedItem.Game.Id];
                addedGame.BackgroundImage = RestoreMediaToRemovedGame(addedGame, removedItem.BackgroundImage);
                addedGame.CoverImage = RestoreMediaToRemovedGame(addedGame, removedItem.CoverImage);
                addedGame.Icon = RestoreMediaToRemovedGame(addedGame, removedItem.Icon);

                PlayniteApi.Database.Games.Update(addedGame);
                gamesToRestore.Remove(removedItem);
            }
        }

        private string RestoreMediaToRemovedGame(Game game, string mediaPath)
        {
            if (mediaPath.IsNullOrEmpty())
            {
                return null;
            }

            if (FileSystem.FileExists(mediaPath))
            {
                var restoredMediaPath = PlayniteApi.Database.AddFile(mediaPath, game.Id);
                FileSystem.DeleteFileSafe(mediaPath);
                return restoredMediaPath;
            }
            else if (mediaPath.StartsWith("http"))
            {
                return mediaPath;
            }

            return null;
        }

        private void SetSubscriptions()
        {
            PlayniteApi.Database.Games.ItemCollectionChanged += (sender, ItemCollectionChangedArgs) =>
            {
                var removedGames = 0;
                foreach (var removedItem in ItemCollectionChangedArgs.RemovedItems)
                {
                    if (settings.Settings.ReadModeAllowDeleteGames)
                    {
                        continue;
                    }
                    
                    var backgroundImage = string.Empty;
                    if (!removedItem.BackgroundImage.IsNullOrEmpty())
                    {
                        backgroundImage = StoreAndReturnTempMediaPath(
                            PlayniteApi.Database.GetFullFilePath(removedItem.BackgroundImage));
                    }

                    var coverImage = string.Empty;
                    if (!removedItem.CoverImage.IsNullOrEmpty())
                    {
                        coverImage = StoreAndReturnTempMediaPath(
                            PlayniteApi.Database.GetFullFilePath(removedItem.CoverImage));
                    }

                    var icon = string.Empty;
                    if (!removedItem.Icon.IsNullOrEmpty())
                    {
                        icon = StoreAndReturnTempMediaPath(
                            PlayniteApi.Database.GetFullFilePath(removedItem.Icon));
                    }

                    var restoreItem = new RestoreItem
                    {
                        Game = removedItem,
                        BackgroundImage = backgroundImage,
                        CoverImage = coverImage,
                        Icon = icon
                    };

                    gamesToRestore.Add(restoreItem);
                    removedGames++;
                }

                if (removedGames > 0)
                {
                    timerRestoreGames.Start();
                }

            };

            PlayniteApi.Database.Games.ItemUpdated += (sender, ItemUpdatedArgs) =>
            {
                foreach (var updatedItem in ItemUpdatedArgs.UpdatedItems)
                {
                    if (!startGameValues.ContainsKey(updatedItem.OldData.Id))
                    {
                        startGameValues[updatedItem.OldData.Id] = new OriginalItem
                        {
                            Hidden = updatedItem.OldData.Hidden,
                            Favorite = updatedItem.OldData.Favorite
                        };
                    }

                    if (!settings.Settings.ReadModeAllowHiding)
                    {
                        if (updatedItem.NewData.Hidden != startGameValues[updatedItem.OldData.Id].Hidden)
                        {
                            var game = PlayniteApi.Database.Games[updatedItem.OldData.Id];
                            if (game != null)
                            {
                                game.Hidden = startGameValues[updatedItem.OldData.Id].Hidden;
                                PlayniteApi.Database.Games.Update(game);
                            }
                        }
                    }

                    if (!settings.Settings.ReadModeAllowFavorites)
                    {
                        if (updatedItem.NewData.Favorite != startGameValues[updatedItem.OldData.Id].Favorite)
                        {
                            var game = PlayniteApi.Database.Games[updatedItem.OldData.Id];
                            if (game != null)
                            {
                                game.Favorite = startGameValues[updatedItem.OldData.Id].Favorite;
                                PlayniteApi.Database.Games.Update(game);
                            }
                        }
                    }
                }
            };
        }

        private string StoreAndReturnTempMediaPath(string path)
        {
            if (FileSystem.FileExists(path))
            {
                var targetPath = Path.Combine(GetPluginUserDataPath(), Path.GetFileName(path));
                FileSystem.CopyFile(path, targetPath);
                return targetPath;
            }
            else if (path.StartsWith("http"))
            {
                return path;
            }

            return string.Empty;
        }

        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
            // Add code to be executed when game is finished installing.
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            // Add code to be executed when game is started running.
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
            // Add code to be executed when game is uninstalled.
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {

            if (!settings.Settings.PasswordSet)
            {
                return;
            }
            
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                if (!settings.Settings.EnableOnDesktopMode)
                {
                    return;
                }

                if (settings.ValidatePassword())
                {
                    PlayniteApi.Dialogs.ShowMessage("LOC_PlayniteControlLocker_DialogMessagePlaniteFullModeStart", "Playnite Control Locker");
                    return;
                }


                var playniteExecutable = Path.Combine(PlayniteApi.Paths.ApplicationPath, "Playnite.DesktopApp.exe");
                if (settings.Settings.DesktopCheckCloseOnFail && File.Exists(playniteExecutable))
                {
                    PlayniteApi.Dialogs.ShowMessage(
                        ResourceProvider.GetString("LOC_PlayniteControlLocker_DialogMessagePasswordIncorrect") +
                        "\n\n" +
                        ResourceProvider.GetString("LOC_PlayniteControlLocker_DialogMessagePlaniteClosing"),
                        "Playnite Control Locker");

                    ProcessStarter.StartProcess(playniteExecutable, "--shutdown");
                    return;
                }
            }
            else
            {
                if (!settings.Settings.EnableOnFullscreenMode)
                {
                    return;
                }

                if (settings.ValidatePassword())
                {
                    PlayniteApi.Dialogs.ShowMessage("LOC_PlayniteControlLocker_DialogMessagePlaniteFullModeStart", "Playnite Control Locker");
                    return;
                }

                var playniteExecutable = Path.Combine(PlayniteApi.Paths.ApplicationPath, "Playnite.FullscreenApp.exe");
                if (settings.Settings.FullcreenCheckCloseOnFail && File.Exists(playniteExecutable))
                {
                    PlayniteApi.Dialogs.ShowMessage(
                        ResourceProvider.GetString("LOC_PlayniteControlLocker_DialogMessagePasswordIncorrect") +
                        "\n\n" +
                        ResourceProvider.GetString("LOC_PlayniteControlLocker_DialogMessagePlaniteClosing"),
                        "Playnite Control Locker");

                    ProcessStarter.StartProcess(playniteExecutable, "--shutdown");
                    return;
                }
            }

            PlayniteApi.Dialogs.ShowMessage(
                ResourceProvider.GetString("LOC_PlayniteControlLocker_DialogMessagePasswordIncorrect") +
                "\n\n" +
                ResourceProvider.GetString("LOC_PlayniteControlLocker_DialogMessagePlaniteControlModeStart"),
                "Playnite Control Locker");
            
            SetSubscriptions();
            currentPlayniteMode = ExecutionModes.ControlMode;
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            // Add code to be executed when Playnite is shutting down.
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // Add code to be executed when library is updated.
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new PlayniteControlLockerSettingsView();
        }
    }
}