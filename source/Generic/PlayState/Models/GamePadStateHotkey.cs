using Playnite.SDK;
using PlayState.XInputDotNetPure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayState.Models
{
    public class GamePadStateHotkey : ObservableObject
    {
        GamePadButtons buttons;
        public GamePadButtons Buttons { get => buttons; set => SetValue(ref buttons, value); }

        GamePadDPad dPad;
        public GamePadDPad DPad { get => dPad; set => SetValue(ref dPad, value); }

        GamePadTriggers triggers = new GamePadTriggers(0f, 0f);
        public GamePadTriggers Triggers { get => triggers; set => SetValue(ref triggers, value); }

        public GamePadStateHotkey(GamePadState gamePadState)
        {
            Buttons = gamePadState.Buttons;
            DPad = gamePadState.DPad;
            Triggers = gamePadState.Triggers;
        }

        public bool IsGamePadStateEqual(GamePadState gamePadState)
        {
            if (Buttons != gamePadState.Buttons)
            {
                return false;
            }

            if (DPad != gamePadState.DPad)
            {
                return false;
            }

            if (Triggers.Left >= 0.5f && gamePadState.Triggers.Left < 0.5f)
            {
                return false;
            }

            if (Triggers.Right >= 0.5f && gamePadState.Triggers.Right < 0.5f)
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            var lines = new List<string>();
            var pressedButtons = Buttons.GetPressedButtonsList();
            var pressedDpadButtons = DPad.GetPressedButtonsList();
            if (pressedButtons.HasItems())
            {
                lines.Add(string.Format(ResourceProvider.GetString("LOCPlayState_SettingsGamePadHotkeyButtonsLabel"), string.Join(", ", pressedButtons)));
            }

            if (pressedDpadButtons.HasItems())
            {
                lines.Add(string.Format(ResourceProvider.GetString("LOCPlayState_SettingsGamePadHotkeyDpadLabel"), string.Join(", ", pressedDpadButtons)));
            }

            var enabledTriggers = new List<string>();
            if (Triggers.Left >= 0.5f)
            {
                enabledTriggers.Add("LeftTrigger");
            }

            if (Triggers.Right >= 0.5f)
            {
                enabledTriggers.Add("RightTrigger");
            }

            if (enabledTriggers.HasItems())
            {
                lines.Add(string.Format(ResourceProvider.GetString("LOCPlayState_SettingsGamePadHotkeyTriggersLabel"), string.Join(", ", enabledTriggers)));
            }

            return string.Join("\n", lines);
        }
    }
}