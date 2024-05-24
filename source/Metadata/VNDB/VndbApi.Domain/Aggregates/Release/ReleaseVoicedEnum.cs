using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApi.Domain.SharedKernel;

namespace VndbApi.Domain.ReleaseAggregate
{
    public enum ReleaseVoicedEnum
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        [IntRepresentation(ReleaseConstants.Voiced.NotVoiced)]
        Unknown,

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
