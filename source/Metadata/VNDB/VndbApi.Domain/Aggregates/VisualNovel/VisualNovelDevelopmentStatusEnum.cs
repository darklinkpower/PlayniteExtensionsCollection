using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApi.Domain.SharedKernel;

namespace VndbApi.Domain.VisualNovelAggregate
{
    /// <summary>
    /// Enum representing different development statuses of visual novels.
    /// </summary>
    public enum VisualNovelDevelopmentStatusEnum
    {
        /// <summary>
        /// Indicates that the visual novel is finished.
        /// </summary>
        [IntRepresentation(VisualNovelConstants.DevelopmentStatus.Finished)]
        Finished,

        /// <summary>
        /// Indicates that the visual novel is in development.
        /// </summary>
        [IntRepresentation(VisualNovelConstants.DevelopmentStatus.InDevelopment)]
        InDevelopment,

        /// <summary>
        /// Indicates that the visual novel has been cancelled.
        /// </summary>
        [IntRepresentation(VisualNovelConstants.DevelopmentStatus.Cancelled)]
        Cancelled
    }
}