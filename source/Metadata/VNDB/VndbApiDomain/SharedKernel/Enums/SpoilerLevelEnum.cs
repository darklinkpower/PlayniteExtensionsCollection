using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;

namespace VndbApiDomain.SharedKernel
{
    public enum SpoilerLevelEnum
    {
        [IntRepresentation(SpoilerLevel.None)]
        None,
        [IntRepresentation(SpoilerLevel.Minimum)]
        Minimum,
        [IntRepresentation(SpoilerLevel.Major)]
        Major
    }
}