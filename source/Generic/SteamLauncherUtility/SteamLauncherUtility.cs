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
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SteamLauncherUtility
{
    public class SteamLauncherUtility : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static Guid steamPluginId = Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab");

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
            if (!Steam.IsGameSteamGame(game))
            {
                return;
            }

            if (!settings.Settings.RestartIfRunningIncorrectArgs && SteamClient.GetIsSteamRunning())
            {
                return;
            }

            if (PlayniteApi.Database.Games.Any(x => x.IsRunning && x.PluginId == steamPluginId))
            {
                logger.Info("A Steam game was detected as running and execution was stopped");
                return;
            }

            var modeFeatureName = GetModeFeatureName();
            var argumentsList = GetSteamLaunchArguments();
            if (game.Features != null)
            {
                var featureFound = game.Features.Any(f => f.Name.Equals(modeFeatureName, StringComparison.OrdinalIgnoreCase));

                // Mode 0: Global mode. Mode 1: Selective mode
                if ((settings.Settings.LaunchMode == 0 && featureFound) || (settings.Settings.LaunchMode == 1 && !featureFound))
                {
                    logger.Info(string.Format("Stopped execution in game \"{0}\". Mode \"{1}\". Feature: \"{2}\". Found: \"{3}\"", game.Name, settings.Settings.LaunchMode, modeFeatureName, featureFound));
                    if (IsSteamRunningWithArgs(argumentsList))
                    {
                        SteamClient.StartSteam(true, string.Empty);
                    }

                    return;
                }
            }

            if (IsSteamRunningWithArgs(argumentsList))
            {
                return;
            }

            SteamClient.StartSteam(true, argumentsList);
        }

        private bool IsSteamRunningWithArgs(List<string> argumentsList)
        {
            if (!SteamClient.GetIsSteamRunning())
            {
                return false;
            }

            var wmiQuery = string.Format("select CommandLine from Win32_Process where Name='{0}'", "steam.exe");
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery))
            {
                using (ManagementObjectCollection retObjectCollection = searcher.Get())
                {
                    foreach (ManagementObject retObject in retObjectCollection)
                    {
                        var startArguments = retObject["CommandLine"].ToString();
                        if (startArguments.IsNullOrEmpty())
                        {
                            continue;
                        }

                        logger.Debug($"Steam is running with arguments {startArguments}");
                        var matchingArgsCount = argumentsList.Where(x => startArguments.Contains(x, StringComparison.OrdinalIgnoreCase)).Count();
                        if (argumentsList.Count == matchingArgsCount)
                        {
                            logger.Debug($"Steam is running with same arguments");
                            return true;
                        }
                    }

                    return false;
                }
            }
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
                        var argumentsList = GetSteamLaunchArguments();
                        var argumentsString = argumentsList.Aggregate((x, b) => x + " " + b);
                        if (IsSteamRunningWithArgs(argumentsList))
                        {
                            PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCSteam_Launcher_UtilityDialogMessageSteamIsRunningCorrectArgs"), argumentsString), "Steam Launcher Utility");
                            return;
                        }

                        SteamClient.StartSteam(true, argumentsString);
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

        public List<string> GetSteamLaunchArguments()
        {
            var argumentsList = new List<string>();
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                if (settings.Settings.DisableSteamWebBrowserOnDesktopMode)
                {
                    argumentsList.Add("-no-browser");
                }

                if (settings.Settings.LaunchSteamBpmOnDesktopMode)
                {
                    argumentsList.Add("-bigpicture");
                }
                else
                {
                    argumentsList.Add("-silent");
                }
            }
            else if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen)
            {
                if (settings.Settings.DisableSteamWebBrowserOnFullscreenMode)
                {
                    argumentsList.Add("-no-browser");
                }

                if (settings.Settings.LaunchSteamBpmOnFullscreenMode)
                {
                    argumentsList.Add("-bigpicture");
                }
                else
                {
                    argumentsList.Add("-silent");
                }
            }

            return argumentsList;
        }

        public void AddModeFeature()
        {
            var featureName = GetModeFeatureName();
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