﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;

namespace VndbApiDomain.ProducerAggregate
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