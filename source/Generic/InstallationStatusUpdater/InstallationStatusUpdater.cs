using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace InstallationStatusUpdater
{
    public class InstallationStatusUpdater : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private static readonly Regex driveRegex = new Regex(@"^\w:\\", RegexOptions.Compiled);

        private InstallationStatusUpdaterSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("ed9c467f-5ab5-478f-a09f-936146188ad0");

        public InstallationStatusUpdater(IPlayniteAPI api) : base(api)
        {
            settings = new InstallationStatusUpdaterSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (settings.Settings.UpdateOnStartup == true)
            {
                DetectInstallationStatus(false);
            }
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            if (settings.Settings.UpdateOnLibraryUpdate == true)
            {
                DetectInstallationStatus(false);
            }
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new InstallationStatusUpdaterSettingsView();
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {

            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCInstallation_Status_Updater_MenuItemStatusUpdaterDescription"),
                    MenuSection = "@Installation Status Updater",
                    Action = o => {
                        DetectInstallationStatus(true);
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCInstallation_Status_Updater_MenuAddIgnoreFeatureDescription"),
                    MenuSection = "@Installation Status Updater",
                    Action = o => {
                        AddIgnoreFeature();
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCInstallation_Status_Updater_MenuRemoveIgnoreFeatureDescription"),
                    MenuSection = "@Installation Status Updater",
                    Action = o => {
                        RemoveIgnoreFeature();
                    }
                }
            };
        }

        public bool DetectIsRomInstalled(GameRom rom, string installDirectory)
        {
            if (string.IsNullOrEmpty(rom.Path))
            {
                return false;
            }

            if (driveRegex.IsMatch(rom.Path))
            {
                return File.Exists(rom.Path);
            }

            string romFullPath = rom.Path;
            if (!string.IsNullOrEmpty(installDirectory))
            {
                romFullPath = Path.Combine(installDirectory, rom.Path);
            }

            return File.Exists(romFullPath);
        }

        public bool DetectIsRomInstalled(Game game, string installDirectory)
        {
            if (game.Roms == null)
            {
                return false;
            }
            if (game.Roms.Count == 0)
            {
                return false;
            }
            if (game.Roms.Count > 1 && settings.Settings.UseOnlyFirstRomDetection == false)
            {
                var isInstalled = false;
                foreach (GameRom rom in game.Roms)
                {
                    isInstalled = DetectIsRomInstalled(rom, installDirectory);
                    if (isInstalled == true)
                    {
                        return true;
                    }
                }
            }
            else
            {
                return DetectIsRomInstalled(game.Roms[0], installDirectory);
            }

            return false;
        }

        public bool DetectIsFileActionInstalled(GameAction gameAction, string installDirectory)
        {
            if (string.IsNullOrEmpty(gameAction.Path))
            {
                return false;
            }

            if (driveRegex.IsMatch(gameAction.Path))
            {
                return File.Exists(gameAction.Path);
            }

            var fullfilePath = gameAction.Path;
            if (!string.IsNullOrEmpty(installDirectory))
            {
                fullfilePath = Path.Combine(installDirectory, fullfilePath);
            }

            return File.Exists(fullfilePath);
        }

        public bool DetectIsAnyActionInstalled(Game game, string installDirectory)
        {
            var isInstalled = false;
            foreach (GameAction gameAction in game.GameActions)
            {
                if (gameAction.IsPlayAction = false && settings.Settings.OnlyUsePlayActionGameActions == true)
                {
                    continue;
                }
                
                if (gameAction.Type == GameActionType.URL)
                {
                    if (settings.Settings.UrlActionIsInstalled == true)
                    {
                        return true;
                    }
                }
                else if (gameAction.Type == GameActionType.Script)
                {
                    if (settings.Settings.ScriptActionIsInstalled == true)
                    {
                        return true;
                    }
                }
                else if (gameAction.Type == GameActionType.File)
                {
                    if (settings.Settings.ScriptActionIsInstalled == true)
                    {
                        isInstalled = DetectIsFileActionInstalled(gameAction, installDirectory);
                        if (isInstalled == true)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void DetectInstallationStatus(bool showResultsDialog)
        {
            var gameCollection = PlayniteApi.Database.Games;
            string skipFeatureName = "[Status Updater] Ignore";
            int markedInstalled = 0;
            int markedUninstalled = 0;
            foreach (Game game in gameCollection)
            {
                if (game.IncludeLibraryPluginAction == true && settings.Settings.SkipHandledByPlugin == true)
                {
                    continue;
                }
                
                if (game.Features != null)
                {
                    var skipGame = false;
                    foreach (GameFeature gameFeature in game.Features)
                    {
                        if (gameFeature.Name == skipFeatureName)
                        {
                            skipGame = true;
                            break;
                        }
                    }
                    if (skipGame == true)
                    {
                        continue;
                    }
                }
                
                var isInstalled = false;
                var installDirectory = string.Empty;
                if (!string.IsNullOrEmpty(game.InstallDirectory))
                {
                    installDirectory = game.InstallDirectory.ToLower();
                }

                if (game.GameActions == null)
                {
                    isInstalled = false;
                }
                else if (game.GameActions.Count > 0)
                {
                    isInstalled = DetectIsAnyActionInstalled(game, installDirectory);
                }

                if (isInstalled == false)
                {
                    isInstalled = DetectIsRomInstalled(game, installDirectory);
                }

                if (game.IsInstalled == true && isInstalled == false)
                {
                    game.IsInstalled = false;
                    PlayniteApi.Database.Games.Update(game);
                    markedUninstalled++;
                    logger.Info(string.Format("Game: {0} marked as uninstalled", game.Name));
                }
                else if (game.IsInstalled == false && isInstalled == true)
                {
                    game.IsInstalled = true;
                    PlayniteApi.Database.Games.Update(game);
                    markedInstalled++;
                    logger.Info(string.Format("Game: {0} marked as installed", game.Name));
                }
            }

            if (showResultsDialog == true)
            {
                PlayniteApi.Dialogs.ShowMessage(
                    string.Format(ResourceProvider.GetString("LOCInstallation_Status_Updater_StatusUpdaterResultsMessage"), 
                    markedUninstalled.ToString(), 
                    markedInstalled.ToString()), "Installation Status Updater"
                );
            }
        }

        public void AddIgnoreFeature()
        {
            string skipFeatureName = "[Status Updater] Ignore";
            GameFeature feature = PlayniteApi.Database.Features.Add(skipFeatureName);
            int featureAddedCount = 0;
            var gameCollection = PlayniteApi.MainView.SelectedGames;
            foreach (Game game in gameCollection)
            {
                if (game.FeatureIds == null)
                {
                    game.FeatureIds = new List<Guid> { feature.Id };
                    continue;
                }

                var containsFeature = false;
                foreach (Guid featureId in game.FeatureIds)
                {
                    if (featureId == feature.Id)
                    {
                        containsFeature = true;
                        break;
                    }
                }

                if (containsFeature == false)
                {
                    game.FeatureIds.Add(feature.Id);
                    PlayniteApi.Database.Games.Update(game);
                    featureAddedCount++;
                    logger.Info(string.Format("Game: {0} Added ignore feature", game.Name));
                }
            }

            PlayniteApi.Dialogs.ShowMessage(
                string.Format(ResourceProvider.GetString("LOCInstallation_Status_Updater_StatusUpdaterAddIgnoreFeatureMessage"),
                featureAddedCount.ToString()), "Installation Status Updater"
            );
        }

        public void RemoveIgnoreFeature()
        {
            string skipFeatureName = "[Status Updater] Ignore";
            GameFeature feature = PlayniteApi.Database.Features.Add(skipFeatureName);
            int featureRemovedCount = 0;
            var gameCollection = PlayniteApi.MainView.SelectedGames;
            foreach (Game game in gameCollection)
            {
                if (game.FeatureIds == null)
                {
                    continue;
                }

                foreach (Guid featureId in game.FeatureIds)
                {
                    if (featureId == feature.Id)
                    {
                        game.FeatureIds.Remove(feature.Id);
                        PlayniteApi.Database.Games.Update(game);
                        featureRemovedCount++;
                        logger.Info(string.Format("Game: {0} Removed ignore feature", game.Name));
                        break;
                    }
                }
            }

            PlayniteApi.Dialogs.ShowMessage(
                string.Format(ResourceProvider.GetString("LOCInstallation_Status_Updater_StatusUpdaterRemoveIgnoreFeatureMessage"), 
                featureRemovedCount.ToString()), "Installation Status Updater"
            );
        }
    }
}