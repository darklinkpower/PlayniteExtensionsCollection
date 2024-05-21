using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Common.Attributes;

namespace VNDBMetadata.VndbDomain.Aggregates.VnAggregate
{
    /// <summary>
    /// Enum representing different lengths of visual novels.
    /// </summary>
    public enum VnLengthEnum
    {
        /// <summary>
        /// Indicates a very short visual novel.
        /// </summary>
        [IntRepresentation(VnConstants.VnLength.VeryShort)]
        VeryShort,

        /// <summary>
        /// Indicates a short visual novel.
        /// </summary>
        [IntRepresentation(VnConstants.VnLength.Short)]
        Short,

        /// <summary>
        /// Indicates a medium-length visual novel.
        /// </summary>
        [IntRepresentation(VnConstants.VnLength.Medium)]
        Medium,

        /// <summary>
        /// Indicates a long visual novel.
        /// </summary>
        [IntRepresentation(VnConstants.VnLength.Long)]
        Long,

        /// <summary>
        /// Indicates a very long visual novel.
        /// </summary>
        [IntRepresentation(VnConstants.VnLength.VeryLong)]
        VeryLong
    }
}