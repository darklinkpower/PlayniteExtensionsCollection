using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApi.Domain.SharedKernel;

namespace VndbApi.Infrastructure.ReleaseAggregate
{
    public enum ReleaseRequestSortEnum
    {
        [StringRepresentation(ReleaseConstants.RequestSort.Id)]
        Id,
        [StringRepresentation(ReleaseConstants.RequestSort.Title)]
        Title,
        [StringRepresentation(ReleaseConstants.RequestSort.Released)]
        Released,
        [StringRepresentation(ReleaseConstants.RequestSort.SearchRank)]
        SearchRank
    }
}