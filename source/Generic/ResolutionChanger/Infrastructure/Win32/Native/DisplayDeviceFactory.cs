using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static WinApi.Structs;

namespace DisplayHelper.Infrastructure.Win32.Native
{
    internal static class DisplayDeviceFactory
    {
        public static DISPLAY_DEVICE Create()
        {
            return new DISPLAY_DEVICE
            {
                cb = Marshal.SizeOf<DISPLAY_DEVICE>()
            };
        }
    }
}
