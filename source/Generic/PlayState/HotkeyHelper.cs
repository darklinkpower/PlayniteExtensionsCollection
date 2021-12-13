using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlayState
{
    public static class HotkeyHelper
    {
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey([In] IntPtr hWnd, [In] int id, [In] uint fsModifiers, [In] uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey([In] IntPtr hWndm, [In] int id);

        public static uint WM_HOTKEY = 0x0312;

        [Flags]
        public enum Mod : uint
        {
            NONE = 0x0000,
            ALT = 0x0001,
            CTRL = 0x0002,
            SHIFT = 0x0004,
            WIN = 0x0008,
            NOREPEAT = 0x4000,
        }

        public static uint ToVK(this ModifierKeys mod)
        {
            uint vk = 0;
            if ((mod & ModifierKeys.Control) == ModifierKeys.Control)
            {
                vk |= (uint)Mod.CTRL;
            }
            if ((mod & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                vk |= (uint)Mod.ALT;
            }
            if ((mod & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                vk |= (uint)Mod.SHIFT;
            }
            if ((mod & ModifierKeys.Windows) == ModifierKeys.Windows)
            {
                vk |= (uint)Mod.WIN;
            }
            return vk;
        }

        public static uint ToVK(this Key key)
        {
            return (uint)KeyInterop.VirtualKeyFromKey(key);
        }
    }
}