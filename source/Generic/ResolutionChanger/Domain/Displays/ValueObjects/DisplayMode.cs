using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Domain.Displays.ValueObjects
{
    public sealed class DisplayMode : IEquatable<DisplayMode>
    {
        public Resolution Resolution { get; }

        public RefreshRate RefreshRate { get; }

        public DisplayMode(
            Resolution resolution,
            RefreshRate refreshRate)
        {
            Resolution = resolution;
            RefreshRate = refreshRate;
        }

        public override string ToString()
        {
            return string.Format(
                "{0} @ {1}",
                Resolution,
                RefreshRate);
        }

        public bool Equals(DisplayMode other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(Resolution, other.Resolution) &&
                   Equals(RefreshRate, other.RefreshRate);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DisplayMode);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                hash = (hash * 23) + (Resolution != null ? Resolution.GetHashCode() : 0);
                hash = (hash * 23) + (RefreshRate != null ? RefreshRate.GetHashCode() : 0);

                return hash;
            }
        }

        public static bool operator ==(
            DisplayMode left,
            DisplayMode right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(
            DisplayMode left,
            DisplayMode right)
        {
            return !Equals(left, right);
        }
    }
}
