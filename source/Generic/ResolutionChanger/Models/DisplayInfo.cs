using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Models
{
    public class DisplayMode : ObservableObject, IEquatable<DisplayMode>
    {
        private int width;
        public int Width { get => width; set => SetValue(ref width, value); }

        private int height;
        public int Height { get => height; set => SetValue(ref height, value); }

        private int displayFrenquency;
        public int DisplayFrenquency { get => displayFrenquency; set => SetValue(ref displayFrenquency, value); }

        public string AspectRatio => CalculateAspectRatioString();

        public string DisplayName => $"{width}x{height} ({AspectRatio}) {DisplayFrenquency}Hz";

        public DisplayMode(int width, int height, int displayFrenquency)
        {
            Width = width;
            Height = height;
            DisplayFrenquency = displayFrenquency;
        }

        private string CalculateAspectRatioString()
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

        public override bool Equals(object obj)
        {
            if (obj is DisplayMode displayMode)
            {
                return Equals(displayMode);
            }
            else
            {
                return false;
            }
        }

        public static bool operator ==(DisplayMode obj1, DisplayMode obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(DisplayMode obj1, DisplayMode obj2)
        {
            return !obj1.Equals(obj2);
        }

        public bool Equals(DisplayMode other)
        {
            return Width == other.Width &&
                Height == other.Height &&
                DisplayFrenquency == other.DisplayFrenquency;
        }

        public override int GetHashCode()
        {
            return Width.GetHashCode() ^
                Height.GetHashCode() ^
                DisplayFrenquency.GetHashCode();
        }
    }

    public class DisplayInfo : ObservableObject
    {
        public readonly string DeviceName;
        public readonly string DeviceString;
        public readonly string DeviceID;
        public readonly string DeviceKey;
        public readonly List<DisplayMode> DisplayModes = new List<DisplayMode>();
        public string DisplayName => $"{DeviceString} - {DeviceName}";

        public DisplayInfo(string deviceName, string deviceString, string deviceID, string deviceKey, List<DisplayMode> displayModes)
        {
            DeviceName = deviceName;
            DeviceString = deviceString;
            DeviceID = deviceID;
            DeviceKey = deviceKey;
            DisplayModes = displayModes;
        }
    }
}