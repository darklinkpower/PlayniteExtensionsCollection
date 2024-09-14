using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using PlayniteUtilitiesCommon;
using PlayState.Enums;
using PlayState.Models;
using PlayState.Native;
using PlayState.XInputDotNetPure;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlayState
{
    public partial class PlayState
    {
        private const int HOTKEY_ID = 3754;

        private bool IsWindows10Or11()
        {
            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
            {
                var productName = key?.GetValue("ProductName")?.ToString() ?? string.Empty;
                return productName.Contains("Windows 10") || productName.Contains("Windows 11");
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Winuser.WM_HOTKEY)
            {
                if (wParam.ToInt32() == HOTKEY_ID)
                {
                    uint vkey = ((uint)lParam >> 16) & 0xFFFF;
                    if (vkey == (uint)KeyInterop.VirtualKeyFromKey(Settings.Settings.SuspendHotKey.Key))
                    {
                        SendSuspendSignal();
                    }
                    else if (vkey == (uint)KeyInterop.VirtualKeyFromKey(Settings.Settings.InformationHotkey.Key))
                    {
                        SendInformationSignal();
                    }
                    else if (vkey == (uint)KeyInterop.VirtualKeyFromKey(Settings.Settings.MinimizeMaximizeGameHotKey.Key))
                    {
                        SendMinimizeMaximizeSignal();
                    }

                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        public void RegisterHotkey()
        {
            if (!UnregisterHotKeyHandler())
            {
                return;
            }

            try
            {
                var informationHkRegisterSuccess = RegisterAndLogHotkey("Information", Settings.Settings.InformationHotkey);
                var suspendHkRegisterSuccess = RegisterAndLogHotkey("Pause/resume", Settings.Settings.SuspendHotKey);
                var minimizeHkRegisterSuccess = RegisterAndLogHotkey("Minimize/Maximize", Settings.Settings.MinimizeMaximizeGameHotKey);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to register hotkeys.");
            }
        }

        private bool RegisterAndLogHotkey(string hotkeyName, HotKey hotkey)
        {
            var registerSuccess = RegisterHotKey(hotkey);
            if (registerSuccess)
            {
                globalHotkeyRegistered = true;
                _logger.Debug($"{hotkeyName} Hotkey registered with hotkey {hotkey}.");
            }
            else
            {
                PlayniteApi.Notifications.Add(new NotificationMessage(Guid.NewGuid().ToString(),
                    $"PlayState: {string.Format(ResourceProvider.GetString("LOCPlayState_NotificationMessageHotkeyRegisterFailed"), hotkey)}",
                    NotificationType.Error));
                _logger.Error($"Failed to register configured {hotkeyName} Hotkey {hotkey}.");
            }

            return registerSuccess;
        }

        public bool RegisterHotKey(HotKey hotKey)
        {
            return User32.RegisterHotKey(mainWindowHandle, HOTKEY_ID, (uint)hotKey.Modifiers, (uint)KeyInterop.VirtualKeyFromKey(hotKey.Key));
        }

        public bool UnregisterHotKeyHandler()
        {
            if (!globalHotkeyRegistered)
            {
                return true;
            }

            var success = User32.UnregisterHotKey(mainWindowHandle, HOTKEY_ID);
            if (success)
            {
                globalHotkeyRegistered = false;
            }
            else
            {
                _logger.Error($"Failed to unregister hotkey handler");
            }

            return success;
        }

        private void SendCloseSignal()
        {
            HotKey testHotkey = new HotKey(Key.F4, ModifierKeys.Alt);
            Input.InputSender.SendHotkeyInput(testHotkey);
        }

        private void SendInformationSignal()
        {
            _playStateManager.ShowCurrentGameStatusNotification();
        }

        private void SendSuspendSignal()
        {
            _playStateManager.SwitchCurrentGameState();
        }

        private void SendMinimizeMaximizeSignal()
        {
            _playStateManager.SwitchMinimizeMaximizeCurrentGame();
        }

        private async void InvokeGameProcessesDetection(OnGameStartedEventArgs args)
        {
            var game = args.Game;
            _playStateManager.AddGameToDetection(game);
            var sourceActionHandled = await ScanGameSourceAction(args.Game, args.SourceAction);
            if (sourceActionHandled)
            {
                return;
            }

            var detectionDirectory = GetGameDetectionDirectory(args.Game, args.SourceAction);
            if (!detectionDirectory.IsNullOrEmpty())
            {
                var scanHandled = await ScanGameProcessesFromDirectoryAsync(game, detectionDirectory);
                if (scanHandled)
                {
                    return;
                }
            }

            _playStateManager.AddPlayStateData(game, new List<ProcessItem>());
        }

        private async Task<bool> ScanGameProcessesFromDirectoryAsync(Game game, string gameInstallDir)
        {
            var suspendPlaytimeOnly = Settings.Settings.GlobalSuspendMode == SuspendModes.Playtime && !PlayniteUtilities.GetGameHasFeature(game, featureSuspendProcesses, true) || PlayniteUtilities.GetGameHasFeature(game, featureSuspendPlaytime, true);

            // Fix for some games that take longer to start, even when already detected as running
            await Task.Delay(20000);
            if (!_playStateManager.IsGameBeingDetected(game))
            {
                _logger.Debug($"Detection Id was not detected. Execution of WMI Query task stopped.");
                return true;
            }

            var gameProcesses = ProcessesHandler.GetProcessesWmiQuery(true, gameInstallDir, Settings.Settings.ExecutablesScanExclusionList);
            if (gameProcesses.Count > 0 && gameProcesses.Any(x => x.Process.MainWindowHandle != IntPtr.Zero))
            {
                _logger.Debug($"Found {gameProcesses.Count} game processes in initial WMI query");
                _playStateManager.AddPlayStateData(game, gameProcesses);
                return true;
            }

            // No need to wait for the loop if we don't want to suspend processes
            if (suspendPlaytimeOnly)
            {
                _playStateManager.AddPlayStateData(game, new List<ProcessItem>());
                _playStateManager.AddGameToDetection(game);
            }

            // Waiting is useful for games that use a startup launcher, since
            // it can take some time before the user launches the game from it
            await Task.Delay(40000);
            var filterPaths = true;
            for (int i = 0; i < 7; i++)
            {
                // This is done to stop execution in case a new game was launched
                // or the launched game was closed
                if (!_playStateManager.IsGameBeingDetected(game))
                {
                    _logger.Debug($"Detection Id was not detected. Execution of WMI Query task stopped.");
                    return true;
                }

                // Try a few times with filters.
                // If nothing is found, try without filters. This helps in cases
                // where the active process is being filtered out by filters
                _logger.Debug($"Starting WMI loop number {i}");
                if (i == 4)
                {
                    _logger.Debug("FilterPaths set to false for WMI Query");
                    filterPaths = false;
                }

                gameProcesses = ProcessesHandler.GetProcessesWmiQuery(filterPaths, gameInstallDir, Settings.Settings.ExecutablesScanExclusionList);
                if (gameProcesses.Count > 0 && gameProcesses.Any(x => x.Process.MainWindowHandle != IntPtr.Zero))
                {
                    _logger.Debug($"Found {gameProcesses.Count} game processes");
                    _playStateManager.AddPlayStateData(game, gameProcesses);
                    return true;
                }
                else
                {
                    await Task.Delay(15000);
                }
            }

            _logger.Debug("Couldn't find any game process");
            if (suspendPlaytimeOnly)
            {
                return true;
            }
            return false;
        }

        private string GetGameDetectionDirectory(Game game, GameAction gameAction = null)
        {
            if (gameAction != null && !gameAction.Path.IsNullOrEmpty())
            {
                var expandedAction = PlayniteApi.ExpandGameVariables(game, gameAction);
                if (Path.IsPathRooted(expandedAction.Path))
                {
                    return Path.GetDirectoryName(expandedAction.Path);
                }
                else if (!expandedAction.WorkingDir.IsNullOrEmpty())
                {
                    var fullPath = Path.Combine(expandedAction.WorkingDir, expandedAction.Path);
                    if (FileSystem.FileExists(fullPath))
                    {
                        return fullPath;
                    }
                }
            }
            
            // Games from Xbox library are UWP apps. UWP apps run from the primary drive, e.g. "C:\".
            // If the game install location is not in the primary drive, Windows creates a symlink from the real files
            // to the primary drive and starts running the game from there. For this reason, we need to obtain the location
            // from where the game is running for Xbox game by detecting installed UWP apps as the Xbox plugin
            // reports the real installation directory in the games and not the fake one in C:\
            if (game.PluginId == Guid.Parse("7e4fbb5e-2ae3-48d4-8ba0-6b30e7a4e287"))
            {
                var gameInstallDir = Programs.GetUwpWorkdirFromGameId(game.GameId);
                if (gameInstallDir.IsNullOrEmpty() || !FileSystem.DirectoryExists(gameInstallDir))
                {
                    _playStateManager.RemoveGameFromDetection(game);
                    return null;
                }
                else
                {
                    return gameInstallDir;
                }
            }
            else if (!game.InstallDirectory.IsNullOrEmpty())
            {
                return game.InstallDirectory;
            }

            return null;
        }

        private async Task<bool> ScanGameSourceAction(Game game, GameAction sourceAction)
        {
            if (sourceAction is null || sourceAction.Type != GameActionType.Emulator || sourceAction.EmulatorProfileId.IsNullOrEmpty())
            {
                return false;
            }

            _logger.Debug("Source action is emulator.");
            var emulatorProfileId = sourceAction.EmulatorProfileId;
            var emulator = PlayniteApi.Database.Emulators[sourceAction.EmulatorId];
            if (emulatorProfileId.StartsWith("#builtin_"))
            {
                if (emulator.InstallDir.IsNullOrEmpty())
                {
                    return true;
                }

                await Task.Delay(15000);
                if (!_playStateManager.IsGameBeingDetected(game))
                {
                    _logger.Debug($"Detection Id was not detected. Execution of WMI Query task stopped.");
                    return true;
                }

                // Executable names for builtin profiles are not accesible so the only way is to scan
                // the directory for running executables
                var emuProcesses = ProcessesHandler.GetProcessesWmiQuery(false, emulator.InstallDir, Settings.Settings.ExecutablesScanExclusionList);
                if (emuProcesses.HasItems() && emuProcesses.Any(x => x.Process.MainWindowHandle != IntPtr.Zero))
                {
                    _logger.Debug($"Found {emuProcesses.Count} processes for BuiltIn emulator {emulator.Name}");
                    _playStateManager.AddPlayStateData(game, emuProcesses);
                    return true;
                }
                else
                {
                    _logger.Debug($"Failed to get processes for BuiltIn emulator {emulator.Name}");
                    return true;
                }
            }

            var profile = emulator?.CustomProfiles.FirstOrDefault(p => p.Id == emulatorProfileId);
            if (profile is null)
            {
                _logger.Debug($"Failed to get Custom emulator profile");
                return true;
            }

            _logger.Debug($"Custom emulator profile executable is {profile.Executable}");
            var gameProcesses = ProcessesHandler.GetProcessesWmiQuery(false, string.Empty, Settings.Settings.ExecutablesScanExclusionList, profile.Executable);
            if (gameProcesses.Count > 0 && gameProcesses.Any(x => x.Process.MainWindowHandle != IntPtr.Zero))
            {
                _playStateManager.AddPlayStateData(game, gameProcesses);
            }
            else
            {
                _logger.Debug($"Failed to get valid Custom emulator profile executables process items. Starting delay...");
                await Task.Delay(20000);
                _logger.Debug($"Delay finished");

                gameProcesses = ProcessesHandler.GetProcessesWmiQuery(false, string.Empty, Settings.Settings.ExecutablesScanExclusionList, profile.Executable);
                if (gameProcesses.Count > 0 && gameProcesses.Any(x => x.Process.MainWindowHandle != IntPtr.Zero))
                {
                    _playStateManager.AddPlayStateData(game, gameProcesses);
                }
            }

            return true;
        }
    }
}