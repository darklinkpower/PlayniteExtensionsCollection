using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Common.Attributes;

namespace VNDBMetadata.VndbDomain.Aggregates.TagAggregate
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