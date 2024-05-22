using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Attributes;

namespace VNDBFuze.VndbDomain.Aggregates.TagAggregate
{
    public enum TagRequestSortEnum
    {
        [StringRepresentation(TagConstants.RequestSort.Id)]
        Id,
        [StringRepresentation(TagConstants.RequestSort.Name)]
        Name,
        [StringRepresentation(TagConstants.RequestSort.VnCount)]
        VnCount,
        [StringRepresentation(TagConstants.RequestSort.SearchRank)]
        SearchRank
    }
}