using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WinApi.Enums;
using static WinApi.Flags;
using static WinApi.Structs;

namespace DisplayHelper.Infrastructure.Win32.Services
{
    public interface IWin32DisplayApi
    {
        bool EnumDisplayDevices(
            string device,
            uint deviceIndex,
            ref DISPLAY_DEVICE displayDevice,
            uint flags);

        bool EnumDisplaySettings(
            string deviceName,
            int modeIndex,
            ref DEVMODE devMode);

        DISP_CHANGE ChangeDisplaySettingsEx(
            string deviceName,
            ref DEVMODE devMode,
            IntPtr hwnd,
            ChangeDisplaySettingsFlags flags,
            IntPtr lParam);

        DISP_CHANGE ChangeDisplaySettingsEx(
            string deviceName,
            IntPtr devMode,
            IntPtr hwnd,
            ChangeDisplaySettingsFlags flags,
            IntPtr lParam);
    }
}
