using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;

namespace VndbApiInfrastructure.ProducerAggregate
{
    public enum ProducerRequestSortEnum
    {
        [StringRepresentation(ProducerConstants.RequestSort.Id)]
        Id,
        [StringRepresentation(ProducerConstants.RequestSort.Name)]
        Name,
        [StringRepresentation(ProducerConstants.RequestSort.SearchRank)]
        SearchRank
    }
}