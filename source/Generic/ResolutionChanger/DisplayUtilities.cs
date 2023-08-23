using Playnite.SDK;
using ResolutionChanger.Enums;
using ResolutionChanger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResolutionChanger
{
    public class DisplayUtilities
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DISPLAY_DEVICE
        {
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public int StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        public static class User_32
        {
            [DllImport("user32.dll")]
            public static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);
            [DllImport("user32.dll")]
            public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);
            [DllImport("user32.dll")]
            public static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);
            [DllImport("user32.dll")]
            public static extern int ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, uint dwflags, IntPtr lParam);

            public const int ENUM_CURRENT_SETTINGS = -1;
            public const int ENUM_REGISTRY_SETTINGS = -2;
            public const int CDS_UPDATEREGISTRY = 0x01;
            public const int CDS_TEST = 0x02;
            public const int DISP_CHANGE_SUCCESSFUL = 0;
            public const int DISP_CHANGE_RESTART = 1;
            public const int DISP_CHANGE_FAILED = -1;
            public const int DISP_CHANGE_BADMODE = -2;
            public const int DISP_CHANGE_NOTUPDATED = -3;
            public const int DISP_CHANGE_BADFLAGS = -4;
            public const int DISP_CHANGE_BADPARAM = -5;
            public const int CCDEVICENAME = 32;
            public const int CCFORMNAME = 32;
            public const int DM_PELSWIDTH = 0x00080000;
            public const int DM_PELSHEIGHT = 0x00100000;
            public const int DM_DISPLAYFREQUENCY = 0x00040000;
        }

        public static class DisplayHelper
        {
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
                DEVMODE devMode = GetDevMode();
                User_32.EnumDisplaySettings(Screen.PrimaryScreen.DeviceName, User_32.ENUM_CURRENT_SETTINGS, ref devMode);
                return devMode;
            }

            public static string GetMainScreenName()
            {
                return Screen.PrimaryScreen.DeviceName;
            }

            public static ResolutionChangeResult ChangeScreenConfiguration(string displayDeviceName, int newWidth, int newHeight, int newRefreshRate)
            {
                if (newWidth == 0 && newHeight == 0 && newRefreshRate == 0)
                {
                    logger.Debug("Nothing to set. Width, height, and refresh rate are 0.");
                    return ResolutionChangeResult.Success;
                }

                try
                {
                    logger.Debug($"Setting configuration of device \"{displayDeviceName}\" to {newWidth}x{newHeight} and refresh rate {newRefreshRate}...");
                    DEVMODE devMode = GetDevMode();
                    if (User_32.EnumDisplaySettings(displayDeviceName, User_32.ENUM_CURRENT_SETTINGS, ref devMode))
                    {
                        if (newWidth != 0 && newHeight != 0)
                        {
                            devMode.dmPelsWidth = newWidth;
                            devMode.dmPelsHeight = newHeight;
                            devMode.dmFields |= User_32.DM_PELSWIDTH | User_32.DM_PELSHEIGHT;
                        }

                        if (newRefreshRate != 0)
                        {
                            devMode.dmDisplayFrequency = newRefreshRate;
                            devMode.dmFields |= User_32.DM_DISPLAYFREQUENCY;
                        }

                        int testResult = User_32.ChangeDisplaySettingsEx(displayDeviceName, ref devMode, IntPtr.Zero, User_32.CDS_TEST, IntPtr.Zero);
                        if (testResult == User_32.DISP_CHANGE_SUCCESSFUL)
                        {
                            var changeResult = User_32.ChangeDisplaySettingsEx(displayDeviceName, ref devMode, IntPtr.Zero, User_32.CDS_UPDATEREGISTRY, IntPtr.Zero);
                            switch (changeResult)
                            {
                                case User_32.DISP_CHANGE_SUCCESSFUL:
                                    logger.Info($"Display settings changed successfully");
                                    return ResolutionChangeResult.Success;
                                case User_32.DISP_CHANGE_RESTART:
                                    logger.Info("Resolution change requires restart.");
                                    return ResolutionChangeResult.RestartRequired;
                                default:
                                    logger.Info($"Failed to set resolution ({changeResult}).");
                                    return ResolutionChangeResult.Failed;
                            }
                        }
                        else
                        {
                            logger.Info($"Display change test failed with value {testResult}");
                            return ResolutionChangeResult.Failed;
                        }
                    }
                    else
                    {
                        logger.Info($"Failed to enumerate {displayDeviceName} display settings");
                        return ResolutionChangeResult.Failed;
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, $"An error occurred during ChangeScreenConfiguration");
                    return ResolutionChangeResult.Failed;
                }
            }

            public static ResolutionChangeResult ChangeDisplayConfiguration(string displayDeviceName, int newWidth, int newHeight, int newRefreshRate)
            {
                return ChangeScreenConfiguration(displayDeviceName, newWidth, newHeight, newRefreshRate);
            }

            public static ResolutionChangeResult RestoreDisplayConfiguration(DisplayConfigChangeData displayRestoreData)
            {
                if (!displayRestoreData.ResolutionChanged && !displayRestoreData.RefreshRateChanged)
                {
                    return ResolutionChangeResult.Success;
                }

                var newWidth = displayRestoreData.ResolutionChanged ? displayRestoreData.DevMode.dmPelsWidth : 0;
                var newHeight = displayRestoreData.ResolutionChanged ? displayRestoreData.DevMode.dmPelsHeight : 0;
                var newFrequency = displayRestoreData.RefreshRateChanged ? displayRestoreData.DevMode.dmDisplayFrequency : 0;

                return ChangeScreenConfiguration(displayRestoreData.DisplayDeviceName, newWidth, newHeight, newFrequency);
            }

            public static List<DEVMODE> GetScreenAvailableDevModes(string displayDeviceName)
            {
                var availableModes = new List<DEVMODE>();
                try
                {
                    var devMode = GetDevMode();
                    for (int modeIndex = 0; User_32.EnumDisplaySettings(displayDeviceName, modeIndex, ref devMode); modeIndex++)
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

            public static List<DISPLAY_DEVICE> GetAvailableScreenDevices()
            {
                var availableDisplayDevices = new List<DISPLAY_DEVICE>();
                try
                {
                    DISPLAY_DEVICE displayDevice = GetDisplayDevice();
                    for (uint deviceIndex = 0; (User_32.EnumDisplayDevices(null, deviceIndex, ref displayDevice, 0)); deviceIndex++)
                    {
                        availableDisplayDevices.Add(displayDevice);
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Error while enumarating display devices");
                }

                return availableDisplayDevices;
            }

            public static List<DEVMODE> GetMainScreenAvailableDevModes()
            {
                return GetScreenAvailableDevModes(GetMainScreenName());
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
}