using Microsoft.Win32;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using SteamCommon;
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
            if (!game.IsInstalled)
            {
                return;
            }

            if (!Steam.IsGameSteamGame(game))
            {
                return;
            }

            var modeFeatureName = GetModeFeatureName();
            if (game.Features != null)
            {
                var featureFound = game.Features.Any(f => f.Name == modeFeatureName);
                if (settings.Settings.LaunchMode == 0 && featureFound)
                {
                    logger.Info(string.Format("Stopped execution in game \"{0}\". Global mode and game has \"{1}\" feature", game.Name, modeFeatureName));
                    return;
                }
                else if (settings.Settings.LaunchMode == 1 && featureFound)
                {
                    logger.Info(string.Format("Stopped execution in game \"{0}\". Selective mode and game doesn't have \"{1}\" feature", game.Name, modeFeatureName));
                    return;
                }
            }

            SteamClient.StartSteam(settings.Settings.CloseSteamIfRunning, GetSteamLaunchArguments());
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
                        SteamClient.StartSteam(settings.Settings.CloseSteamIfRunning, GetSteamLaunchArguments());
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

        public string GetSteamLaunchArguments()
        {
            var sb = new StringBuilder();
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                if (settings.Settings.DisableSteamWebBrowserOnDesktopMode)
                {
                    sb.Append("-no-browser ");
                }
                if (settings.Settings.LaunchSteamBpmOnDesktopMode)
                {
                    sb.Append("-bigpicture ");
                }
                else
                {
                    sb.Append("-silent ");
                }
            }
            else if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen)
            {
                if (settings.Settings.DisableSteamWebBrowserOnFullscreenMode)
                {
                    sb.Append("-no-browser ");
                }
                if (settings.Settings.LaunchSteamBpmOnFullscreenMode)
                {
                    sb.Append("-bigpicture ");
                }
                else
                {
                    sb.Append("-silent ");
                }
            }

            return sb.ToString();
        }

        public void AddModeFeature()
        {
            var featureName = "[Splash Screen] Skip splash image";
            var featureAddedCount = PlayniteUtilities.AddFeatureToGames(PlayniteApi,
                PlayniteApi.MainView.SelectedGames.Distinct().Where(g => Steam.IsGameSteamGame(g)),
                featureName);
            PlayniteApi.Dialogs.ShowMessage(string.Format("Added \"{0}\" feature to {1} game(s).", featureName, featureAddedCount), "Steam Launcher Utility");
        }

        public void RemoveModeFeature()
        {
            var featureName = GetModeFeatureName();
            var featureRemovedCount = PlayniteUtilities.RemoveFeatureFromGames(
                PlayniteApi,
                PlayniteApi.MainView.SelectedGames.Where(g => Steam.IsGameSteamGame(g)),
                featureName);
            PlayniteApi.Dialogs.ShowMessage(string.Format("Removed \"{0}\" feature from {1} game(s).", featureName, featureRemovedCount), "Steam Launcher Utility");
        }
    }
}