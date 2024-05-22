using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Attributes;

namespace VNDBFuze.VndbDomain.Aggregates.TagAggregate
{
    /// <summary>
    /// Enum representing tag categories.
    /// </summary>
    public enum TagCategoryEnum
    {
        /// <summary>
        /// Content category.
        /// </summary>
        [StringRepresentation(TagConstants.TagCategory.Content)]
        Content,

        /// <summary>
        /// Sexual content category.
        /// </summary>
        [StringRepresentation(TagConstants.TagCategory.SexualContent)]
        SexualContent,

        /// <summary>
        /// Technical tags category.
        /// </summary>
        [StringRepresentation(TagConstants.TagCategory.Technical)]
        Technical
    }
}