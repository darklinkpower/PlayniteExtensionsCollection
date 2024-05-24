using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApi.Domain.SharedKernel;
using VndbApi.Domain.StaffAggregate;

namespace VndbApi.Infrastructure.StaffAggregate
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