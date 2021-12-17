using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
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
                if (resMatch.Success)
                {
                    var width = int.Parse(resMatch.Groups[1].Value);
                    var height = int.Parse(resMatch.Groups[2].Value);
                    var resChangeResult = ChangeResolution(width, height);
                    if (resChangeResult)
                    {
                        resolutionChanged = true;
                    }

                    break;
                }
            }
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return mainMenuitems;
        }

        private bool ChangeResolution(int width, int height)
        {
            logger.Debug($"Setting resolution to {width}x{height}...");
            var changeResult = DisplayHelper.ChangeResolution(width, height);
            if (changeResult == 0)
            {
                logger.Info("Resolution set");
                return true;
            }
            else
            {
                logger.Info("Failed to set resolution");
                return false;
            }
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            if (!resolutionChanged || !detectedResolutions.Any())
            {
                return;
            }

            //Due to issue #2634 this event is raised before the game IsRunning property
            //is reverted to false, so we need to verify that only this game is set to
            //running in all the database
            if (!IsAnyOtherGameRunning(args.Game))
            {
                logger.Info("Restoring default resolution...");
                var resChangeResult = ChangeResolution(detectedResolutions.First().Key, detectedResolutions.First().Value);
                if (resChangeResult)
                {
                    resolutionChanged = false;
                }
            }
            else
            {
                logger.Info("Games were detected as running so resolution was not changed");
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
                            SetResolutionFeature(resolution.Key, resolution.Value);
                            PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCResolutionChanger_DialogMessageDone"));
                        }
                    }
                );
            }
        }

        private void RemoveResolutionConfigurationSelected()
        {
            foreach (var game in PlayniteApi.MainView.SelectedGames)
            {
                if (game.FeatureIds == null)
                {
                    continue;
                }
                else
                {
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
        }

        private void SetResolutionFeature(int width, int height)
        {
            var featureName = $"[RC] {width}x{height}";
            var feature = PlayniteApi.Database.Features.Add(featureName);
            foreach (var game in PlayniteApi.MainView.SelectedGames)
            {
                if (game.FeatureIds == null)
                {
                    game.FeatureIds = new List<Guid>{feature.Id};
                    PlayniteApi.Database.Games.Update(game);
                }
                else
                {
                    var featuresRemoved = false;
                    var rcFeatures = game.Features.Where(x => x.Name != featureName && Regex.IsMatch(x.Name, @"^\[RC\] (\d+)x(\d+)$", RegexOptions.IgnoreCase));
                    if (rcFeatures != null && rcFeatures.Count() > 0)
                    {
                        foreach (var rcFeature in rcFeatures)
                        {
                            game.FeatureIds.Remove(rcFeature.Id);
                        }
                        featuresRemoved = true;
                    }

                    var featureAdded = game.FeatureIds.AddMissing(feature.Id);
                    if (featuresRemoved || featureAdded)
                    {
                        PlayniteApi.Database.Games.Update(game);
                    }
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