using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Attributes;

namespace VNDBFuze.VndbDomain.Aggregates.TraitAggregate
{
    [Flags]
    public enum TraitRequestFieldsFlags
    {
        None = 0,
        [StringRepresentation(TraitConstants.Fields.Id)]
        Id = 1 << 0,
        [StringRepresentation(TraitConstants.Fields.Name)]
        Name = 1 << 1,
        [StringRepresentation(TraitConstants.Fields.Aliases)]
        Aliases = 1 << 2,
        [StringRepresentation(TraitConstants.Fields.Description)]
        Description = 1 << 3,
        [StringRepresentation(TraitConstants.Fields.Searchable)]
        Searchable = 1 << 4,
        [StringRepresentation(TraitConstants.Fields.Applicable)]
        Applicable = 1 << 5,
        [StringRepresentation(TraitConstants.Fields.GroupId)]
        GroupId = 1 << 6,
        [StringRepresentation(TraitConstants.Fields.GroupName)]
        GroupName = 1 << 7,
        [StringRepresentation(TraitConstants.Fields.CharCount)]
        CharCount = 1 << 8
    }


}
