using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Attributes;

namespace VNDBFuze.VndbDomain.Aggregates.VnAggregate
{
    /// <summary>
    /// Enum representing different development statuses of visual novels.
    /// </summary>
    public enum VnDevelopmentStatusEnum
    {
        /// <summary>
        /// Indicates that the visual novel is finished.
        /// </summary>
        [IntRepresentation(VnConstants.VnDevelopmentStatus.Finished)]
        Finished,

        /// <summary>
        /// Indicates that the visual novel is in development.
        /// </summary>
        [IntRepresentation(VnConstants.VnDevelopmentStatus.InDevelopment)]
        InDevelopment,

        /// <summary>
        /// Indicates that the visual novel has been cancelled.
        /// </summary>
        [IntRepresentation(VnConstants.VnDevelopmentStatus.Cancelled)]
        Cancelled
    }
}