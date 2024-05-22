using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Attributes;

namespace VNDBFuze.VndbDomain.Aggregates.TagAggregate
{
    [Flags]
    public enum TagRequestFieldsFlags
    {
        None = 0,
        [StringRepresentation(TagConstants.Fields.Id)]
        Id = 1 << 0,
        [StringRepresentation(TagConstants.Fields.Name)]
        Name = 1 << 1,
        [StringRepresentation(TagConstants.Fields.Aliases)]
        Aliases = 1 << 2,
        [StringRepresentation(TagConstants.Fields.Description)]
        Description = 1 << 3,
        [StringRepresentation(TagConstants.Fields.Category)]
        Category = 1 << 4,
        [StringRepresentation(TagConstants.Fields.Searchable)]
        Searchable = 1 << 5,
        [StringRepresentation(TagConstants.Fields.Applicable)]
        Applicable = 1 << 6,
        [StringRepresentation(TagConstants.Fields.VnCount)]
        VnCount = 1 << 7
    }
}
