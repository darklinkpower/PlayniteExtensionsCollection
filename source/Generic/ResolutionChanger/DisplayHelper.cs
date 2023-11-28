using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using DisplayHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using static WinApi.Flags;

namespace DisplayHelper
{
    public class DisplayHelper : GenericPlugin
    {
        private const string menuItemsMonitorIconName = "rcMonitorIconResource";
        private const string menuItemsDeleteIconName = "rcDeleteIcon";
        private static readonly ILogger logger = LogManager.GetLogger();
        private List<GameMenuItem> gameMenuItems = new List<GameMenuItem>();
        private DisplayConfigChangeData displayRestoreData = null;

        private DisplayHelperSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("32b6a5c7-be17-4852-b4f7-f059a7321f4c");

        public DisplayHelper(IPlayniteAPI api) : base(api)
        {
            settings = new DisplayHelperSettingsViewModel(this);
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
            return new DisplayHelperSettingsView();
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
            var changeDisplaySettings = (newWidth != 0 && newHeight != 0) || newRefreshRate != 0;
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

            if (game.Features.HasItems())
            {
                foreach (var feature in game.Features)
                {
                    if (feature.Name.IsNullOrEmpty())
                    {
                        continue;
                    }

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

                    if (targetDisplayName.IsNullOrEmpty())
                    {
                        var displayMatch = Regex.Match(feature.Name, @"^\[RC\] Display: (.+)", RegexOptions.IgnoreCase);
                        if (displayMatch.Success)
                        {
                            targetDisplayName = displayMatch.Groups[1].Value;
                            continue;
                        }
                    }
                }
            }

            var modeDisplayInfo = PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop ?
                      settings.Settings.DesktopModeDisplayInfo : settings.Settings.FullscreenModeDisplayInfo;
            if (modeDisplayInfo is null)
            {
                return;
            }

            if (newWidth == 0 && newHeight == 0
                && modeDisplayInfo.ChangeResolution && modeDisplayInfo.Width.HasValue && modeDisplayInfo.Height.HasValue)
            {
                newWidth = modeDisplayInfo.Width.Value;
                newHeight = modeDisplayInfo.Height.Value;
            }

            if (newRefreshRate == 0
                && modeDisplayInfo.ChangeRefreshRate && modeDisplayInfo.RefreshRate.HasValue)
            {
                newRefreshRate = modeDisplayInfo.RefreshRate.Value;
            }

            if (targetDisplayName.IsNullOrEmpty()
                && modeDisplayInfo.TargetSpecificDisplay && !modeDisplayInfo.TargetDisplayName.IsNullOrEmpty())
            {
                targetDisplayName = modeDisplayInfo.TargetDisplayName;
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
            var mainDisplayInfo = DisplayUtilities.GetPrimaryDisplayDeviceInfo();
            if (mainDisplayInfo is null)
            {
                return;
            }

            var menuSection = "Display Helper";
            var resolutionSection = ResourceProvider.GetString("LOCResolutionChanger_MenuSectionDisplayResolution");
            var refreshRateSection = ResourceProvider.GetString("LOCDisplayHelper_MenuSectionDisplayRefreshRate");
            var displaySection = ResourceProvider.GetString("LOCDisplayHelper_DisplayLabel").TrimEnd(':');

            gameMenuItems = new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCDisplayHelper_MenuItemDescriptionClearResolutionFeature"),
                    MenuSection = $"{menuSection}|{resolutionSection}",
                    Icon = menuItemsDeleteIconName,
                    Action = a =>
                    {
                        RemoveGamesRegexMatchingFeatures(@"^\[RC\] (\d+)x(\d+)$", a.Games);
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCResolutionChanger_DialogMessageDone"));
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCDisplayHelper_MenuItemDescriptionClearFrequencyFeature"),
                    MenuSection = $"{menuSection}|{refreshRateSection}",
                    Icon = menuItemsDeleteIconName,
                    Action = a =>
                    {
                        RemoveGamesRegexMatchingFeatures(@"^\[RC\] (\d+)Hz$", a.Games);
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCResolutionChanger_DialogMessageDone"));
                    }
                }
            };

            var resolutions = GetUniqueResolutionsFromDisplayInfo(mainDisplayInfo);
            foreach (var resolution in resolutions)
            {
                gameMenuItems.Add(
                    new GameMenuItem
                    {
                        Description = string.Format(ResourceProvider.GetString("LOCDisplayHelper_MenuItemDescriptionSetLaunchResolutionFeature"), resolution.Value.Key, resolution.Value.Value, resolution.Key),
                        MenuSection = $"{menuSection}|{resolutionSection}",
                        Icon = menuItemsMonitorIconName,
                        Action = a =>
                        {
                            RemoveGamesRegexMatchingFeatures(@"^\[RC\] (\d+)x(\d+)$", a.Games);
                            PlayniteUtilities.AddFeatureToGames(PlayniteApi, a.Games, $"[RC] {resolution.Value.Key}x{resolution.Value.Value}");
                            PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCResolutionChanger_DialogMessageDone"));
                        }
                    }
                );
            }

            foreach (var displayFrequency in mainDisplayInfo.DisplayModes.Select(x => x.DisplayFrenquency).Distinct())
            {
                gameMenuItems.Add(
                    new GameMenuItem
                    {
                        Description = string.Format(ResourceProvider.GetString("LOCDisplayHelper_MenuItemDescriptionSetLaunchRefreshRateFeature"), displayFrequency),
                        MenuSection = $"{menuSection}|{refreshRateSection}",
                        Icon = menuItemsMonitorIconName,
                        Action = a =>
                        {
                            RemoveGamesRegexMatchingFeatures(@"^\[RC\] (\d+)Hz$", a.Games);
                            PlayniteUtilities.AddFeatureToGames(PlayniteApi, a.Games, $"[RC] {displayFrequency}Hz");
                            PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCResolutionChanger_DialogMessageDone"));
                        }
                    }
                );
            }

            foreach (var displayDevice in DisplayUtilities.GetAvailableDisplayDevices())
            {
                gameMenuItems.Add(
                    new GameMenuItem
                    {
                        Description = $"{displayDevice.DeviceName} ({displayDevice.DeviceString})",
                        MenuSection = $"{menuSection}|{displaySection}",
                        Icon = menuItemsMonitorIconName,
                        Action = a =>
                        {
                            RemoveGamesRegexMatchingFeatures(@"^\[RC\] Display: .+", a.Games);
                            PlayniteUtilities.AddFeatureToGames(PlayniteApi, a.Games, $"[RC] Display: {displayDevice.DeviceName}");
                            PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCResolutionChanger_DialogMessageDone"));
                        }
                    }
                );
            }
        }

        private IEnumerable<KeyValuePair<string, KeyValuePair<int, int>>> GetUniqueResolutionsFromDisplayInfo(DisplayInfo displayInfo)
        {
            return displayInfo.DisplayModes
                .Select(x => new KeyValuePair<string, KeyValuePair<int, int>>(x.AspectRatio, new KeyValuePair<int, int>(x.Width, x.Height)))
                .Distinct();
        }

        private void RemoveGamesRegexMatchingFeatures(string regexDef, List<Game> games)
        {
            using (PlayniteApi.Database.BufferedUpdate())
            {
                foreach (var game in games)
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