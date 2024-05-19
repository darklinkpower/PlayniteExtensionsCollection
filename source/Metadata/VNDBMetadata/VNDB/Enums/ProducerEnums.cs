using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDB.ApiConstants;

namespace VNDBMetadata.VNDB.Enums
{
    public enum ProducerTypeEnum
    {
        [StringRepresentation(ProducerConstants.ProducerType.Company)]
        Company,
        [StringRepresentation(ProducerConstants.ProducerType.Individual)]
        Individual,
        [StringRepresentation(ProducerConstants.ProducerType.AmateurGroup)]
        AmateurGroup
    }

    public enum ProducerSortEnum
    {
        [StringRepresentation(ProducerConstants.ProducerSort.Id)]
        Id,
        [StringRepresentation(ProducerConstants.ProducerSort.Name)]
        Name,
        [StringRepresentation(ProducerConstants.ProducerSort.SearchRank)]
        SearchRank
    }

}