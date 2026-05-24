using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Domain.Displays.ValueObjects
{
    public sealed class RefreshRate : IEquatable<RefreshRate>
    {
        public int Value { get; }

        public RefreshRate(int value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"{Value}Hz";
        }

        public bool Equals(RefreshRate other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RefreshRate);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(
            RefreshRate left,
            RefreshRate right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(
            RefreshRate left,
            RefreshRate right)
        {
            return !Equals(left, right);
        }
    }
}
