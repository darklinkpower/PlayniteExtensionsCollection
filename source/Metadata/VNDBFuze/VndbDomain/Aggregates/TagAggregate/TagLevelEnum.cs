using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Attributes;

namespace VNDBFuze.VndbDomain.Aggregates.TagAggregate
{
    /// <summary>
    /// Enum representing different levels of tags.
    /// </summary>
    public enum TagLevelEnum
    {
        /// <summary>
        /// Indicates level 0.
        /// </summary>
        [IntRepresentation(TagConstants.TagLevel.Zero)]
        Zero,
        /// <summary>
        /// Indicates level 1.
        /// </summary>
        [IntRepresentation(TagConstants.TagLevel.One)]
        One,
        /// <summary>
        /// Indicates level 2.
        /// </summary>
        [IntRepresentation(TagConstants.TagLevel.Two)]
        Two,
        /// <summary>
        /// Indicates level 3.
        /// </summary>
        [IntRepresentation(TagConstants.TagLevel.Three)]
        Three
    }
}