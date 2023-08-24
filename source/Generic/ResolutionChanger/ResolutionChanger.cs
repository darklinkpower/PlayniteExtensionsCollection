using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using ResolutionChanger.Enums;
using ResolutionChanger.Models;
using ResolutionChanger.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

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

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            ApplyDisplayGameStartConfiguration(args);
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

        private void ApplyDisplayGameStartConfiguration(OnGameStartingEventArgs args)
        {
            var game = args.Game;
            if (settings.Settings.ChangeResOnlyGamesNotRunning && IsAnyOtherGameRunning(game))
            {
                logger.Debug("Another game was detected as running during game start");
                return;
            }

            GetDisplaySettingsNewValues(game, out int newWidth, out int newHeight, out int newRefreshRate, out string targetDisplayName);
            var changeDisplaySettings = (newWidth != 0 && newHeight != 0) && newRefreshRate != 0;
            var availableDisplays = DisplayUtilities.GetAvailableDisplayDevices();
            var currentPrimaryDisplayName = DisplayUtilities.GetPrimaryScreenName();
            if (targetDisplayName.IsNullOrEmpty())
            {
                targetDisplayName = currentPrimaryDisplayName; // If no specific display device has been specified, make changes to current primary display
            }

            var targetDisplay = availableDisplays.FirstOrDefault(x => x.DeviceName == targetDisplayName);
            var isTargetDisplayAvailable = !targetDisplay.DeviceName.IsNullOrEmpty();
            if (!isTargetDisplayAvailable)
            {
                logger.Debug($"Target display {targetDisplayName} is not attached to computer");
                return;
            }

            var setAsPrimaryDisplay = currentPrimaryDisplayName != targetDisplayName &&
                                      !targetDisplay.StateFlags.HasFlag(DisplayDeviceStateFlags.PrimaryDevice);
            if (changeDisplaySettings || setAsPrimaryDisplay)
            {
                // If the screen configuration was changed before, restore it before changing it again
                if (!RestoreDisplayData())
                {
                    return;
                }

                ApplyGameStartDisplayConfiguration(newWidth, newHeight, newRefreshRate, targetDisplayName, currentPrimaryDisplayName, setAsPrimaryDisplay);
            }
        }

        private void ApplyGameStartDisplayConfiguration(int newWidth, int newHeight, int newRefreshRate, string targetDisplayName, string currentPrimaryDisplayName, bool setAsPrimaryDisplay)
        {
            var targetDisplayCurrentDevMode = DisplayUtilities.GetScreenDevMode(targetDisplayName);
            var displayChangeSuccess = DisplayUtilities.ChangeDisplayConfiguration(targetDisplayName, newWidth, newHeight, newRefreshRate, setAsPrimaryDisplay);
            if (displayChangeSuccess)
            {
                var restoreResolution = newWidth != 0 && newHeight != 0;
                var restoreRefreshRate = newRefreshRate != 0;

                displayRestoreData = new DisplayConfigChangeData(targetDisplayCurrentDevMode, targetDisplayName, currentPrimaryDisplayName, restoreResolution, restoreRefreshRate);
                logger.Info($"Stored restore display data. Screen: {currentPrimaryDisplayName}. Resolution {displayRestoreData.DevMode.dmPelsWidth}x{displayRestoreData.DevMode.dmPelsHeight}. Frequency: {displayRestoreData.DevMode.dmDisplayFrequency}");
            }
        }

        private void GetDisplaySettingsNewValues(Game game, out int newWidth, out int newHeight, out int newRefreshRate, out string targetDisplayName)
        {
            newWidth = 0;
            newHeight = 0;
            newRefreshRate = 0;
            targetDisplayName = string.Empty;
            var modeDisplayInfo = PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop ?
                                  settings.Settings.DesktopModeDisplayInfo : settings.Settings.FullscreenModeDisplayInfo;
            if (modeDisplayInfo != null)
            {
                if (modeDisplayInfo.ChangeResolution && modeDisplayInfo.Width > 0 && modeDisplayInfo.Height > 0)
                {
                    newWidth = modeDisplayInfo.Width;
                    newHeight = modeDisplayInfo.Height;
                }

                if (modeDisplayInfo.ChangeRefreshRate && modeDisplayInfo.RefreshRate > 0)
                {
                    newRefreshRate = modeDisplayInfo.RefreshRate;
                }

                if (modeDisplayInfo.TargetSpecificDisplay && modeDisplayInfo.TargetDisplay != null)
                {
                    targetDisplayName = modeDisplayInfo.TargetDisplay.DeviceName;
                }
            }

            if (!game.Features.HasItems())
            {
                return;
            }

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

            RestoreDisplayData();
        }

        private bool RestoreDisplayData()
        {
            if (displayRestoreData is null)
            {
                return true;
            }

            var displayRestoreSuccess = DisplayUtilities.RestoreDisplayConfiguration(displayRestoreData);
            if (displayRestoreSuccess)
            {
                displayRestoreData = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void CreateGameMenuItems()
        {
            var devModes = DisplayUtilities.GetMainScreenAvailableDevModes()
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
                        Description = string.Format(ResourceProvider.GetString("LOCResolutionChanger_MenuItemDescriptionSetLaunchResolutionFeature"), resolution.Key, resolution.Value, DisplayUtilities.CalculateAspectRatioString(resolution.Key, resolution.Value)),
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