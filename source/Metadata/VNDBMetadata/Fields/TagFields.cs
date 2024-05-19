using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.Fields
{
    [Flags]
    public enum TagFields
    {
        None = 0,
        Id = 1 << 0,
        Name = 1 << 1,
        Aliases = 1 << 2,
        Description = 1 << 3,
        Category = 1 << 4,
        Searchable = 1 << 5,
        Applicable = 1 << 6,
        VnCount = 1 << 7
    }
}
