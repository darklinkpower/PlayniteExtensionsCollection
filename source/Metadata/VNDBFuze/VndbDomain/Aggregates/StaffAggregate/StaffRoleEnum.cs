using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Attributes;

namespace VNDBFuze.VndbDomain.Aggregates.StaffAggregate
{
    public enum StaffRoleEnum
    {
        [StringRepresentation(StaffConstants.Role.Seiyuu)]
        Seiyuu,

        [StringRepresentation(StaffConstants.Role.Scenario)]
        Scenario,

        [StringRepresentation(StaffConstants.Role.Director)]
        Director,

        [StringRepresentation(StaffConstants.Role.CharacterDesign)]
        CharacterDesign,

        [StringRepresentation(StaffConstants.Role.Artist)]
        Artist,

        [StringRepresentation(StaffConstants.Role.Composer)]
        Music,

        [StringRepresentation(StaffConstants.Role.Songs)]
        Vocals,

        [StringRepresentation(StaffConstants.Role.Translator)]
        Translator,

        [StringRepresentation(StaffConstants.Role.Editor)]
        Editor,

        [StringRepresentation(StaffConstants.Role.QualityAssurance)]
        QualityAssurance,

        [StringRepresentation(StaffConstants.Role.Staff)]
        Staff
    }
}
