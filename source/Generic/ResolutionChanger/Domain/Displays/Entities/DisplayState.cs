using DisplayHelper.Domain.Displays.ValueObjects;
using DisplayHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Domain.Displays.Entities
{
    public sealed class DisplayState
    {
        public DisplayMode Mode { get; }

        public DisplayPosition Position { get; }

        public DisplayOrientation Orientation { get; }

        public DisplayScaling Scaling { get; }

        public DisplayState(
            DisplayMode mode,
            DisplayPosition position,
            DisplayOrientation orientation,
            DisplayScaling scaling)
        {
            Mode = mode;
            Position = position;
            Orientation = orientation;
            Scaling = scaling;
        }

        public override string ToString()
        {
            return string.Format(
                $"{nameof(Mode)}={Mode}, {nameof(Position)}={Position}, {nameof(Orientation)}={Orientation}, {nameof(Scaling)}={Scaling}",
                Mode,
                Position,
                Orientation,
                Scaling);
        }

        public override bool Equals(object obj)
        {
            if (obj is DisplayState other)
            {
                //return Equals(Resolution, other.Resolution) &&
                //       Equals(RefreshRate, other.RefreshRate);
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                //hash = (hash * 23) + (Resolution != null ? Resolution.GetHashCode() : 0);
                //hash = (hash * 23) + (RefreshRate != null ? RefreshRate.GetHashCode() : 0);

                return hash;
            }
        }

        public static bool operator ==(DisplayState left, DisplayState right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DisplayState left, DisplayState right)
        {
            return !Equals(left, right);
        }
    }
}
