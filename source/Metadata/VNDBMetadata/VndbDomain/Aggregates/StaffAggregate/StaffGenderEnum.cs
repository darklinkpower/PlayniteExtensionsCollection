using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Common.Attributes;

namespace VNDBMetadata.VndbDomain.Aggregates.StaffAggregate
{
    public enum StaffGenderEnum
    {
        [StringRepresentation(StaffConstants.Gender.Male)]
        Male,
        [StringRepresentation(StaffConstants.Gender.Female)]
        Female
    }
}