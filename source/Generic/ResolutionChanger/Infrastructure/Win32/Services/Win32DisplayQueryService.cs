using DisplayHelper.Domain.Displays.Entities;
using DisplayHelper.Domain.Displays.Interfaces;
using DisplayHelper.Domain.Displays.ValueObjects;
using DisplayHelper.Infrastructure.Win32.Mapping;
using DisplayHelper.Infrastructure.Win32.Native;
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
    public sealed class Win32DisplayQueryService : IDisplayQueryService
    {
        private readonly IWin32DisplayApi _displayApi;
        private readonly IMonitorIdentityProvider _monitorIdentityProvider;

        public Win32DisplayQueryService(
            IWin32DisplayApi displayApi,
            IMonitorIdentityProvider monitorIdentityProvider)
        {
            _displayApi = displayApi;
            _monitorIdentityProvider = monitorIdentityProvider;
        }

        public IReadOnlyList<DisplayDevice> GetDisplays()
        {
            var displays = new List<DisplayDevice>();

            DISPLAY_DEVICE adapter = DisplayDeviceFactory.Create();

            for (uint adapterIndex = 0;
                 _displayApi.EnumDisplayDevices(
                     null,
                     adapterIndex,
                     ref adapter,
                     0);
                 adapterIndex++)
            {
                if (!adapter.StateFlags.HasFlag(
                    DisplayDeviceStateFlags.AttachedToDesktop))
                {
                    adapter = DisplayDeviceFactory.Create();
                    continue;
                }

                var currentMode =
                    GetCurrentMode(adapter.DeviceName);

                var supportedModes =
                    GetSupportedModes(adapter.DeviceName);

                DISPLAY_DEVICE monitor =
                    DisplayDeviceFactory.Create();

                bool monitorFound = false;

                for (uint monitorIndex = 0;
                     _displayApi.EnumDisplayDevices(
                         adapter.DeviceName,
                         monitorIndex,
                         ref monitor,
                         0);
                     monitorIndex++)
                {
                    monitorFound = true;

                    var identifier =
                        MonitorIdentity.Unknown;

                    if (!string.IsNullOrWhiteSpace(monitor.DeviceID))
                    {
                        identifier =
                            _monitorIdentityProvider.Get(
                                monitor.DeviceID);
                    }

                    displays.Add(
                        DisplayDeviceMapper.Map(
                            adapter,
                            monitor,
                            currentMode,
                            supportedModes,
                            identifier));

                    monitor = DisplayDeviceFactory.Create();
                }

                // Fallback in case no monitor objects were returned.
                // This can happen for some virtual/mirrored displays.
                if (!monitorFound)
                {
                    displays.Add(
                        DisplayDeviceMapper.Map(
                            adapter,
                            monitor,
                            currentMode,
                            supportedModes,
                            MonitorIdentity.Unknown));
                }

                adapter = DisplayDeviceFactory.Create();
            }

            return displays;
        }

        public DisplayDevice GetPrimaryDisplay()
        {
            return GetDisplays().FirstOrDefault(x => x.IsPrimary);
        }

        public DisplayDevice GetDisplayByAdapterId(string displayId)
        {
            return GetDisplays().FirstOrDefault(x => x.AdapterId == displayId);
        }

        public IReadOnlyList<DisplayMode> GetSupportedModes(
            string displayAdapterName)
        {
            var modes = new List<DisplayMode>();

            var devMode = DevModeFactory.Create();

            for (int i = 0;
                 _displayApi.EnumDisplaySettings(
                     displayAdapterName,
                     i,
                     ref devMode);
                 i++)
            {
                modes.Add(
                    DisplayModeMapper.Map(devMode));

                devMode = DevModeFactory.Create();
            }

            return modes
                .Distinct()
                .OrderByDescending(x => x.Resolution.Width)
                .ThenByDescending(x => x.Resolution.Height)
                .ThenByDescending(x => x.RefreshRate.Value)
                .ToList();
        }

        private DisplayState GetCurrentMode(string displayId)
        {
            var devMode = DevModeFactory.Create();

            _displayApi.EnumDisplaySettings(
                displayId,
                (int)DisplaySettings.ENUM_CURRENT_SETTINGS,
                ref devMode);

            return DevModeMapper.Map(devMode);
        }
    }
}