using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Attributes;
using VNDBFuze.VndbDomain.Common.Constants;

namespace VNDBFuze.VndbDomain.Common.Enums
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