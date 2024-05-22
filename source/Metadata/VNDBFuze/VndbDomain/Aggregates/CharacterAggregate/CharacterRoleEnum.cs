using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Attributes;

namespace VNDBFuze.VndbDomain.Aggregates.CharacterAggregate
{
    public enum CharacterRoleEnum
    {
        /// <summary>
        /// Main protagonist.
        /// </summary>
        [StringRepresentation(CharacterConstants.VnRoles.Main)]
        Main,

        /// <summary>
        /// Main character.
        /// </summary>
        [StringRepresentation(CharacterConstants.VnRoles.Primary)]
        Primary,

        /// <summary>
        /// Side character.
        /// </summary>
        [StringRepresentation(CharacterConstants.VnRoles.Side)]
        Side,

        /// <summary>
        /// Appears.
        /// </summary>
        [StringRepresentation(CharacterConstants.VnRoles.Appears)]
        Appears
    }

}