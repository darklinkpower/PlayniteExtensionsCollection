using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using DisplayHelper.Models;
using DisplayHelper.Native;
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
        // Menu item -> game feature it sets, used to show a check mark on the active choice.
        private readonly Dictionary<GameMenuItem, string> menuItemFeatures = new Dictionary<GameMenuItem, string>();
        private DisplayConfigChangeData displayRestoreData = null;
        private WindowMover windowMover = null;
        private const string windowModeBorderlessFeature = "[RC] Window mode: Move window (borderless)";
        private const string windowModeSwitchPrimaryFeature = "[RC] Window mode: Switch primary display";
        private const string windowModeFeatureRegex = @"^\[RC\] Window mode: .+";

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
            if (args.Games == null || args.Games.Count != 1)
            {
                return gameMenuItems;
            }

            var game = args.Games[0];
            var gameFeatures = new HashSet<string>(
                game.Features?.Where(x => !x.Name.IsNullOrEmpty()).Select(x => x.Name) ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);

            var items = new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = "Show current settings",
                    MenuSection = "Display Helper",
                    Icon = menuItemsMonitorIconName,
                    Action = a => ShowSettingsSummary(game)
                }
            };

            foreach (var item in gameMenuItems)
            {
                if (menuItemFeatures.TryGetValue(item, out var feature) && gameFeatures.Contains(feature))
                {
                    items.Add(new GameMenuItem
                    {
                        Description = "\u2713 " + item.Description,
                        MenuSection = item.MenuSection,
                        Icon = item.Icon,
                        Action = item.Action
                    });
                }
                else
                {
                    items.Add(item);
                }
            }

            return items;
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            ApplyDisplayGameStartConfiguration(args);
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            if (windowMover != null && args.StartedProcessId > 0)
            {
                windowMover.KnownProcessId = args.StartedProcessId;
            }
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            StopWindowMover();
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
            var moveGameWindow = setAsPrimaryDisplay && ShouldMoveGameWindow(game);
            if (moveGameWindow)
            {
                // Keep the current primary display and move the game window instead
                setAsPrimaryDisplay = false;
            }

            if (changeDisplaySettings || setAsPrimaryDisplay)
            {
                // If the screen configuration was changed before, restore it before changing it again
                if (!RestoreDisplayData())
                {
                    return;
                }

                ApplyGameStartDisplayConfiguration(newWidth, newHeight, newRefreshRate, targetDisplayName, currentPrimaryDisplayName, setAsPrimaryDisplay);
            }

            if (moveGameWindow)
            {
                StartWindowMover(game, targetDisplayName);
            }
        }

        private bool ShouldMoveGameWindow(Game game)
        {
            if (game.Features.HasItems())
            {
                if (game.Features.Any(x => string.Equals(x.Name, windowModeBorderlessFeature, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }

                if (game.Features.Any(x => string.Equals(x.Name, windowModeSwitchPrimaryFeature, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }
            }

            return settings.Settings.MoveWindowToTargetDisplay;
        }

        private void StartWindowMover(Game game, string targetDisplayName)
        {
            StopWindowMover();
            string installDirectory = null;
            try
            {
                if (!game.InstallDirectory.IsNullOrEmpty())
                {
                    installDirectory = PlayniteApi.ExpandGameVariables(game, game.InstallDirectory);
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to get game install directory");
            }

            windowMover = new WindowMover(installDirectory, targetDisplayName);
            windowMover.Start();
        }

        private void StopWindowMover()
        {
            windowMover?.Stop();
            windowMover = null;
        }

        private void ApplyGameStartDisplayConfiguration(int newWidth, int newHeight, int newRefreshRate, string targetDisplayName, string currentPrimaryDisplayName, bool setAsPrimaryDisplay)
        {
            var targetDisplayCurrentDevMode = DisplayUtilities.GetScreenDevMode(targetDisplayName);
            var displayChangeSuccess = DisplayUtilities.ChangeDisplayConfiguration(targetDisplayName, newWidth, newHeight, newRefreshRate, setAsPrimaryDisplay);
            if (displayChangeSuccess)
            {
                var restoreResolution = newWidth != 0 && newHeight != 0;
                var restoreRefreshRate = newRefreshRate != 0;

                var primaryToRestore = setAsPrimaryDisplay ? currentPrimaryDisplayName : targetDisplayName;
                displayRestoreData = new DisplayConfigChangeData(targetDisplayCurrentDevMode, targetDisplayName, primaryToRestore, restoreResolution, restoreRefreshRate);
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

        public void RefreshGameMenuItems()
        {
            CreateGameMenuItems();
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

            menuItemFeatures.Clear();
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
                AddFeatureMenuItem($"[RC] {resolution.Value.Key}x{resolution.Value.Value}",
                    new GameMenuItem
                    {
                        Description = string.Format("{0}x{1} ({2})", resolution.Value.Key, resolution.Value.Value, resolution.Key),
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
                AddFeatureMenuItem($"[RC] {displayFrequency}Hz",
                    new GameMenuItem
                    {
                        Description = string.Format("{0}Hz", displayFrequency),
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

            var windowModeSection = "Window mode";
            AddFeatureMenuItem(windowModeBorderlessFeature, CreateWindowModeMenuItem("Move window to chosen display (borderless)", windowModeBorderlessFeature, $"{menuSection}|{windowModeSection}"));
            AddFeatureMenuItem(windowModeSwitchPrimaryFeature, CreateWindowModeMenuItem("Switch primary display", windowModeSwitchPrimaryFeature, $"{menuSection}|{windowModeSection}"));
            gameMenuItems.Add(new GameMenuItem
            {
                Description = "Use global setting",
                MenuSection = $"{menuSection}|{windowModeSection}",
                Icon = menuItemsDeleteIconName,
                Action = a =>
                {
                    RemoveGamesRegexMatchingFeatures(windowModeFeatureRegex, a.Games);
                    PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCResolutionChanger_DialogMessageDone"));
                }
            });

            gameMenuItems.Add(new GameMenuItem
            {
                Description = "Clear display (use global setting)",
                MenuSection = $"{menuSection}|{displaySection}",
                Icon = menuItemsDeleteIconName,
                Action = a =>
                {
                    RemoveGamesRegexMatchingFeatures(@"^\[RC\] Display: .+", a.Games);
                    PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCResolutionChanger_DialogMessageDone"));
                }
            });

            var monitorFriendlyNames = MonitorNames.GetMonitorFriendlyNames();
            foreach (var displayDevice in DisplayUtilities.GetAvailableDisplayDevices())
            {
                AddFeatureMenuItem($"[RC] Display: {displayDevice.DeviceName}",
                    new GameMenuItem
                    {
                        Description = MonitorNames.GetLabel(displayDevice.DeviceName, displayDevice.DeviceString, monitorFriendlyNames),
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

        private void AddFeatureMenuItem(string featureName, GameMenuItem item)
        {
            gameMenuItems.Add(item);
            menuItemFeatures[item] = featureName;
        }

        private void ShowSettingsSummary(Game game)
        {
            int? gameWidth = null, gameHeight = null, gameRefresh = null;
            string gameDisplay = null;
            bool? gameMoveWindow = null;
            if (game.Features.HasItems())
            {
                foreach (var feature in game.Features.Where(x => !x.Name.IsNullOrEmpty()))
                {
                    var res = Regex.Match(feature.Name, @"^\[RC\] (\d+)x(\d+)$", RegexOptions.IgnoreCase);
                    var hz = Regex.Match(feature.Name, @"^\[RC\] (\d+)Hz$", RegexOptions.IgnoreCase);
                    var disp = Regex.Match(feature.Name, @"^\[RC\] Display: (.+)", RegexOptions.IgnoreCase);
                    if (res.Success && gameWidth == null)
                    {
                        gameWidth = int.Parse(res.Groups[1].Value);
                        gameHeight = int.Parse(res.Groups[2].Value);
                    }
                    else if (hz.Success && gameRefresh == null)
                    {
                        gameRefresh = int.Parse(hz.Groups[1].Value);
                    }
                    else if (disp.Success && gameDisplay == null)
                    {
                        gameDisplay = disp.Groups[1].Value;
                    }
                    else if (string.Equals(feature.Name, windowModeBorderlessFeature, StringComparison.OrdinalIgnoreCase))
                    {
                        gameMoveWindow = true;
                    }
                    else if (string.Equals(feature.Name, windowModeSwitchPrimaryFeature, StringComparison.OrdinalIgnoreCase))
                    {
                        gameMoveWindow = false;
                    }
                }
            }

            var isDesktop = PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop;
            var global = isDesktop ? settings.Settings.DesktopModeDisplayInfo : settings.Settings.FullscreenModeDisplayInfo;
            var modeName = isDesktop ? "Desktop mode" : "Fullscreen mode";
            const string fromGame = "set for this game";
            var fromGlobal = $"global setting, {modeName}";

            var devices = DisplayUtilities.GetAvailableDisplayDevices();
            var friendlyNames = MonitorNames.GetMonitorFriendlyNames();
            var primaryName = DisplayUtilities.GetPrimaryScreenName(devices);
            string Label(string deviceName)
            {
                var device = devices.FirstOrDefault(x => x.DeviceName == deviceName);
                return device.DeviceName.IsNullOrEmpty()
                    ? $"{deviceName} (NOT CONNECTED)"
                    : MonitorNames.GetLabel(device.DeviceName, device.DeviceString, friendlyNames);
            }

            string targetDisplay = null, displaySource = null;
            if (!gameDisplay.IsNullOrEmpty())
            {
                targetDisplay = gameDisplay;
                displaySource = fromGame;
            }
            else if (global != null && global.TargetSpecificDisplay && !global.TargetDisplayName.IsNullOrEmpty())
            {
                targetDisplay = global.TargetDisplayName;
                displaySource = fromGlobal;
            }

            string resolution = null, resolutionSource = null;
            if (gameWidth.HasValue)
            {
                resolution = $"{gameWidth}x{gameHeight}";
                resolutionSource = fromGame;
            }
            else if (global != null && global.ChangeResolution && global.Width.HasValue && global.Height.HasValue)
            {
                resolution = $"{global.Width}x{global.Height}";
                resolutionSource = fromGlobal;
            }

            string refresh = null, refreshSource = null;
            if (gameRefresh.HasValue)
            {
                refresh = $"{gameRefresh}Hz";
                refreshSource = fromGame;
            }
            else if (global != null && global.ChangeRefreshRate && global.RefreshRate.HasValue)
            {
                refresh = $"{global.RefreshRate}Hz";
                refreshSource = fromGlobal;
            }

            var moveWindow = gameMoveWindow ?? settings.Settings.MoveWindowToTargetDisplay;
            var windowModeSource = gameMoveWindow.HasValue ? fromGame : "global setting";

            SummaryRow Row(string label, string value, string source, bool warning = false)
            {
                var kind = source == null ? SummarySource.None : (source == fromGame ? SummarySource.Game : SummarySource.Global);
                return new SummaryRow
                {
                    Label = label,
                    Value = value,
                    Source = kind,
                    SourceText = kind == SummarySource.None ? "Default" : (kind == SummarySource.Game ? "This game" : "Global"),
                    IsWarning = warning
                };
            }

            var targetConnected = targetDisplay != null && devices.Any(x => x.DeviceName == targetDisplay);
            var rows = new List<SummaryRow>
            {
                Row("Display", targetDisplay == null ? "Not set (primary display)" : Label(targetDisplay), displaySource, targetDisplay != null && !targetConnected),
                Row("Window mode", moveWindow ? "Move window (borderless)" : "Switch primary display", gameMoveWindow.HasValue ? fromGame : fromGlobal),
                Row("Resolution", resolution ?? "Not changed", resolutionSource),
                Row("Refresh rate", refresh ?? "Not changed", refreshSource),
                new SummaryRow { Label = "Primary display now", Value = Label(primaryName), Source = SummarySource.Global, SourceText = "Windows" }
            };

            var notes = new List<SummaryNote>();
            var effectiveTarget = targetDisplay == null ? primaryName : targetDisplay;
            if (targetDisplay != null && !targetConnected)
            {
                notes.Add(new SummaryNote(SummaryNoteKind.Warning, "The chosen display is not connected, so Display Helper will do nothing."));
            }
            else
            {
                if (effectiveTarget == primaryName)
                {
                    notes.Add(new SummaryNote(SummaryNoteKind.Action, "The game opens on the primary display as usual."));
                }
                else if (moveWindow)
                {
                    notes.Add(new SummaryNote(SummaryNoteKind.Action, $"The game window is moved to {Label(effectiveTarget)} as a borderless window."));
                    notes.Add(new SummaryNote(SummaryNoteKind.Info, "Primary display, taskbar and desktop icons stay as they are."));
                    notes.Add(new SummaryNote(SummaryNoteKind.Warning, "Set the game to windowed or borderless in its own video options. Exclusive fullscreen can't be moved."));
                }
                else
                {
                    notes.Add(new SummaryNote(SummaryNoteKind.Action, $"{Label(effectiveTarget)} becomes the primary display while the game runs, then it's restored."));
                }

                if (resolution != null || refresh != null)
                {
                    var changes = string.Join(" @ ", new[] { resolution, refresh }.Where(x => x != null));
                    notes.Add(new SummaryNote(SummaryNoteKind.Action, $"{Label(effectiveTarget)} is set to {changes} while the game runs, then it's restored."));
                }

                if (settings.Settings.ChangeResOnlyGamesNotRunning)
                {
                    notes.Add(new SummaryNote(SummaryNoteKind.Info, "Nothing is changed if another game is already running."));
                }
            }

            SettingsSummaryWindow.Show(PlayniteApi, game.Name, rows, notes);
        }

        private GameMenuItem CreateWindowModeMenuItem(string description, string featureName, string section)
        {
            return new GameMenuItem
            {
                Description = description,
                MenuSection = section,
                Icon = menuItemsMonitorIconName,
                Action = a =>
                {
                    RemoveGamesRegexMatchingFeatures(windowModeFeatureRegex, a.Games);
                    PlayniteUtilities.AddFeatureToGames(PlayniteApi, a.Games, featureName);
                    PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCResolutionChanger_DialogMessageDone"));
                }
            };
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