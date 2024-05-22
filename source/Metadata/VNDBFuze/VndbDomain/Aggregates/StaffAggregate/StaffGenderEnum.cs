using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Attributes;

namespace VNDBFuze.VndbDomain.Aggregates.StaffAggregate
{
    public enum StaffGenderEnum
    {
        [StringRepresentation(StaffConstants.Gender.Male)]
        Male,
        [StringRepresentation(StaffConstants.Gender.Female)]
        Female
    }
}