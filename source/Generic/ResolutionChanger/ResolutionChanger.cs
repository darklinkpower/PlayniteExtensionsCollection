using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using ResolutionChanger.Enums;
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
        private const string menuItemsMonitorIconName = "rcMonitorIconResource";
        private const string menuItemsDeleteIconName = "rcDeleteIcon";
        private static readonly ILogger logger = LogManager.GetLogger();
        private List<GameMenuItem> gameMenuItems = new List<GameMenuItem>();
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

            PlayniteUtilities.AddTextIcoFontResource(menuItemsMonitorIconName, "\xEA48");
            PlayniteUtilities.AddTextIcoFontResource(menuItemsDeleteIconName, "\xEF00");
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            CreateGameMenuItems();
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            return gameMenuItems;
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            ApplyDisplayConfiguration(args);
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            RestoreDisplayData(args);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new ResolutionChangerSettingsView();
        }

        private bool IsAnyOtherGameRunning(Game game)
        {
            return PlayniteApi.Database.Games.Any(x => x.IsRunning && x.Id != game.Id);
        }

        private void ApplyDisplayConfiguration(OnGameStartedEventArgs args)
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

            var newWidth = 0;
            var newHeight = 0;
            var newRefreshRate = 0;
            foreach (var feature in game.Features)
            {
                if (newWidth == 0 && newHeight == 0)
                {
                    var resMatch = Regex.Match(feature.Name, @"^\[RC\] (\d+)x(\d+)$", RegexOptions.IgnoreCase);
                    if (resMatch.Success)
                    {
                        newWidth = int.Parse(resMatch.Groups[1].Value);
                        newHeight = int.Parse(resMatch.Groups[2].Value);
                        continue;
                    }
                }

                if (newRefreshRate == 0)
                {
                    var refreshMatch = Regex.Match(feature.Name, @"^\[RC\] (\d+)Hz$", RegexOptions.IgnoreCase);
                    if (refreshMatch.Success)
                    {
                        newRefreshRate = int.Parse(refreshMatch.Groups[1].Value);
                        continue;
                    }
                }
            }

            if (newWidth == 0 && newHeight == 0 && newRefreshRate == 0)
            {
                return;
            }

            // If the screen configuration was changed before, restore it before changing it again
            if (displayRestoreData != null)
            {
                var displayRestoreResult = DisplayHelper.RestoreDisplayConfiguration(displayRestoreData);
                if (displayRestoreResult == ResolutionChangeResult.Success)
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
            var mainScreenName = DisplayHelper.GetMainScreenName();
            var displayChangeResult = DisplayHelper.ChangeDisplayConfiguration(mainScreenName, newWidth, newHeight, newRefreshRate);
            if (displayChangeResult == ResolutionChangeResult.Success)
            {
                var changeResolution = newWidth != 0 && newHeight != 0;
                var changeRefreshRate = newRefreshRate != 0;
                displayRestoreData = new DisplayConfigChangeData(currentDevMode, mainScreenName, changeResolution, changeRefreshRate);
                logger.Info($"Stored DevMode. Device: {displayRestoreData.DevMode.dmDeviceName}. Resolution {displayRestoreData.DevMode.dmPelsWidth}x{displayRestoreData.DevMode.dmPelsHeight}. Frequency: {displayRestoreData.DevMode.dmDisplayFrequency}");
            }
        }

        private void RestoreDisplayData(OnGameStoppedEventArgs args)
        {
            if (displayRestoreData is null)
            {
                return;
            }

            //Due to issue #2634 this event is raised before the game IsRunning property
            //is reverted to false, so we need to verify that only this game is set to
            //running in all the database
            if (settings.Settings.ChangeResOnlyGamesNotRunning && IsAnyOtherGameRunning(args.Game))
            {
                logger.Debug("Another game was detected as running during OnGameStopped");
                return;
            }

            if (displayRestoreData.ResolutionChanged || displayRestoreData.RefreshRateChanged)
            {
                logger.Info($"Restoring previous display for screen: {displayRestoreData.DevMode.dmDeviceName}, configuration: {displayRestoreData.DevMode.dmPelsWidth}x{displayRestoreData.DevMode.dmPelsHeight} {displayRestoreData.DevMode.dmDisplayFrequency}hz");
                var restoreSuccess = DisplayHelper.RestoreDisplayConfiguration(displayRestoreData);
                if (restoreSuccess == ResolutionChangeResult.Success)
                {
                    displayRestoreData = null;
                }
                else
                {
                    logger.Info($"Failed to restore display configuration");
                }
            }
        }

        private void CreateGameMenuItems()
        {
            var devModes = DisplayHelper.GetMainScreenAvailableDevModes()
                .Distinct()
                .OrderByDescending(dm => dm.dmPelsWidth)
                .ThenByDescending(dm => dm.dmPelsHeight)
                .ThenByDescending(dm => dm.dmDisplayFrequency)
                .ToList();

            var resolutionSection = ResourceProvider.GetString("LOCResolutionChanger_MenuSectionDisplayResolution");
            var frequencySection = ResourceProvider.GetString("LOCResolutionChanger_MenuSectionDisplayFrequency");

            gameMenuItems = new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCResolutionChanger_MenuItemDescriptionClearResolutionFeature"),
                    MenuSection = $"Resolution Changer|{resolutionSection}",
                    Icon = menuItemsDeleteIconName,
                    Action = a =>
                    {
                        RemoveResolutionConfigurationSelected(@"^\[RC\] (\d+)x(\d+)$");
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCResolutionChanger_DialogMessageDone"));
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCResolutionChanger_MenuItemDescriptionClearFrequencyFeature"),
                    MenuSection = $"Resolution Changer|{frequencySection}",
                    Icon = menuItemsDeleteIconName,
                    Action = a =>
                    {
                        RemoveResolutionConfigurationSelected(@"^\[RC\] (\d+)Hz$");
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCResolutionChanger_DialogMessageDone"));
                    }
                }
            };

            var resolutions = GetUniqueResolutionsFromDevModes(devModes);
            foreach (var resolution in resolutions)
            {
                gameMenuItems.Add(
                    new GameMenuItem
                    {
                        Description = string.Format(ResourceProvider.GetString("LOCResolutionChanger_MenuItemDescriptionSetLaunchResolutionFeature"), resolution.Key, resolution.Value, DisplayHelper.CalculateAspectRatioString(resolution.Key, resolution.Value)),
                        MenuSection = $"Resolution Changer|{resolutionSection}",
                        Icon = menuItemsMonitorIconName,
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
                gameMenuItems.Add(
                    new GameMenuItem
                    {
                        Description = string.Format(ResourceProvider.GetString("LOCResolutionChanger_MenuItemDescriptionSetLaunchFrequencyFeature"), displayFrequency),
                        MenuSection = $"Resolution Changer|{frequencySection}",
                        Icon = menuItemsMonitorIconName,
                        Action = a =>
                        {
                            PlayniteUtilities.AddFeatureToGames(PlayniteApi, PlayniteApi.MainView.SelectedGames.Distinct(), $"[RC] {displayFrequency}Hz");
                            PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCResolutionChanger_DialogMessageDone"));
                        }
                    }
                );
            }
        }

        private IEnumerable<KeyValuePair<int, int>> GetUniqueResolutionsFromDevModes(List<DEVMODE> devModes)
        {
            return devModes
                .Select(devMode => new KeyValuePair<int, int>(devMode.dmPelsWidth, devMode.dmPelsHeight))
                .Distinct();
        }

        private void RemoveResolutionConfigurationSelected(string regexDef)
        {
            using (PlayniteApi.Database.BufferedUpdate())
            {
                foreach (var game in PlayniteApi.MainView.SelectedGames.Distinct())
                {
                    if (!game.FeatureIds.HasItems())
                    {
                        continue;
                    }

                    var extensionFeatures = game.Features.Where(x => Regex.IsMatch(x.Name, regexDef, RegexOptions.IgnoreCase));
                    if (extensionFeatures != null)
                    {
                        foreach (var rcFeature in extensionFeatures)
                        {
                            game.FeatureIds.Remove(rcFeature.Id);
                        }

                        PlayniteApi.Database.Games.Update(game);
                    }
                }
            }
        }

    }
}