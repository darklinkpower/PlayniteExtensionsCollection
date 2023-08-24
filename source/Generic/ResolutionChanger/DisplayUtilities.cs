using Playnite.SDK;
using ResolutionChanger.Enums;
using ResolutionChanger.Models;
using ResolutionChanger.Native;
using ResolutionChanger.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ResolutionChanger
{
    public static class DisplayUtilities
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static DEVMODE GetDevMode()
        {
            return new DEVMODE()
            {
                dmDeviceName = new string(new char[32]),
                dmFormName = new string(new char[32]),
                dmSize = (short)Marshal.SizeOf(typeof(DEVMODE))
            };
        }

        private static DISPLAY_DEVICE GetDisplayDevice()
        {
            return new DISPLAY_DEVICE()
            {
                cb = (short)Marshal.SizeOf(typeof(DISPLAY_DEVICE))
            };
        }

        public static DEVMODE GetMainScreenDevMode()
        {
            return GetScreenDevMode(GetPrimaryScreenName());
        }

        public static DEVMODE GetScreenDevMode(string deviceName)
        {
            DEVMODE devMode = GetDevMode();
            User32.EnumDisplaySettings(deviceName, User32.ENUM_CURRENT_SETTINGS, ref devMode);
            return devMode;
        }

        public static string GetPrimaryScreenName()
        {
            return GetPrimaryScreenName(GetAvailableDisplayDevices());
        }

        public static string GetPrimaryScreenName(List<DISPLAY_DEVICE> displayDevices)
        {
            return displayDevices.First(x => x.StateFlags.HasFlag(DisplayDeviceStateFlags.PrimaryDevice)).DeviceName;
        }

        private static bool SetPrimaryDisplay(string displayName)
        {
            var displays = GetAvailableDisplayDevices();
            var primaryDisplayDevice = displays.Find(d => d.DeviceName.Equals(displayName));
            if (!primaryDisplayDevice.DeviceName.Equals(displayName))
            {
                return false;
            }

            var deviceMode = GetDevMode();
            if (!User32.EnumDisplaySettings(primaryDisplayDevice.DeviceName, User32.ENUM_CURRENT_SETTINGS, ref deviceMode))
            {
                return false;
            }

            var offsetx = deviceMode.dmPosition.x;
            var offsety = deviceMode.dmPosition.y;
            if (offsetx == 0 && offsety == 0)
            {
                return true;
            }

            deviceMode.dmPosition.x = 0;
            deviceMode.dmPosition.y = 0;
            var dwFlags = (ChangeDisplaySettingsFlags.CDS_SET_PRIMARY | ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY | ChangeDisplaySettingsFlags.CDS_NORESET);
            if (User32.ChangeDisplaySettingsEx(primaryDisplayDevice.DeviceName, ref deviceMode, (IntPtr)null, dwFlags, IntPtr.Zero) != DISP_CHANGE.Successful)
            {
                return false;
            }

            var otherDisplays = GetAvailableDisplayDevices().FindAll(d => !d.DeviceName.Equals(displayName));
            var otherDwFlags = ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY | ChangeDisplaySettingsFlags.CDS_NORESET;
            foreach (var otherDisplay in otherDisplays)
            {
                var otherDeviceMode = GetDevMode();
                if (!User32.EnumDisplaySettings(otherDisplay.DeviceName, User32.ENUM_CURRENT_SETTINGS, ref otherDeviceMode))
                {
                    return false;
                }

                otherDeviceMode.dmPosition.x -= offsetx;
                otherDeviceMode.dmPosition.y -= offsety;
                if (User32.ChangeDisplaySettingsEx(otherDisplay.DeviceName, ref otherDeviceMode, (IntPtr)null, otherDwFlags, IntPtr.Zero) != DISP_CHANGE.Successful)
                {
                    return false;
                }
            }

            return SaveDisplaySettings() == DISP_CHANGE.Successful;
        }

        private static DISP_CHANGE SaveDisplaySettings()
        {
            return User32.ChangeDisplaySettingsEx(null, IntPtr.Zero, (IntPtr)null, ChangeDisplaySettingsFlags.CDS_NONE, (IntPtr)null);
        }

        public static DISP_CHANGE ChangeDisplaySettings(string displayDeviceName, int newWidth = 0, int newHeight = 0, int newRefreshRate = 0, bool applyChanges = true)
        {
            try
            {
                DEVMODE devMode = GetDevMode();
                if (User32.EnumDisplaySettings(displayDeviceName, User32.ENUM_CURRENT_SETTINGS, ref devMode))
                {
                    var displayChangeFlags = ChangeDisplaySettingsFlags.CDS_NONE;
                    if (newWidth != 0 && newHeight != 0)
                    {
                        devMode.dmPelsWidth = newWidth;
                        devMode.dmPelsHeight = newHeight;
                        devMode.dmFields |= User32.DM_PELSWIDTH | User32.DM_PELSHEIGHT;
                        displayChangeFlags |= ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY;
                    }

                    if (newRefreshRate != 0)
                    {
                        devMode.dmDisplayFrequency = newRefreshRate;
                        devMode.dmFields |= User32.DM_DISPLAYFREQUENCY;
                        displayChangeFlags |= ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY;
                    }

                    if ((displayChangeFlags & ~(ChangeDisplaySettingsFlags.CDS_NONE)) == ChangeDisplaySettingsFlags.CDS_NONE)
                    {
                        logger.Debug($"No changes are needed for \"{displayDeviceName}\" display");
                        return DISP_CHANGE.Successful;
                    }

                    if (!applyChanges)
                    {
                        displayChangeFlags |= ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY;
                        displayChangeFlags |= ChangeDisplaySettingsFlags.CDS_NORESET;
                    }

                    logger.Debug($"Setting configuration of device \"{displayDeviceName}\", {newWidth}x{newHeight}, {newRefreshRate}, {displayChangeFlags}");
                    var testResult = User32.ChangeDisplaySettingsEx(displayDeviceName, ref devMode, IntPtr.Zero, ChangeDisplaySettingsFlags.CDS_TEST, IntPtr.Zero);
                    if (testResult == DISP_CHANGE.Successful)
                    {
                        var changeResult = User32.ChangeDisplaySettingsEx(displayDeviceName, ref devMode, IntPtr.Zero, displayChangeFlags, IntPtr.Zero);
                        switch (changeResult)
                        {
                            case DISP_CHANGE.Successful:
                                logger.Info($"Display settings changed successfully");
                                return changeResult;
                            default:
                                logger.Info($"Failed to change display settings: {changeResult}");
                                return changeResult;
                        }
                    }
                    else
                    {
                        logger.Info($"Display change test failed with value {testResult}");
                        return DISP_CHANGE.Failed;
                    }
                }
                else
                {
                    logger.Info($"Failed to enumerate {displayDeviceName} display settings");
                    return DISP_CHANGE.Failed;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"An error occurred during ChangeScreenConfiguration");
                return DISP_CHANGE.Failed;
            }
        }

        public static bool ChangeDisplayConfiguration(string displayDeviceName, int newWidth, int newHeight, int newRefreshRate, bool setAsPrimaryDevice)
        {
            if (setAsPrimaryDevice && !SetPrimaryDisplay(displayDeviceName))
            {
                return false;
            }
            
            return ChangeDisplaySettings(displayDeviceName, newWidth, newHeight, newRefreshRate, true) == DISP_CHANGE.Successful;
        }

        public static bool RestoreDisplayConfiguration(DisplayConfigChangeData displayRestoreData)
        {
            var restoreResolution = displayRestoreData.RestoreResolutionValues;
            var restoreRefreshRate = displayRestoreData.RestoreRefreshRate;
            var restorePrimaryDisplay = displayRestoreData.RestorePrimaryDisplay;

            if (!restoreResolution && !restoreRefreshRate && !restorePrimaryDisplay)
            {
                return true;
            }

            var newWidth = restoreResolution ? displayRestoreData.DevMode.dmPelsWidth : 0;
            var newHeight = restoreResolution ? displayRestoreData.DevMode.dmPelsHeight : 0;
            var newFrequency = restoreRefreshRate ? displayRestoreData.DevMode.dmDisplayFrequency : 0;

            var onlyRestorePrimaryDisplay = !restoreResolution && !restoreRefreshRate && restorePrimaryDisplay;
            if (restorePrimaryDisplay)
            {
                var success = SetPrimaryDisplay(displayRestoreData.PrimaryDisplayName);
                if (onlyRestorePrimaryDisplay)
                {
                    return success;
                }
            }

            return ChangeDisplaySettings(displayRestoreData.TargetDisplayName, newWidth, newHeight, newFrequency, true) == DISP_CHANGE.Successful;
        }

        public static List<DEVMODE> GetMainScreenAvailableDevModes()
        {
            return GetScreenAvailableDevModes(GetPrimaryScreenName());
        }

        public static List<DEVMODE> GetScreenAvailableDevModes(string displayDeviceName)
        {
            var availableModes = new List<DEVMODE>();
            try
            {
                var devMode = GetDevMode();
                for (int modeIndex = 0; User32.EnumDisplaySettings(displayDeviceName, modeIndex, ref devMode); modeIndex++)
                {
                    int width = devMode.dmPelsWidth;
                    int height = devMode.dmPelsHeight;
                    if (width != 0 && height != 0)
                    {
                        availableModes.Add(devMode);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error while enumerating {displayDeviceName} display settings");
            }

            return availableModes;
        }

        public static List<DISPLAY_DEVICE> GetAvailableDisplayDevices()
        {
            var availableDisplayDevices = new List<DISPLAY_DEVICE>();
            try
            {
                DISPLAY_DEVICE displayDevice = GetDisplayDevice();
                for (uint deviceIndex = 0; (User32.EnumDisplayDevices(null, deviceIndex, ref displayDevice, 0)); deviceIndex++)
                {
                    displayDevice.cb = Marshal.SizeOf(displayDevice);
                    if (displayDevice.StateFlags.HasFlag(DisplayDeviceStateFlags.AttachedToDesktop))
                    {
                        availableDisplayDevices.Add(displayDevice);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error while enumarating display devices");
            }

            return availableDisplayDevices;
        }

        public static string CalculateAspectRatioString(int width, int height)
        {
            int gcd = CalculateGreatestCommonDivisor(width, height);
            int aspectWidth = width / gcd;
            int aspectHeight = height / gcd;

            return $"{aspectWidth}:{aspectHeight}";
        }

        private static int CalculateGreatestCommonDivisor(int a, int b)
        {
            while (b != 0)
            {
                int remainder = a % b;
                a = b;
                b = remainder;
            }

            return a;
        }
    }
}