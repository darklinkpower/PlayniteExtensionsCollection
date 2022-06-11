using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using ResolutionChanger.Models;
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
        private List<MainMenuItem> mainMenuitems;
        private DisplayConfigChangeData displayRestoreData = null;

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
            if (!game.Features.HasItems())
            {
                return;
            }

            if (settings.Settings.ChangeResOnlyGamesNotRunning && IsAnyOtherGameRunning(game))
            {
                logger.Debug("Another game was detected as running during game start");
                return;
            }

            var width = 0;
            var height = 0;
            var refreshRate = 0;    
            foreach (var feature in game.Features)
            {
                if (width == 0 && height == 0)
                {
                    var resMatch = Regex.Match(feature.Name, @"^\[RC\] (\d+)x(\d+)$", RegexOptions.IgnoreCase);
                    if (resMatch.Success)
                    {
                        width = int.Parse(resMatch.Groups[1].Value);
                        height = int.Parse(resMatch.Groups[2].Value);
                        continue;
                    }
                }

                if (refreshRate == 0)
                {
                    var refreshMatch = Regex.Match(feature.Name, @"^\[RC\] (\d+)Hz$", RegexOptions.IgnoreCase);
                    if (refreshMatch.Success)
                    {
                        refreshRate = int.Parse(refreshMatch.Groups[1].Value);
                    }
                }
            }

            if (width == 0 && height == 0 && refreshRate == 0)
            {
                return;
            }

            // If the screen configuration was changed before, restore it before changing it again
            if (displayRestoreData != null)
            {
                var restoreSuccess = DisplayHelper.RestoreDisplayConfiguration(displayRestoreData);
                if (restoreSuccess)
                {
                    displayRestoreData = null;
                }
                else
                {
                    // Don't continue if restore failed
                    return;
                }
            }

            var currentDevMode = DisplayHelper.GetMainScreenDevMode();
            var changeResolution = width != 0 && height != 0;
            var changeRefreshRate = refreshRate != 0;
            var displayChanged = DisplayHelper.ChangeDisplayConfiguration(currentDevMode, width, height, refreshRate);
            if (displayChanged)
            {
                displayRestoreData = new DisplayConfigChangeData(currentDevMode, changeResolution, changeRefreshRate);
                logger.Info($"Stored DevMode. Device: {displayRestoreData.DevMode.dmDeviceName}. Resolution {displayRestoreData.DevMode.dmPelsWidth}x{displayRestoreData.DevMode.dmPelsHeight}. Frequency: {displayRestoreData.DevMode.dmDisplayFrequency}");
            }
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return mainMenuitems;
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            if (displayRestoreData == null)
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

            if (displayRestoreData == null)
            {
                return;
            }

            logger.Info($"Restoring previous display configuration {displayRestoreData.DevMode.dmPelsWidth}x{displayRestoreData.DevMode.dmPelsHeight}, refresh rate {displayRestoreData.DevMode.dmDisplayFrequency}");
            var restoreSuccess = DisplayHelper.RestoreDisplayConfiguration(displayRestoreData);
            if (restoreSuccess)
            {
                displayRestoreData = null;
            }
            else
            {
                logger.Info($"Failed to restore display configuration");
            }
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            var devModes = DisplayHelper.GetMainScreenAvailableDevModes()
                .Distinct()
                .OrderByDescending(dm => dm.dmPelsWidth)
                .ThenByDescending(dm => dm.dmPelsHeight)
                .ThenByDescending(dm => dm.dmDisplayFrequency)
                .ToList();

            var resolutionSection = ResourceProvider.GetString("LOCResolutionChanger_MenuSectionDisplayResolution");
            var frequencySection = ResourceProvider.GetString("LOCResolutionChanger_MenuSectionDisplayFrequency");

            mainMenuitems = new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCResolutionChanger_MenuItemDescriptionClearResolutionFeature"),
                    MenuSection = $"@Resolution Changer|{resolutionSection}",
                    Action = a =>
                    {
                        RemoveResolutionConfigurationSelected(@"^\[RC\] (\d+)x(\d+)$");
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCResolutionChanger_DialogMessageDone"));
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCResolutionChanger_MenuItemDescriptionClearFrequencyFeature"),
                    MenuSection = $"@Resolution Changer|{frequencySection}",
                    Action = a =>
                    {
                        RemoveResolutionConfigurationSelected(@"^\[RC\] (\d+)Hz$");
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCResolutionChanger_DialogMessageDone"));
                    }
                }
            };

            var resolutions = GetAvailableResolutionsFromDevModes(devModes);
            foreach (var resolution in resolutions)
            {
                mainMenuitems.Add(
                    new MainMenuItem
                    {
                        Description = string.Format(ResourceProvider.GetString("LOCResolutionChanger_MenuItemDescriptionSetLaunchResolutionFeature"), resolution.Key, resolution.Value, DisplayHelper.GetResolutionAspectRatio(resolution.Key, resolution.Value)),
                        MenuSection = $"@Resolution Changer|{resolutionSection}",
                        Action = a =>
                        {
                            PlayniteUtilities.AddFeatureToGames(PlayniteApi, PlayniteApi.MainView.SelectedGames.Distinct(), $"[RC] {resolution.Key}x{resolution.Value}");
                            PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCResolutionChanger_DialogMessageDone"));
                        }
                    }
                );
            }

            foreach (var displayFrequency in devModes.Select(dm => dm.dmDisplayFrequency).Distinct())
            {
                mainMenuitems.Add(
                    new MainMenuItem
                    {
                        Description = string.Format(ResourceProvider.GetString("LOCResolutionChanger_MenuItemDescriptionSetLaunchFrequencyFeature"), displayFrequency),
                        MenuSection = $"@Resolution Changer|{frequencySection}",
                        Action = a =>
                        {
                            PlayniteUtilities.AddFeatureToGames(PlayniteApi, PlayniteApi.MainView.SelectedGames.Distinct(), $"[RC] {displayFrequency}Hz");
                            PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCResolutionChanger_DialogMessageDone"));
                        }
                    }
                );
            }
        }

        private List<KeyValuePair<int, int>> GetAvailableResolutionsFromDevModes(List<DEVMODE> dms)
        {
            var list = new List<KeyValuePair<int, int>>();
            foreach (var dm in dms)
            {
                list.Add(new KeyValuePair<int, int>(dm.dmPelsWidth, dm.dmPelsHeight));
            }

            return list.Distinct().ToList();
        }

        private void RemoveResolutionConfigurationSelected(string regexDef)
        {
            using (PlayniteApi.Database.BufferedUpdate())
            foreach (var game in PlayniteApi.MainView.SelectedGames.Distinct())
            {
                if (!game.FeatureIds.HasItems())
                {
                    continue;
                }

                var rcFeatures = game.Features.Where(x => Regex.IsMatch(x.Name, regexDef, RegexOptions.IgnoreCase));
                if (rcFeatures != null)
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