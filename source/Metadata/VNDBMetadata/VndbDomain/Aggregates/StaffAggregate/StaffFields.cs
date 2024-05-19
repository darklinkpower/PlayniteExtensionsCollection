using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.Fields
{
    [Flags]
    public enum StaffFields
    {
        None = 0,
        Id = 1 << 0,
        Aid = 1 << 1,
        IsMain = 1 << 2,
        Name = 1 << 3,
        Original = 1 << 4,
        Lang = 1 << 5,
        Gender = 1 << 6,
        Description = 1 << 7,
        ExtLinks = 1 << 8,
        Aliases = 1 << 9,
        AliasesAid = 1 << 10,
        AliasesName = 1 << 11,
        AliasesLatin = 1 << 12,
        AliasesIsMain = 1 << 13
    }

}
