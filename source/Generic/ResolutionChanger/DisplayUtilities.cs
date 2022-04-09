using Playnite.SDK;
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

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
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
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
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

        public static class User_32
        {
            [DllImport("user32.dll")]
            public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);
            [DllImport("user32.dll")]
            public static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

            public const int ENUM_CURRENT_SETTINGS = -1;
            public const int ENUM_REGISTRY_SETTINGS = -2;
            public const int CDS_UPDATEREGISTRY = 0x01;
            public const int CDS_TEST = 0x02;
            public const int DISP_CHANGE_SUCCESSFUL = 0;
            public const int DISP_CHANGE_RESTART = 1;
            public const int DISP_CHANGE_FAILED = -1;
        }

        public static class DisplayHelper
        {
            public static DEVMODE GetMainScreenDevMode()
            {
                DEVMODE dm = new DEVMODE();
                var screen = Screen.PrimaryScreen;
                dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                User_32.EnumDisplaySettings(screen.DeviceName, User_32.ENUM_CURRENT_SETTINGS, ref dm);

                return dm;
            }
            
            public static int ChangeScreenConfigurationV(DEVMODE dm, int width, int height, int refreshRate)
            {
                if (width == 0 && height == 0 && refreshRate == 0)
                {
                    logger.Debug($"Nothing to set. Width, height and refresh rate is 0");
                    return 0;
                }

                logger.Debug($"Setting configuration of device \"{dm.dmDeviceName}\" to {width}x{height} and refresh rate {refreshRate}...");
                if (User_32.EnumDisplaySettings(null, User_32.ENUM_CURRENT_SETTINGS, ref dm))
                {
                    if (width != 0 && height != 0)
                    {
                        dm.dmPelsWidth = width;
                        dm.dmPelsHeight = height;
                    }

                    if (refreshRate != 0)
                    {
                        dm.dmDisplayFrequency = refreshRate;
                    }

                    int iRet = User_32.ChangeDisplaySettings(ref dm, User_32.CDS_TEST);
                    if (iRet == User_32.DISP_CHANGE_FAILED)
                    {
                        logger.Info($"Failed to set resolution DISP_CHANGE_FAILED");
                        return -1;
                    }
                    else
                    {
                        iRet = User_32.ChangeDisplaySettings(ref dm, User_32.CDS_UPDATEREGISTRY);
                        switch (iRet)
                        {
                            case User_32.DISP_CHANGE_SUCCESSFUL:
                                logger.Info($"Resolution set to {width}x{height} succesfully");
                                return 0;
                            case User_32.DISP_CHANGE_RESTART:
                                logger.Info($"Failed to set resolution DISP_CHANGE_RESTART");
                                return 1;
                            default:
                                logger.Info($"Failed to set resolution (default)");
                                return -1;
                        }
                    }
                }
                else
                {
                    logger.Info($"Failed to set resolution. EnumDisplaySettings returned false");
                    return -1;
                }
            }

            public static bool ChangeDisplayConfiguration(DEVMODE devMode, int width, int height, int refreshRate)
            {
                return ChangeScreenConfigurationV(devMode, width, height, refreshRate) == 0;
            }

            public static bool RestoreDisplayConfiguration(DisplayConfigChangeData displayRestoreData)
            {
                if (!displayRestoreData.ResolutionChanged && !displayRestoreData.RefreshRateChanged)
                {
                    return true;
                }

                var width = 0;
                var height = 0;
                var frequency = 0;

                if (displayRestoreData.ResolutionChanged)
                {
                    width = displayRestoreData.DevMode.dmPelsWidth;
                    height = displayRestoreData.DevMode.dmPelsHeight;
                }

                if (displayRestoreData.RefreshRateChanged)
                {
                    frequency = displayRestoreData.DevMode.dmDisplayFrequency;
                }

                return ChangeScreenConfigurationV(displayRestoreData.DevMode, width, height, frequency) == 0;
            }

            private static DEVMODE GetDevMode()
            {
                DEVMODE dm = new DEVMODE();
                dm.dmDeviceName = new string(new char[32]);
                dm.dmFormName = new string(new char[32]);
                dm.dmSize = (short)Marshal.SizeOf(dm);
                return dm;
            }

            public static List<DEVMODE> GetScreenAvailableDevModes(DEVMODE dm)
            {
                var list = new List<DEVMODE>();
                int i = 0;
                try
                {
                    while (User_32.EnumDisplaySettings(null, i, ref dm))
                    {
                        var width = dm.dmPelsWidth;
                        var height = dm.dmPelsHeight;
                        if (width != 0 && height != 0)
                        {
                            list.Add(dm);
                        }

                        i++;
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error while obtaining display resolutions");
                }

                return list;
            }

            public static List<DEVMODE> GetMainScreenAvailableDevModes()
            {
                return GetScreenAvailableDevModes(GetMainScreenDevMode());
            }

            public static string GetResolutionAspectRatio(int width, int height)
            {
                int Remainder;

                var a = width;
                var b = height;
                while (b != 0)
                {
                    Remainder = a % b;
                    a = b;
                    b = Remainder;
                }

                var gcd = a;
                return $"{width/gcd}:{height/gcd}";
            }
        }
    }
}