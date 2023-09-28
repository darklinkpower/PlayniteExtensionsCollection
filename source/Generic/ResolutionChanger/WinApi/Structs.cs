using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static WinApi.Enums;
using static WinApi.Flags;

namespace WinApi
{
    public static class Structs
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DISPLAY_DEVICE
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public DisplayDeviceStateFlags StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTL
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
        public struct DEVMODE
        {
            public const int CCHDEVICENAME = 32;
            public const int CCHFORMNAME = 32;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            [FieldOffset(0)]
            public string dmDeviceName;
            [FieldOffset(32)]
            public short dmSpecVersion;
            [FieldOffset(34)]
            public short dmDriverVersion;
            [FieldOffset(36)]
            public short dmSize;
            [FieldOffset(38)]
            public short dmDriverExtra;
            [FieldOffset(40)]
            public DeviceModeFieldsFlags dmFields;

            [FieldOffset(44)]
            short dmOrientation;
            [FieldOffset(46)]
            short dmPaperSize;
            [FieldOffset(48)]
            short dmPaperLength;
            [FieldOffset(50)]
            short dmPaperWidth;
            [FieldOffset(52)]
            short dmScale;
            [FieldOffset(54)]
            short dmCopies;
            [FieldOffset(56)]
            short dmDefaultSource;
            [FieldOffset(58)]
            short dmPrintQuality;

            [FieldOffset(44)]
            public POINTL dmPosition;
            [FieldOffset(52)]
            public ScreenOrientation dmDisplayOrientation;
            [FieldOffset(56)]
            public int dmDisplayFixedOutput;

            [FieldOffset(60)]
            public short dmColor;
            [FieldOffset(62)]
            public short dmDuplex;
            [FieldOffset(64)]
            public short dmYResolution;
            [FieldOffset(66)]
            public short dmTTOption;
            [FieldOffset(68)]
            public short dmCollate;
            [FieldOffset(72)]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;
            [FieldOffset(102)]
            public short dmLogPixels;
            [FieldOffset(104)]
            public int dmBitsPerPel;
            [FieldOffset(108)]
            public int dmPelsWidth;
            [FieldOffset(112)]
            public int dmPelsHeight;
            [FieldOffset(116)]
            public int dmDisplayFlags;
            [FieldOffset(116)]
            public int dmNup;
            [FieldOffset(120)]
            public int dmDisplayFrequency;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DEVMODE2
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            [MarshalAs(UnmanagedType.U2)]
            public ushort dmSpecVersion;
            [MarshalAs(UnmanagedType.U2)]
            public ushort dmDriverVersion;
            [MarshalAs(UnmanagedType.U2)]
            public ushort dmSize;
            [MarshalAs(UnmanagedType.U2)]
            public ushort dmDriverExtra;
            [MarshalAs(UnmanagedType.U4)]
            public uint dmFields;
            [MarshalAs(UnmanagedType.Struct)]
            public POINTL dmPosition;
            [MarshalAs(UnmanagedType.U4)]
            public ScreenOrientation dmDisplayOrientation;
            [MarshalAs(UnmanagedType.U4)]
            public uint dmDisplayFixedOutput;
            [MarshalAs(UnmanagedType.I2)]
            public short dmColor;
            [MarshalAs(UnmanagedType.I2)]
            public short dmDuplex;
            [MarshalAs(UnmanagedType.I2)]
            public short dmYResolution;
            [MarshalAs(UnmanagedType.I2)]
            public short dmTTOption;
            [MarshalAs(UnmanagedType.I2)]
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            [MarshalAs(UnmanagedType.U2)]
            public ushort dmLogPixels;
            [MarshalAs(UnmanagedType.U4)]
            public uint dmBitsPerPel;
            [MarshalAs(UnmanagedType.U4)]
            public uint dmPelsWidth;
            [MarshalAs(UnmanagedType.U4)]
            public uint dmPelsHeight;
            [MarshalAs(UnmanagedType.U4)]
            public uint dmDisplayFlags;
            [MarshalAs(UnmanagedType.U4)]
            public uint dmDisplayFrequency;
            [MarshalAs(UnmanagedType.U4)]
            public uint dmICMMethod;
            [MarshalAs(UnmanagedType.U4)]
            public uint dmICMIntent;
            [MarshalAs(UnmanagedType.U4)]
            public uint dmMediaType;
            [MarshalAs(UnmanagedType.U4)]
            public uint dmDitherType;
            [MarshalAs(UnmanagedType.U4)]
            public uint dmReserved1;
            [MarshalAs(UnmanagedType.U4)]
            public uint dmReserved2;
            [MarshalAs(UnmanagedType.U4)]
            public uint dmPanningWidth;
            [MarshalAs(UnmanagedType.U4)]
            public uint dmPanningHeight;
        }
    }
}
