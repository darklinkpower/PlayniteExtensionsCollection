using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Common.Attributes;

namespace VNDBMetadata.VndbDomain.Aggregates.StaffAggregate
{
    public enum StaffRequestSortEnum
    {
        [StringRepresentation(StaffConstants.RequestSort.Id)]
        Id,
        [StringRepresentation(StaffConstants.RequestSort.Name)]
        Name,
        [StringRepresentation(StaffConstants.RequestSort.SearchRank)]
        SearchRank
    }
}