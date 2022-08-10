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

        public GamePadStateHotkey(GamePadState gamePadState)
        {
            Buttons = gamePadState.Buttons;
            DPad = gamePadState.DPad;
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

            return true;
        }
    }
}