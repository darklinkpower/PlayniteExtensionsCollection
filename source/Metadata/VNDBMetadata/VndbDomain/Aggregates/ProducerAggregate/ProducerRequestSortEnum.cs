using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Common.Attributes;

namespace VNDBMetadata.VndbDomain.Aggregates.ProducerAggregate
{
    public enum ProducerRequestSortEnum
    {
        [StringRepresentation(ProducerConstants.ProducerSort.Id)]
        Id,
        [StringRepresentation(ProducerConstants.ProducerSort.Name)]
        Name,
        [StringRepresentation(ProducerConstants.ProducerSort.SearchRank)]
        SearchRank
    }
}