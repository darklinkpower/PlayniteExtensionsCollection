using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDB.ApiConstants
{
    [Flags]
    public enum OperatorFlags : uint
    {
        None = 0x0000,
        [FilterFlag("o")]
        Ordering = 0x0001,
        [FilterFlag("n")]
        NullAccepting = 0x0002,
        [FilterFlag("m")]
        MultiEntryMatch = 0x0004,
        [FilterFlag("i")]
        Invertible = 0x0008
    }

    [Flags]
    public enum FilterFlags
    {
        None = 0,
        Ordering = 1 << 0,
        NullAccepting = 1 << 1,
        MultiEntryMatch = 1 << 2,
        Invertible = 1 << 3
    }

    public enum ProducerFilters
    {
        Id = FilterFlags.Ordering | FilterFlags.NullAccepting,
        Search = FilterFlags.MultiEntryMatch | FilterFlags.Invertible,
        Lang = FilterFlags.NullAccepting,
        Type = FilterFlags.Invertible
    }

    public class FilterFlagAttribute : Attribute
    {
        public string Value { get; }

        public FilterFlagAttribute(string value)
        {
            Value = value;
        }
    }
}