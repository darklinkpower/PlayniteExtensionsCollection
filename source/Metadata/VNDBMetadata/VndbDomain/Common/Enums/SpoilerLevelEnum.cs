using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Common.Attributes;
using VNDBMetadata.VndbDomain.Common.Constants;

namespace VNDBMetadata.VndbDomain.Common.Enums
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