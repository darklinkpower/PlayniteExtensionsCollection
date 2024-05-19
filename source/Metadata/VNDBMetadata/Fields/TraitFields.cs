using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.Fields
{
    [Flags]
    public enum TraitFields
    {
        None = 0,
        Id = 1 << 0,
        Name = 1 << 1,
        Aliases = 1 << 2,
        Description = 1 << 3,
        Searchable = 1 << 4,
        Applicable = 1 << 5,
        GroupId = 1 << 6,
        GroupName = 1 << 7,
        CharCount = 1 << 8
    }

}
