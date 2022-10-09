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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlayState
{
    public partial class PlayState
    {
        private const int HOTKEY_ID = 3754;

        private void CheckControllers()
        {
            var maxCheckIndex = Settings.Settings.GamePadHotkeysEnableAllControllers ? 3 : 0;
            var anySignalSent = false;
            for (int i = 0; i <= maxCheckIndex; i++)
            {
                PlayerIndex playerIndex = (PlayerIndex)i;
                GamePadState gamePadState = GamePad.GetState(playerIndex);
                if (gamePadState.IsConnected && (gamePadState.Buttons.IsAnyPressed() || gamePadState.DPad.IsAnyPressed()))
                {
                    if (isAnyGameRunning)
                    {
                        if (Settings.Settings.GamePadInformationHotkeyEnable && Settings.Settings.GamePadInformationHotkey?.IsGamePadStateEqual(gamePadState) == true)
                        {
                            SendInformationSignal();
                            anySignalSent = true;
                        }
                        else if (Settings.Settings.GamePadSuspendHotkeyEnable && Settings.Settings.GamePadSuspendHotkey?.IsGamePadStateEqual(gamePadState) == true)
                        {
                            SendSuspendSignal();
                            anySignalSent = true;
                        }
                        else
                        {
                            foreach (var comboHotkey in Settings.Settings.GamePadToHotkeyCollection)
                            {
                                if (comboHotkey.Mode == GamePadToKeyboardHotkeyModes.Disabled ||
                                   (comboHotkey.Mode != GamePadToKeyboardHotkeyModes.Always &&
                                    comboHotkey.Mode != GamePadToKeyboardHotkeyModes.OnGameRunning))
                                {
                                    continue;
                                }

                                if (comboHotkey.GamePadHotKey.IsGamePadStateEqual(gamePadState))
                                {
                                    Input.InputSender.SendHotkeyInput(comboHotkey.KeyboardHotkey);
                                    anySignalSent = true;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var comboHotkey in Settings.Settings.GamePadToHotkeyCollection)
                        {
                            if (comboHotkey.Mode == GamePadToKeyboardHotkeyModes.Disabled ||
                               (comboHotkey.Mode != GamePadToKeyboardHotkeyModes.Always &&
                                comboHotkey.Mode != GamePadToKeyboardHotkeyModes.OnGameNotRunning))
                            {
                                continue;
                            }

                            if (comboHotkey.GamePadHotKey.IsGamePadStateEqual(gamePadState))
                            {
                                Input.InputSender.SendHotkeyInput(comboHotkey.KeyboardHotkey);
                                anySignalSent = true;
                                break;
                            }
                        }
                    }
                }
            }

            // To prevent events from firing continously if the
            // buttons keep being pressed
            if (anySignalSent)
            {
                System.Threading.Thread.Sleep(350);
            }
        }

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
                if (RegisterHotKey(Settings.Settings.InformationHotkey))
                {
                    globalHotkeyRegistered = true;
                    logger.Debug($"Information Hotkey registered with hotkey {Settings.Settings.InformationHotkey}.");
                }
                else
                {
                    PlayniteApi.Notifications.Add(new NotificationMessage(Guid.NewGuid().ToString(),
                        "PlayState: " + string.Format(ResourceProvider.GetString("LOCPlayState_NotificationMessageHotkeyRegisterFailed"), Settings.Settings.InformationHotkey),
                        NotificationType.Error));
                    logger.Error($"Failed to register configured information Hotkey {Settings.Settings.InformationHotkey}.");
                }

                if (RegisterHotKey(Settings.Settings.SuspendHotKey))
                {
                    globalHotkeyRegistered = true;
                    logger.Debug($"Pause/resume Hotkey registered with hotkey {Settings.Settings.SuspendHotKey}.");
                }
                else
                {
                    PlayniteApi.Notifications.Add(new NotificationMessage(Guid.NewGuid().ToString(),
                        "PlayState: " + string.Format(ResourceProvider.GetString("LOCPlayState_NotificationMessageHotkeyRegisterFailed"), Settings.Settings.SuspendHotKey),
                        NotificationType.Error));
                    logger.Error($"Failed to register configured pause/resume Hotkey {Settings.Settings.SuspendHotKey}.");
                }

            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to register hotkeys.");
            }
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
                logger.Error($"Failed to unregister hotkey handler");
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
            playStateManager.ShowCurrentGameStatusNotification();
        }

        private void SendSuspendSignal()
        {
            playStateManager.SwitchCurrentGameState();
        }

        private async void InvokeGameProcessesDetection(OnGameStartedEventArgs args)
        {
            var game = args.Game;
            playStateManager.AddGameToDetection(game);
            var sourceActionHandled = await ScanGameSourceAction(args.Game, args.SourceAction);
            if (sourceActionHandled)
            {
                return;
            }

            var gameInstallDir = GetGameInstallDir(args.Game);
            if (!gameInstallDir.IsNullOrEmpty())
            {
                var scanHandled = await ScanGameProcessesFromDirectoryAsync(game, gameInstallDir);
                if (scanHandled)
                {
                    return;
                }
            }

            playStateManager.AddPlayStateData(game, new List<ProcessItem>());
        }

        private async Task<bool> ScanGameProcessesFromDirectoryAsync(Game game, string gameInstallDir)
        {
            var suspendPlaytimeOnly = Settings.Settings.GlobalSuspendMode == SuspendModes.Playtime && !PlayniteUtilities.GetGameHasFeature(game, featureSuspendProcesses, true) || PlayniteUtilities.GetGameHasFeature(game, featureSuspendPlaytime, true);

            // Fix for some games that take longer to start, even when already detected as running
            await Task.Delay(20000);
            if (!playStateManager.IsGameBeingDetected(game))
            {
                logger.Debug($"Detection Id was not detected. Execution of WMI Query task stopped.");
                return true;
            }

            var gameProcesses = ProcessesHandler.GetProcessesWmiQuery(true, gameInstallDir);
            if (gameProcesses.Count > 0 && gameProcesses.Any(x => x.Process.MainWindowHandle != IntPtr.Zero))
            {
                logger.Debug($"Found {gameProcesses.Count} game processes in initial WMI query");
                playStateManager.AddPlayStateData(game, gameProcesses);
                return true;
            }

            // No need to wait for the loop if we don't want to suspend processes
            if (suspendPlaytimeOnly)
            {
                playStateManager.AddPlayStateData(game, new List<ProcessItem>());
                playStateManager.AddGameToDetection(game);
            }

            // Waiting is useful for games that use a startup launcher, since
            // it can take some time before the user launches the game from it
            await Task.Delay(40000);
            var filterPaths = true;
            for (int i = 0; i < 7; i++)
            {
                // This is done to stop execution in case a new game was launched
                // or the launched game was closed
                if (!playStateManager.IsGameBeingDetected(game))
                {
                    logger.Debug($"Detection Id was not detected. Execution of WMI Query task stopped.");
                    return true;
                }

                // Try a few times with filters.
                // If nothing is found, try without filters. This helps in cases
                // where the active process is being filtered out by filters
                logger.Debug($"Starting WMI loop number {i}");
                if (i == 4)
                {
                    logger.Debug("FilterPaths set to false for WMI Query");
                    filterPaths = false;
                }

                gameProcesses = ProcessesHandler.GetProcessesWmiQuery(filterPaths, gameInstallDir);
                if (gameProcesses.Count > 0 && gameProcesses.Any(x => x.Process.MainWindowHandle != IntPtr.Zero))
                {
                    logger.Debug($"Found {gameProcesses.Count} game processes");
                    playStateManager.AddPlayStateData(game, gameProcesses);
                    return true;
                }
                else
                {
                    await Task.Delay(15000);
                }
            }

            logger.Debug("Couldn't find any game process");
            if (suspendPlaytimeOnly)
            {
                return true;
            }
            return false;
        }

        private string GetGameInstallDir(Game game)
        {
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
                    playStateManager.RemoveGameFromDetection(game);
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
            if (sourceAction == null || sourceAction.Type != GameActionType.Emulator)
            {
                return false;
            }

            logger.Debug("Source action is emulator.");
            var emulatorProfileId = sourceAction.EmulatorProfileId;
            if (emulatorProfileId.StartsWith("#builtin_"))
            {
                //Currently it isn't possible to obtain the emulator path
                //for emulators using Builtin profiles
                logger.Debug("Source action was builtin emulator, which is not compatible. Execution stopped.");
                return true;
            }

            var emulator = PlayniteApi.Database.Emulators[sourceAction.EmulatorId];
            var profile = emulator?.CustomProfiles.FirstOrDefault(p => p.Id == emulatorProfileId);
            if (profile == null)
            {
                logger.Debug($"Failed to get Custom emulator profile");
                return true;
            }

            logger.Debug($"Custom emulator profile executable is {profile.Executable}");
            var gameProcesses = ProcessesHandler.GetProcessesWmiQuery(false, string.Empty, profile.Executable);
            if (gameProcesses.Count > 0 && gameProcesses.Any(x => x.Process.MainWindowHandle != IntPtr.Zero))
            {
                playStateManager.AddPlayStateData(game, gameProcesses);
            }
            else
            {
                logger.Debug($"Failed to get valid Custom emulator profile executables process items. Starting delay...");
                await Task.Delay(20000);
                logger.Debug($"Delay finished");

                gameProcesses = ProcessesHandler.GetProcessesWmiQuery(false, string.Empty, profile.Executable);
                if (gameProcesses.Count > 0 && gameProcesses.Any(x => x.Process.MainWindowHandle != IntPtr.Zero))
                {
                    playStateManager.AddPlayStateData(game, gameProcesses);
                }
            }

            return true;
        }
    }
}