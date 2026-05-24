using DisplayHelper.Domain.Displays.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WinApi.Structs;

namespace DisplayHelper.Infrastructure.Win32.Mapping
{
    internal static class DisplayModeMapper
    {
        public static DisplayMode Map(
            DEVMODE devMode)
        {
            return new DisplayMode(
                new Resolution(
                    devMode.dmPelsWidth,
                    devMode.dmPelsHeight),

                new RefreshRate(
                    devMode.dmDisplayFrequency));
        }
    }
}
