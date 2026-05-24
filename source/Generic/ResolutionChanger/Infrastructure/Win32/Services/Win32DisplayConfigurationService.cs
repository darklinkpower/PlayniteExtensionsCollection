using DisplayHelper.Application.Displays.DTOs;
using DisplayHelper.Domain.Common;
using DisplayHelper.Domain.Displays.Entities;
using DisplayHelper.Domain.Displays.Interfaces;
using DisplayHelper.Domain.Displays.ValueObjects;
using DisplayHelper.Infrastructure.Win32.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static WinApi.Enums;
using static WinApi.Flags;
using static WinApi.Structs;

namespace DisplayHelper.Infrastructure.Win32.Services
{

    public static class User32Test
    {
        [DllImport("user32.dll")]
        public static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);
        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string deviceName, DisplaySettings displaySetting, ref DEVMODE devMode);
        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string deviceName, int modeNumber, ref DEVMODE devMode);
        [DllImport("user32.dll")]
        public static extern DISP_CHANGE ChangeDisplaySettings(ref DEVMODE devMode, ChangeDisplaySettingsFlags dwflags);
        [DllImport("user32.dll")]
        public static extern DISP_CHANGE ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, ChangeDisplaySettingsFlags dwflags, IntPtr lParam);
        [DllImport("user32.dll")]
        // A signature for ChangeDisplaySettingsEx with a DEVMODE struct as the second parameter won't allow you to pass in IntPtr.Zero, so create an overload
        public static extern DISP_CHANGE ChangeDisplaySettingsEx(string lpszDeviceName, IntPtr lpDevMode, IntPtr hwnd, ChangeDisplaySettingsFlags dwflags, IntPtr lParam);
    }

    public sealed class Win32DisplayConfigurationService : IDisplayConfigurationService
    {
        private readonly IWin32DisplayApi _win32DisplayApi;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern DISP_CHANGE ChangeDisplaySettingsEx(
    string lpszDeviceName,
    IntPtr lpDevMode,
    IntPtr hwnd,
    ChangeDisplaySettingsFlags dwflags,
    IntPtr lParam);

        public Win32DisplayConfigurationService(IWin32DisplayApi win32DisplayApi)
        {
            _win32DisplayApi = win32DisplayApi;
        }

        public Result ApplyConfiguration(
            ApplyDisplayConfigurationRequest configuration,
            DisplayApplyMode mode)
        {
            var devMode = DevModeFactory.Create();

            if (!_win32DisplayApi.EnumDisplaySettings(
                configuration.DisplayId,
                (int)DisplaySettings.ENUM_CURRENT_SETTINGS,
                ref devMode))
            {
                return Result.Fail("Failed to enumerate current display settings.");
            }

            if (configuration.Resolution != null)
            {
                devMode.dmPelsWidth = configuration.Resolution.Width;
                devMode.dmPelsHeight = configuration.Resolution.Height;

                devMode.dmFields |=
                    DeviceModeFieldsFlags.DM_PELSWIDTH |
                    DeviceModeFieldsFlags.DM_PELSHEIGHT;
            }

            if (configuration.RefreshRate?.Value is int hz)
            {
                devMode.dmDisplayFrequency = hz;
                devMode.dmFields |= DeviceModeFieldsFlags.DM_DISPLAYFREQUENCY;
            }

            // Primary is applied via flag ONLY (no special positioning logic required)
            if (configuration.SetAsPrimary)
            {
                devMode.dmPosition.x = 0;
                devMode.dmPosition.y = 0;

                devMode.dmFields |= DeviceModeFieldsFlags.DM_POSITION;
            }
            else if (configuration.DisplayPosition != null)
            {
                devMode.dmPosition.x = configuration.DisplayPosition.X;
                devMode.dmPosition.y = configuration.DisplayPosition.Y;
                devMode.dmFields |= DeviceModeFieldsFlags.DM_POSITION;
            }

            //if (devMode.dmFields == 0)
            //{
            //    return Result.Fail("No display settings specified.");
            //}

            if (mode != DisplayApplyMode.Transactional)
            {
                var test = _win32DisplayApi.ChangeDisplaySettingsEx(
                    configuration.DisplayId,
                    ref devMode,
                    IntPtr.Zero,
                    ChangeDisplaySettingsFlags.CDS_TEST,
                    IntPtr.Zero);

                if (test != DISP_CHANGE.Successful)
                {
                    return Result.Fail($"Display mode test failed: {test}");
                }
            }

            //var test = _win32DisplayApi.ChangeDisplaySettingsEx(
            //    configuration.DisplayId,
            //    ref devMode,
            //    IntPtr.Zero,
            //    ChangeDisplaySettingsFlags.CDS_TEST,
            //    IntPtr.Zero);

            //if (test != DISP_CHANGE.Successful)
            //{
            //    return Result.Fail($"Display mode test failed: {test}");
            //}

            var flags = ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY;

            if (mode == DisplayApplyMode.Transactional)
            {
                flags |= ChangeDisplaySettingsFlags.CDS_NORESET;
            }

            if (configuration.SetAsPrimary)
            {
                flags |= ChangeDisplaySettingsFlags.CDS_SET_PRIMARY;
            }

            var result = _win32DisplayApi.ChangeDisplaySettingsEx(
                configuration.DisplayId,
                ref devMode,
                IntPtr.Zero,
                flags,
                IntPtr.Zero);

            return result == DISP_CHANGE.Successful
                ? Result.Ok()
                : Result.Fail($"Failed to apply display settings: {result}|DisplayId: {configuration.DisplayId}");
        }

        /// <summary>
        /// Sets the specified display as the primary monitor using Win32 display configuration APIs.
        ///
        /// <para>
        /// <paramref name="displayId"/> MUST be a Win32 DISPLAY device name (e.g. "\\.\DISPLAY1").
        /// </para>
        /// <para>
        /// This is a runtime identifier returned by EnumDisplayDevices / display enumeration.
        /// </para>
        /// <para>
        /// It is NOT a stable hardware identity (e.g. MonitorDeviceId or EDID), and may change across reboots.
        /// </para>
        /// <para>
        /// This method does not resolve identities; resolution must be done before calling.
        /// </para>
        /// </summary>
        public Result SetPrimaryDisplay(string displayId)
        {
            return ApplyConfiguration(
                new ApplyDisplayConfigurationRequest(
                    displayId,
                    resolution: null,
                    refreshRate: null,
                    setAsPrimary: true),
                DisplayApplyMode.Immediate);
        }
        
    }
}
