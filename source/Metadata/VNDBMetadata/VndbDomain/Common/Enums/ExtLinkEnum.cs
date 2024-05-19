using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Common.Attributes;
using VNDBMetadata.VndbDomain.Common.Constants;

namespace VNDBMetadata.VndbDomain.Common.Enums
{
    public enum ExtLinkEnum
    {
        [StringRepresentation(ExtLinks.Release.Steam)]
        Steam,
        [StringRepresentation(ExtLinks.Release.JASTUSA)]
        JastUsa
    }
}