using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static WinApi.Enums;
using static WinApi.Flags;
using static WinApi.Structs;

namespace DisplayHelper.Infrastructure.Win32.Native
{
    internal static class User32Interop
    {
        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(
            string deviceName,
            DisplaySettings modeNum, // Work or int?
            ref DEVMODE devMode);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(
            string deviceName,
            int modeNum, // Work or int?
            ref DEVMODE devMode);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern DISP_CHANGE ChangeDisplaySettingsEx(
            string lpszDeviceName,
            ref DEVMODE lpDevMode,
            IntPtr hwnd,
            ChangeDisplaySettingsFlags dwflags,
            IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern DISP_CHANGE ChangeDisplaySettingsEx(
            string lpszDeviceName,
            IntPtr lpDevMode,
            IntPtr hwnd,
            ChangeDisplaySettingsFlags dwflags,
            IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayDevices(
            string lpDevice,
            uint iDevNum,
            ref DISPLAY_DEVICE lpDisplayDevice,
            uint dwFlags);
    }
}
