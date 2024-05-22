using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Attributes;

namespace VNDBFuze.VndbDomain.Aggregates.ProducerAggregate
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
}