using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class CharExtensions
    {
        public static bool EqualsIgnoreCase(this char char1, char char2)
        {
            return char.ToUpperInvariant(char1) == char.ToUpperInvariant(char2);
        }
    }
}