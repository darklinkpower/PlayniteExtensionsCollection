using Playnite.SDK;
using PlayState.Enums;
using PlayState.ViewModels;
using PlayState.XInputDotNetPure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlayState
{
    public class GamePadHandler
    {
        private readonly PlayState _playState;
        private readonly PlayStateSettings _settings;
        private readonly PlayStateManagerViewModel _playStateManager;
        private readonly Timer _controllersStateCheckTimer;
        private bool _isCheckRunning;
        private const int _pollingRate = 80;

        public GamePadHandler(PlayState playState, PlayStateSettings settings, PlayStateManagerViewModel playStateManager)
        {
            _playState = playState;
            _settings = settings;
            _playStateManager = playStateManager;
            _controllersStateCheckTimer = new Timer(OnControllerTimerElapsed, null, 0, _pollingRate);
        }

        public bool IsAnyControllerConnected()
        {
            for (int i = 0; i <= 3; i++)
            {
                var playerIndex = (PlayerIndex)i;
                var gamePadState = GamePad.GetState(playerIndex);
                if (gamePadState.IsConnected)
                {
                    return true;
                }
            }

            return false;
        }

        private async void OnControllerTimerElapsed(object state)
        {
            if (_isCheckRunning)
            {
                return;
            }

            _isCheckRunning = true;
            try
            {
                await CheckControllersAsync();
            }
            finally
            {
                _isCheckRunning = false;
            }
        }

        private async Task CheckControllersAsync()
        {
            var maxCheckIndex = _settings.GamePadHotkeysEnableAllControllers ? 3 : 0;
            var anySignalSent = false;
            for (int i = 0; i <= maxCheckIndex; i++)
            {
                var playerIndex = (PlayerIndex)i;
                var gamePadState = GamePad.GetState(playerIndex);

                if (!gamePadState.IsConnected || !gamePadState.IsAnyButtonOrDpadPressed)
                {
                    continue;
                }

                if (_playState.IsAnyGameRunning())
                {
                    if (HandleGameRunningHotkeys(gamePadState))
                    {
                        anySignalSent = true;
                    }
                }
                else
                {
                    if (HandleGameNotRunningHotkeys(gamePadState))
                    {
                        anySignalSent = true;
                    }
                }
            }

            // To prevent events from firing continously if the
            // buttons keep being pressed
            if (anySignalSent)
            {
                await Task.Delay(350);
            }
        }

        private bool HandleGameRunningHotkeys(GamePadState gamePadState)
        {
            if (_settings.GamePadInformationHotkeyEnable && _settings.GamePadInformationHotkey?.IsGamePadStateEqual(gamePadState) == true)
            {
                _playStateManager.ShowCurrentGameStatusNotification();
                return true;
            }

            if (_settings.GamePadSuspendHotkeyEnable && _settings.GamePadSuspendHotkey?.IsGamePadStateEqual(gamePadState) == true)
            {
                _playStateManager.SwitchCurrentGameState();
                return true;
            }

            foreach (var comboHotkey in _settings.GamePadToHotkeyCollection)
            {
                if (IsValidGamePadHotkeyMode(comboHotkey.Mode, GamePadToKeyboardHotkeyModes.OnGameRunning))
                {
                    if (comboHotkey.GamePadHotKey.IsGamePadStateEqual(gamePadState))
                    {
                        Input.InputSender.SendHotkeyInput(comboHotkey.KeyboardHotkey);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool HandleGameNotRunningHotkeys(GamePadState gamePadState)
        {
            foreach (var comboHotkey in _settings.GamePadToHotkeyCollection)
            {
                if (IsValidGamePadHotkeyMode(comboHotkey.Mode, GamePadToKeyboardHotkeyModes.OnGameNotRunning))
                {
                    if (comboHotkey.GamePadHotKey.IsGamePadStateEqual(gamePadState))
                    {
                        Input.InputSender.SendHotkeyInput(comboHotkey.KeyboardHotkey);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsValidGamePadHotkeyMode(GamePadToKeyboardHotkeyModes mode, GamePadToKeyboardHotkeyModes targetMode)
        {
            return mode == GamePadToKeyboardHotkeyModes.Always || mode == targetMode;
        }
    }
}