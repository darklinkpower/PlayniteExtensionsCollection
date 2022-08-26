using PlayState.Models;
using PlayState.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static PlayState.Native.Winuser;

namespace PlayState.Input
{
    public static class InputSender
    {
        internal static INPUT BuildINPUT(Key k, KEYEVENTF flags)
        {
            return new INPUT
            {
                Type = Winuser.InputType.INPUT_KEYBOARD,
                Union = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        Vk = (VirtualKeyShort)KeyInterop.VirtualKeyFromKey(k),
                        wScan = 0,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            };
        }

        public static void SendHotkeyInput(HotKey hotkey)
        {
            SendInput(hotkey.Modifiers, hotkey.Key);
        }

        public static void SendInput(ModifierKeys modifiers, Key key)
        {
            var keys = new List<Key>();
            if (modifiers.HasFlag(ModifierKeys.Control))
            {
                keys.Add(Key.LeftCtrl);
            } 
            if (modifiers.HasFlag(ModifierKeys.Alt))
            {
                keys.Add(Key.LeftAlt);
            }
            if (modifiers.HasFlag(ModifierKeys.Shift))
            {
                keys.Add(Key.LeftShift);
            }
            if (modifiers.HasFlag(ModifierKeys.Windows))
            {
                keys.Add(Key.LWin);
            }

            keys.Add(key);
            var totalInputs = keys.Count * 2;
            INPUT[] inputs = new INPUT[totalInputs];

            for (int i = 0; i < keys.Count; i++)
            {
                inputs[i] = BuildINPUT(keys[i], KEYEVENTF.NONE);
                inputs[totalInputs - 1 - i] = BuildINPUT(keys[i], KEYEVENTF.KEYUP);
            }

            User32.SendInput((uint)totalInputs, inputs, INPUT.Size);
        }
    }
    
}