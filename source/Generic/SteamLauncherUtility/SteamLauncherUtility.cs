using Microsoft.Win32;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SteamLauncherUtility
{
    public class SteamLauncherUtility : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private SteamLauncherUtilitySettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("31a65402-5b0c-44f0-9fc2-44b22ca4263c");

        public SteamLauncherUtility(IPlayniteAPI api) : base(api)
        {
            settings = new SteamLauncherUtilitySettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            var game = args.Game;
            if (game.IsInstalled == false)
            {
                return;
            }

            if (BuiltinExtensions.GetExtensionFromId(game.PluginId) != BuiltinExtension.SteamLibrary)
            {
                return;
            }

            string modeFeatureName = GetModeFeatureName();
            if (game.Features != null)
            {
                var matchingFeature = game.Features.Where(f => f.Name == modeFeatureName);
                if (settings.Settings.LaunchMode == 0 && matchingFeature.Count() > 0)
                {
                    logger.Info(string.Format("Stopped execution in game \"{0}\". Global mode and game has \"{1}\" feature", game.Name, modeFeatureName));
                    return;
                }
                else if (settings.Settings.LaunchMode == 1 && matchingFeature.Count() == 0)
                {
                    logger.Info(string.Format("Stopped execution in game \"{0}\". Selective mode and game doesn't have \"{1}\" feature", game.Name, modeFeatureName));
                    return;
                }
            }

            LaunchSteam();
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new SteamLauncherUtilitySettingsView();
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSteam_Launcher_UtilityMenuItemLaunchSteamConfiguredActionsDescription"),
                    MenuSection = "@Steam Launcher Utility",
                    Action = a => {
                        LaunchSteam();
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSteam_Launcher_UtilityMenuItemAddFilterFeatureDescription"),
                    MenuSection = "@Steam Launcher Utility",
                    Action = a => {
                        AddModeFeature();
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSteam_Launcher_UtilityMenuItemRemoveModeFilterFeatureDescription"),
                    MenuSection = "@Steam Launcher Utility",
                    Action = a => {
                        RemoveModeFeature();
                    }
                },
            };
        }

        public bool GetIsSteamRunning()
        {
            Process[] processes = Process.GetProcessesByName("Steam");
            if (processes.Length > 0)
            {
                return true;
            }
            
            return false;
        }

        public string GetModeFeatureName()
        {
            if (settings.Settings.LaunchMode == 0)
            {
                return "[SLU] Global Mode block";
            }
            else
            {
                return "[SLU] Selective Mode allow";
            }
        }

        public string GetSteamInstallationPath()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
            {
                if (key?.GetValueNames().Contains("SteamExe") == true)
                {
                    return key.GetValue("SteamExe")?.ToString().Replace('/', '\\') ?? "C:\\Program Files (x86)\\Steam\\steam.exe";
                }
            }

            return "C:\\Program Files (x86)\\Steam\\steam.exe";
        }

        public string GetSteamLaunchArguments()
        {
            string arguments = "";
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                if (settings.Settings.DisableSteamWebBrowserOnDesktopMode == true)
                {
                    arguments = arguments + " -no-browser";
                }
                if (settings.Settings.LaunchSteamBpmOnDesktopMode == true)
                {
                    arguments = arguments + " -bigpicture";
                }
                else
                {
                    arguments = arguments + " -silent";
                }
            }
            else if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen)
            {
                if (settings.Settings.DisableSteamWebBrowserOnFullscreenMode == true)
                {
                    arguments = arguments + " -no-browser";
                }
                if (settings.Settings.LaunchSteamBpmOnFullscreenMode == true)
                {
                    arguments = arguments + " -bigpicture";
                }
                else
                {
                    arguments = arguments + " -silent";
                }
            }

            return arguments;
        }

        public void LaunchSteam()
        {
            string steamInstallationPath = GetSteamInstallationPath();
            if (!FileSystem.FileExists(steamInstallationPath))
            {
                logger.Error($"Steam executable not detected in path \"{steamInstallationPath}\"");
                return;
            }

            bool isSteamRunning = GetIsSteamRunning();
            if (isSteamRunning == true && settings.Settings.CloseSteamIfRunning == true)
            {
                ProcessStarter.StartProcess(steamInstallationPath, "-shutdown");
                logger.Info("Steam detected running. Closing via command line.");
                for (int i = 0; i < 8; i++)
                {
                    isSteamRunning = GetIsSteamRunning();
                    if (isSteamRunning == true)
                    {
                        logger.Info("Steam detected running.");
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        logger.Info("Steam has closed.");
                        break;
                    }
                }
            }

            if (isSteamRunning == false)
            {
                string launchArguments = GetSteamLaunchArguments();
                ProcessStarter.StartProcess(steamInstallationPath, launchArguments);
                logger.Info($"Steam launched with arguments: \"{launchArguments}\"");
            }
            else
            {
                logger.Warn("Steam was detected as running and was not launched via the extension.");
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

        public void AddModeFeature()
        {
            string featureName = GetModeFeatureName();
            GameFeature feature = PlayniteApi.Database.Features.Add(featureName);
            var gameDatabase = PlayniteApi.MainView.SelectedGames.Where(g => g.PluginId == BuiltinExtensions.GetIdFromExtension(BuiltinExtension.SteamLibrary));
            int featureAddedCount = 0;
            foreach (var game in gameDatabase)
            {
                bool featureAdded = AddFeature(game, feature);
                if (featureAdded == true)
                {
                    featureAddedCount++;
                    logger.Info(String.Format("Added \"{0}\" feature to \"{1}\"", featureName, game.Name));
                }
            }
            PlayniteApi.Dialogs.ShowMessage(String.Format("Added \"{0}\" feature to {1} game(s).", featureName, featureAddedCount), "Steam Launcher Utility");
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

        public void RemoveModeFeature()
        {
            string featureName = GetModeFeatureName();
            GameFeature feature = PlayniteApi.Database.Features.Add(featureName);
            var gameDatabase = PlayniteApi.MainView.SelectedGames.Where(g => g.PluginId == BuiltinExtensions.GetIdFromExtension(BuiltinExtension.SteamLibrary));
            int featureRemovedCount = 0;
            foreach (var game in gameDatabase)
            {
                bool featureRemoved = RemoveFeature(game, feature);
                if (featureRemoved == true)
                {
                    featureRemovedCount++;
                    logger.Info(String.Format("Removed \"{0}\" feature from \"{1}\"", featureName, game.Name));
                }
            }
            PlayniteApi.Dialogs.ShowMessage(String.Format("Removed \"{0}\" feature from {1} game(s).", featureName, featureRemovedCount), "Steam Launcher Utility");
        }
    }
}