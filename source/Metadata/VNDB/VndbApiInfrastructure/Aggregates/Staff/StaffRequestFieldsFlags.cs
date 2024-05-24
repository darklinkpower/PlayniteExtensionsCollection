using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;

namespace VndbApiInfrastructure.StaffAggregate
{
    [Flags]
    public enum StaffRequestFieldsFlags
    {
        None = 0,
        [StringRepresentation(StaffConstants.Fields.Id)]
        Id = 1 << 0,
        [StringRepresentation(StaffConstants.Fields.Aid)]
        Aid = 1 << 1,
        [StringRepresentation(StaffConstants.Fields.IsMain)]
        IsMain = 1 << 2,
        [StringRepresentation(StaffConstants.Fields.Name)]
        Name = 1 << 3,
        [StringRepresentation(StaffConstants.Fields.Original)]
        Original = 1 << 4,
        [StringRepresentation(StaffConstants.Fields.Lang)]
        Lang = 1 << 5,
        [StringRepresentation(StaffConstants.Fields.Gender)]
        Gender = 1 << 6,
        [StringRepresentation(StaffConstants.Fields.Description)]
        Description = 1 << 7
    }

}
