using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using static ResolutionChanger.DisplayUtilities;

namespace ResolutionChanger
{
    public class ResolutionChanger : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private bool resolutionChanged = false;
        private List<KeyValuePair<int, int>> detectedResolutions;
        private List<MainMenuItem> mainMenuitems;
        private DEVMODE devModePriorSet = new DEVMODE();

        private ResolutionChangerSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("32b6a5c7-be17-4852-b4f7-f059a7321f4c");

        public ResolutionChanger(IPlayniteAPI api) : base(api)
        {
            settings = new ResolutionChangerSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        private bool IsAnyOtherGameRunning(Game game)
        {
            return PlayniteApi.Database.Games.Any(x => x.IsRunning && x.Id != game.Id);
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            var game = args.Game;
            if (game.Features == null || game.Features.Count == 0)
            {
                return;
            }

            if (settings.Settings.ChangeResOnlyGamesNotRunning && IsAnyOtherGameRunning(game))
            {
                logger.Debug("Another game was detected as running during game start");
                return;
            }

            foreach (var feature in game.Features)
            {
                var resMatch = Regex.Match(feature.Name, @"^\[RC\] (\d+)x(\d+)$", RegexOptions.IgnoreCase);
                if (!resMatch.Success)
                {
                    continue;
                }

                var currentDevMode = DisplayHelper.GetCurrentScreenDevMode();
                var width = int.Parse(resMatch.Groups[1].Value);
                var height = int.Parse(resMatch.Groups[2].Value);
                var resChanged = DisplayHelper.ChangeResolution(width, height, currentDevMode);
                if (resChanged)
                {
                    if (devModePriorSet.dmPelsWidth == 0 || devModePriorSet.dmPelsHeight == 0)
                    {
                        devModePriorSet = currentDevMode;
                        logger.Info($"Stored DevMode. Device: {devModePriorSet.dmDeviceName}. Resolution {devModePriorSet.dmPelsWidth}x{devModePriorSet.dmPelsHeight}");
                    }
                    resolutionChanged = true;
                }

                break;
            }
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return mainMenuitems;
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            if (!resolutionChanged)
            {
                return;
            }

            //Due to issue #2634 this event is raised before the game IsRunning property
            //is reverted to false, so we need to verify that only this game is set to
            //running in all the database
            if (settings.Settings.ChangeResOnlyGamesNotRunning && IsAnyOtherGameRunning(args.Game))
            {
                logger.Debug("Another game was detected as running during game stop");
                return;
            }

            if (devModePriorSet.dmPelsWidth == 0 || devModePriorSet.dmPelsHeight == 0)
            {
                return;
            }

            var width = devModePriorSet.dmPelsWidth;
            var height = devModePriorSet.dmPelsHeight;
            logger.Info($"Restoring previous resolution {width}x{height}");
            if (DisplayHelper.RestoreResolution(devModePriorSet))
            {
                resolutionChanged = false;
                devModePriorSet = new DEVMODE();
            }
            else
            {
                logger.Info($"Failed to restore resolution");
            }
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            mainMenuitems = new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCResolutionChanger_MenuItemDescriptionClearResolutionFeature"),
                    MenuSection = "@Resolution Changer",
                    Action = a =>
                    {
                        RemoveResolutionConfigurationSelected();
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCResolutionChanger_DialogMessageDone"));
                    }
                }
            };

            detectedResolutions = DisplayHelper.GetPossibleResolutions()
                .Distinct()
                .OrderByDescending(x => x.Key).ThenByDescending(x => x.Value).ToList();
            foreach (var resolution in detectedResolutions)
            {
                mainMenuitems.Add(
                    new MainMenuItem
                    { 
                        Description = string.Format(ResourceProvider.GetString("LOCResolutionChanger_MenuItemDescriptionSetLaunchResolutionFeature"), resolution.Key, resolution.Value, DisplayHelper.GetResolutionAspectRatio(resolution.Key, resolution.Value)),
                        MenuSection = "@Resolution Changer",
                        Action = a => {
                            PlayniteUtilities.AddFeatureToGames(PlayniteApi, PlayniteApi.MainView.SelectedGames.Distinct(), $"[RC] {resolution.Key}x{resolution.Value}");
                            PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCResolutionChanger_DialogMessageDone"));
                        }
                    }
                );
            }
        }

        private void RemoveResolutionConfigurationSelected()
        {
            foreach (var game in PlayniteApi.MainView.SelectedGames.Distinct())
            {
                if (game.FeatureIds == null)
                {
                    continue;
                }

                var rcFeatures = game.Features.Where(x => Regex.IsMatch(x.Name, @"^\[RC\] (\d+)x(\d+)$", RegexOptions.IgnoreCase));
                if (rcFeatures != null && rcFeatures.Count() > 0)
                {
                    foreach (var rcFeature in rcFeatures)
                    {
                        game.FeatureIds.Remove(rcFeature.Id);
                    }

                    PlayniteApi.Database.Games.Update(game);
                }
            }
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new ResolutionChangerSettingsView();
        }


    }
}