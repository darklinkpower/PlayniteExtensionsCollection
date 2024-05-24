using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;

namespace VndbApiInfrastructure.TagAggregate
{
    public enum TagRequestSortEnum
    {
        [StringRepresentation(TagConstants.RequestSort.Id)]
        Id,
        [StringRepresentation(TagConstants.RequestSort.Name)]
        Name,
        [StringRepresentation(TagConstants.RequestSort.VisualNovelCount)]
        VnCount,
        [StringRepresentation(TagConstants.RequestSort.SearchRank)]
        SearchRank
    }
}