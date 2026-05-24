using DisplayHelper.Domain.Displays.Entities;
using DisplayHelper.Domain.Displays.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WinApi.Flags;
using static WinApi.Structs;

namespace DisplayHelper.Infrastructure.Win32.Mapping
{
    internal static class DisplayDeviceMapper
    {
        public static DisplayDevice Map(
            DISPLAY_DEVICE adapter,
            DISPLAY_DEVICE monitor,
            DisplayState currentMode,
            IReadOnlyList<DisplayMode> supportedModes,
            MonitorIdentity identifier)
        {
            return new DisplayDevice(
                adapter.DeviceID,
                adapter.DeviceName,
                adapter.DeviceString,
                adapter.StateFlags.HasFlag(DisplayDeviceStateFlags.PrimaryDevice),
                monitor.DeviceID,
                monitor.DeviceName,
                monitor.DeviceString,
                currentMode,
                supportedModes,
                identifier);
        }
    }
}
