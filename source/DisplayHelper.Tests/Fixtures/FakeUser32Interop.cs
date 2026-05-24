using DisplayHelper.Infrastructure.Win32.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WinApi.Enums;
using static WinApi.Flags;
using static WinApi.Structs;

namespace DisplayHelper.Tests.Fixtures
{
    public sealed class FakeUser32Interop : IWin32DisplayApi
    {
        public List<string> Calls { get; } = new();

        public DISP_CHANGE ChangeDisplaySettingsEx(
            string deviceName,
            ref DEVMODE mode,
            IntPtr hwnd,
            ChangeDisplaySettingsFlags flags,
            IntPtr lParam)
        {
            Calls.Add("ChangeDisplaySettingsEx");
            Calls.Add($"{deviceName}:{flags}:{mode.dmPelsWidth}x{mode.dmPelsHeight}:{mode.dmDisplayFrequency}");

            return DISP_CHANGE.Successful;
        }

        public DISP_CHANGE ChangeDisplaySettingsEx(
            string deviceName,
            nint devMode,
            nint hwnd,
            ChangeDisplaySettingsFlags flags,
            nint lParam)
        {
            Calls.Add("ChangeDisplaySettingsEx");
            Calls.Add($"{deviceName}:{flags}");

            return DISP_CHANGE.Successful;
        }

        public bool EnumDisplayDevices(
            string device,
            uint deviceIndex,
            ref DISPLAY_DEVICE displayDevice,
            uint flags)
        {
            Calls.Add("EnumDisplayDevices");
            Calls.Add($"{device}:{deviceIndex}:{displayDevice.DeviceID}x{flags}");

            return true;
        }

        public bool EnumDisplaySettings(
            string deviceName,
            int modeNum,
            ref DEVMODE mode)
        {
            Calls.Add("EnumDisplaySettings");

            mode.dmPelsWidth = 1920;
            mode.dmPelsHeight = 1080;
            mode.dmDisplayFrequency = 60;

            return true;
        }
    }
}
