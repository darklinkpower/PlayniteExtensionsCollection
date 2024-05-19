using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.VndbDomain.Aggregates.CharacterAggregate
{
    [Flags]
    public enum CharacterRequestsFieldsFlags
    {
        None = 0,
        Id = 1 << 0,
        Name = 1 << 1,
        Original = 1 << 2,
        Aliases = 1 << 3,
        Description = 1 << 4,
        Image = 1 << 5,
        BloodType = 1 << 6,
        Height = 1 << 7,
        Weight = 1 << 8,
        Bust = 1 << 9,
        Waist = 1 << 10,
        Hips = 1 << 11,
        Cup = 1 << 12,
        Age = 1 << 13,
        Birthday = 1 << 14,
        Sex = 1 << 15,
        Vns = 1 << 16,
        VnsSpoiler = 1 << 17,
        VnsRole = 1 << 18,
        Traits = 1 << 19,
        TraitsSpoiler = 1 << 20,
        TraitsLie = 1 << 21
    }

    // Excluded fields: 
    // image.*, vns.*, vns.release.*, traits.*
}
