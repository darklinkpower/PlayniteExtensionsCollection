using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;

namespace VndbApiInfrastructure.ProducerAggregate
{
    [Flags]
    public enum ProducerRequestFieldsFlags
    {
        None = 0,
        [StringRepresentation(ProducerConstants.Fields.Id)]
        Id = 1 << 0,
        [StringRepresentation(ProducerConstants.Fields.Name)]
        Name = 1 << 1,
        [StringRepresentation(ProducerConstants.Fields.Original)]
        Original = 1 << 2,
        [StringRepresentation(ProducerConstants.Fields.Aliases)]
        Aliases = 1 << 3,
        [StringRepresentation(ProducerConstants.Fields.Lang)]
        Language = 1 << 4,
        [StringRepresentation(ProducerConstants.Fields.Type)]
        Type = 1 << 5,
        [StringRepresentation(ProducerConstants.Fields.Description)]
        Description = 1 << 6
    }
}