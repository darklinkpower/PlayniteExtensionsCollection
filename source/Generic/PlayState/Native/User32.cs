using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static PlayState.Native.Winuser;

namespace PlayState.Native
{
    public class User32
    {
        private const string dllName = "User32.dll";

        [DllImport(dllName, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport(dllName, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport(dllName)]
        public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
    }
}