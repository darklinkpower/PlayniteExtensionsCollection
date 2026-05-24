using DisplayHelper.Infrastructure.Win32.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinApi;
using static WinApi.Structs;

namespace DisplayHelper.Infrastructure.Win32.Native
{
    public sealed class Win32DisplayApi : IWin32DisplayApi
    {
        public bool EnumDisplayDevices(
            string device,
            uint deviceIndex,
            ref DISPLAY_DEVICE displayDevice,
            uint flags)
        {
            return User32Interop.EnumDisplayDevices(
                device,
                deviceIndex,
                ref displayDevice,
                flags);
        }

        public bool EnumDisplaySettings(
            string deviceName,
            int modeIndex,
            ref DEVMODE devMode)
        {
            return User32Interop.EnumDisplaySettings(
                deviceName,
                modeIndex,
                ref devMode);
        }

        public Enums.DISP_CHANGE ChangeDisplaySettingsEx(
            string deviceName,
            ref DEVMODE devMode,
            IntPtr hwnd,
            Flags.ChangeDisplaySettingsFlags flags,
            IntPtr lParam)
        {
            return User32Interop.ChangeDisplaySettingsEx(
               deviceName,
               ref devMode,
               hwnd,
               flags,
               lParam);
        }

        public Enums.DISP_CHANGE ChangeDisplaySettingsEx(
            string deviceName,
            IntPtr devMode,
            IntPtr hwnd,
            Flags.ChangeDisplaySettingsFlags flags,
            IntPtr lParam)
        {
            return User32Interop.ChangeDisplaySettingsEx(
               deviceName,
               devMode,
               hwnd,
               flags,
               lParam);
        }

    }
}
