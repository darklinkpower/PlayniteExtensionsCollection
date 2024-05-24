using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApi.Domain.SharedKernel;

namespace VndbApi.Domain.StaffAggregate
{
    public enum StaffGenderEnum
    {
        [StringRepresentation(StaffConstants.Gender.Male)]
        Male,
        [StringRepresentation(StaffConstants.Gender.Female)]
        Female
    }
}