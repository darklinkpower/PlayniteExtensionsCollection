using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Attributes;

namespace VNDBFuze.VndbDomain.Aggregates.StaffAggregate
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