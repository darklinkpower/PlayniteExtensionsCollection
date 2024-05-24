using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApi.Domain.SharedKernel;

namespace VndbApi.Domain.VisualNovelAggregate
{
    /// <summary>
    /// Enum representing different lengths of visual novels.
    /// </summary>
    public enum VnLengthEnum
    {
        /// <summary>
        /// Indicates a very short visual novel.
        /// </summary>
        [IntRepresentation(VisualNovelConstants.Length.VeryShort)]
        VeryShort,

        /// <summary>
        /// Indicates a short visual novel.
        /// </summary>
        [IntRepresentation(VisualNovelConstants.Length.Short)]
        Short,

        /// <summary>
        /// Indicates a medium-length visual novel.
        /// </summary>
        [IntRepresentation(VisualNovelConstants.Length.Medium)]
        Medium,

        /// <summary>
        /// Indicates a long visual novel.
        /// </summary>
        [IntRepresentation(VisualNovelConstants.Length.Long)]
        Long,

        /// <summary>
        /// Indicates a very long visual novel.
        /// </summary>
        [IntRepresentation(VisualNovelConstants.Length.VeryLong)]
        VeryLong
    }
}