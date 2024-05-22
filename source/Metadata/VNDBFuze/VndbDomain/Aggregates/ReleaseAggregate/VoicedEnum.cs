using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Attributes;

namespace VNDBFuze.VndbDomain.Aggregates.ReleaseAggregate
{
    public enum VoicedEnum
    {
        /// <summary>
        /// Not voiced.
        /// </summary>
        [IntRepresentation(ReleaseConstants.Voiced.NotVoiced)]
        NotVoiced,

        /// <summary>
        /// Only ero scenes voiced.
        /// </summary>
        [IntRepresentation(ReleaseConstants.Voiced.EroScenesVoiced)]
        EroScenesVoiced,

        /// <summary>
        /// Partially voiced.
        /// </summary>
        [IntRepresentation(ReleaseConstants.Voiced.PartiallyVoiced)]
        PartiallyVoiced,

        /// <summary>
        /// Fully voiced.
        /// </summary>
        [IntRepresentation(ReleaseConstants.Voiced.FullyVoiced)]
        FullyVoiced
    }
}
