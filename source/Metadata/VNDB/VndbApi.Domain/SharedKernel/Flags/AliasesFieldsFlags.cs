using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApi.Domain.SharedKernel;

namespace VndbApi.Domain.SharedKernel
{
    [Flags]
    public enum AliasesFieldsFlags
    {
        /// <summary>
        /// Integer, alias id.
        /// </summary>
        [StringRepresentation(Aliases.Fields.Aid)]
        Aid = 1 << 0,

        /// <summary>
        /// String, name in original script.
        /// </summary>
        [StringRepresentation(Aliases.Fields.Name)]
        Name = 1 << 1,

        /// <summary>
        /// String, possibly null, romanized version of ‘name’.
        /// </summary>
        [StringRepresentation(Aliases.Fields.Latin)]
        Latin = 1 << 2,

        /// <summary>
        /// Boolean, whether this alias is used as “main” name for the staff entry.
        /// </summary>
        [StringRepresentation(Aliases.Fields.IsMain)]
        IsMain = 1 << 3
    }

}