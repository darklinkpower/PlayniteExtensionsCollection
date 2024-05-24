using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;

namespace VndbApiInfrastructure.TraitAggregate
{
    public enum TraitRequestSortEnum
    {
        [StringRepresentation(TraitConstants.RequestSort.Id)]
        Id,
        [StringRepresentation(TraitConstants.RequestSort.Name)]
        Name,
        [StringRepresentation(TraitConstants.RequestSort.CharCount)]
        CharCount,
        [StringRepresentation(TraitConstants.RequestSort.SearchRank)]
        SearchRank
    }
}