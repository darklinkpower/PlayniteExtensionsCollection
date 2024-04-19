using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PlayState
{
    public static class WindowsHelper
    {
        private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool ShowWindowAsync(IntPtr windowHandle, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool SetForegroundWindow(IntPtr windowHandle);

        public const int SW_SHOW = 5;
        private const int SW_MINIMIZE = 6;
        public const int SW_RESTORE = 9;
        public const int SW_SHOWDEFAULT = 10;

        public static Dictionary<IntPtr, string> GetOpenWindows()
        {
            IntPtr shellWindow = GetShellWindow();
            Dictionary<IntPtr, string> windows = new Dictionary<IntPtr, string>();

            EnumWindows(delegate (IntPtr hWnd, int lParam)
            {
                if (hWnd == shellWindow)
                {
                    return true;
                }

                if (!IsWindowVisible(hWnd))
                {
                    return true;

                }

                var length = GetWindowTextLength(hWnd);
                if (length == 0)
                {
                    return true;
                }

                StringBuilder builder = new StringBuilder(length);
                GetWindowText(hWnd, builder, length + 1);

                windows[hWnd] = builder.ToString();
                return true;

            }, 0);

            return windows;
        }

        public static IntPtr GetForegroundWindowHandle()
        {
            return GetForegroundWindow();
        }

        public static void RestoreAndFocusWindow(IntPtr windowHandle)
        {
            ShowWindow(windowHandle, SW_SHOWDEFAULT);
            ShowWindow(windowHandle, SW_SHOW);
            SetForegroundWindow(windowHandle);
        }

        public static void MinimizeWindow(IntPtr windowHandle)
        {
            ShowWindow(windowHandle, SW_MINIMIZE);
        }
    }
}