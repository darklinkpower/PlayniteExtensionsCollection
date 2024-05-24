using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;

namespace VndbApiDomain.StaffAggregate
{
    public enum StaffGenderEnum
    {
        [StringRepresentation(StaffConstants.Gender.Male)]
        Male,
        [StringRepresentation(StaffConstants.Gender.Female)]
        Female
    }
}