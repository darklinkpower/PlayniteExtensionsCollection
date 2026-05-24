using DisplayHelper.Domain.Displays.Entities;
using DisplayHelper.Domain.Displays.ValueObjects;
using DisplayHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WinApi.Enums;
using static WinApi.Structs;

namespace DisplayHelper.Infrastructure.Win32.Mapping
{
    internal static class DevModeMapper
    {
        public static DisplayState Map(DEVMODE devMode)
        {
            var mode =
                new DisplayMode(
                    new Resolution(
                        devMode.dmPelsWidth,
                        devMode.dmPelsHeight),

                    new RefreshRate(
                        devMode.dmDisplayFrequency));

            var position =
                new DisplayPosition(
                    devMode.dmPosition.x,
                    devMode.dmPosition.y);

            var orientation =
                MapOrientation(devMode.dmDisplayOrientation);

            var scaling =
                MapScaling(devMode.dmDisplayFixedOutput);

            return new DisplayState(
                mode,
                position,
                orientation,
                scaling);
        }

        private static DisplayOrientation MapOrientation(
            ScreenOrientation orientation)
        {
            switch (orientation)
            {
                case ScreenOrientation.Angle0:
                    return DisplayOrientation.Landscape;

                case ScreenOrientation.Angle90:
                    return DisplayOrientation.Portrait;

                case ScreenOrientation.Angle180:
                    return DisplayOrientation.LandscapeFlipped;

                case ScreenOrientation.Angle270:
                    return DisplayOrientation.PortraitFlipped;

                default:
                    return DisplayOrientation.Landscape;
            }
        }

        private static DisplayScaling MapScaling(
            int scaling)
        {
            switch (scaling)
            {
                case 1:
                    return DisplayScaling.Centered;

                case 2:
                    return DisplayScaling.Stretched;

                default:
                    return DisplayScaling.Default;
            }
        }

    }
}
