using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ResolutionChanger
{
    class DisplayUtilities
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
            public string dmDisplayOrientation;
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

        class User_32
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
            public static int ChangeResolution(int width, int height)
            {

                DEVMODE dm = GetDevMode1();

                if (User_32.EnumDisplaySettings(null, User_32.ENUM_CURRENT_SETTINGS, ref dm))
                {

                    dm.dmPelsWidth = width;
                    dm.dmPelsHeight = height;

                    int iRet = User_32.ChangeDisplaySettings(ref dm, User_32.CDS_TEST);

                    if (iRet == User_32.DISP_CHANGE_FAILED)
                    {
                        return -1;
                    }
                    else
                    {
                        iRet = User_32.ChangeDisplaySettings(ref dm, User_32.CDS_UPDATEREGISTRY);
                        switch (iRet)
                        {
                            case User_32.DISP_CHANGE_SUCCESSFUL:
                                {
                                    return 0;
                                }
                            case User_32.DISP_CHANGE_RESTART:
                                {
                                    return 1;
                                }
                            default:
                                {
                                    return -1;
                                }
                        }

                    }


                }
                else
                {
                    return -1;
                }
            }

            private static DEVMODE GetDevMode1()
            {
                DEVMODE dm = new DEVMODE();
                dm.dmDeviceName = new string(new char[32]);
                dm.dmFormName = new string(new char[32]);
                dm.dmSize = (short)Marshal.SizeOf(dm);
                return dm;
            }

            public static List<KeyValuePair<int, int>> GetPossibleResolutions()
            {
                var list = new List<KeyValuePair<int, int>>();
                DEVMODE dm = new DEVMODE();
                int i = 0;
                try
                {
                    while (User_32.EnumDisplaySettings(null, i, ref dm))
                    {
                        var width = dm.dmPelsWidth;
                        var height = dm.dmPelsHeight;
                        if (width != 0 && height != 0)
                        {
                            list.Add(new KeyValuePair<int, int>(dm.dmPelsWidth, dm.dmPelsHeight));
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
