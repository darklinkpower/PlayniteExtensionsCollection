using System.Collections.Generic;
using static WinApi.Flags;

namespace DisplayHelper.Native
{
    /// <summary>
    /// Desktop bounds of each active display, in physical pixels.
    /// </summary>
    public static class MonitorLayout
    {
        public sealed class Bounds
        {
            public string GdiName { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public bool IsPrimary { get; set; }
            public double CenterX => X + Width / 2.0;
            public double CenterY => Y + Height / 2.0;
        }

        public static List<Bounds> GetBounds()
        {
            var result = new List<Bounds>();
            foreach (var device in DisplayUtilities.GetAvailableDisplayDevices())
            {
                var devMode = DisplayUtilities.GetScreenDevMode(device.DeviceName);
                if (devMode.dmPelsWidth == 0 || devMode.dmPelsHeight == 0)
                {
                    continue;
                }

                result.Add(new Bounds
                {
                    GdiName = device.DeviceName,
                    X = devMode.dmPosition.x,
                    Y = devMode.dmPosition.y,
                    Width = devMode.dmPelsWidth,
                    Height = devMode.dmPelsHeight,
                    IsPrimary = device.StateFlags.HasFlag(DisplayDeviceStateFlags.PrimaryDevice)
                });
            }

            return result;
        }
    }
}
