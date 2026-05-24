using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Domain.Displays.ValueObjects
{
    public sealed class DisplayPosition : IEquatable<DisplayPosition>
    {
        public int X { get; }

        public int Y { get; }

        public DisplayPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return string.Format(
                "X={0}, Y={1}",
                X,
                Y);
        }

        public bool Equals(DisplayPosition other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return X == other.X &&
                   Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DisplayPosition);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public static bool operator ==(
            DisplayPosition left,
            DisplayPosition right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(
            DisplayPosition left,
            DisplayPosition right)
        {
            return !Equals(left, right);
        }
    }
}
