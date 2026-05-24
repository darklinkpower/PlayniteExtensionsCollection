using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static WinApi.Structs;

namespace DisplayHelper.Infrastructure.Win32.Native
{
    public static class DevModeFactory
    {
        public static DEVMODE Create()
        {
            return new DEVMODE
            {
                dmDeviceName = new string('\0', 32),
                dmFormName = new string('\0', 32),
                dmSize = (short)Marshal.SizeOf(typeof(DEVMODE))
            };
        }
    }
}
