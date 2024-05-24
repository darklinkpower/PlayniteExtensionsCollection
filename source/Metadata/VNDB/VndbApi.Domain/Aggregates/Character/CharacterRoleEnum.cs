using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApi.Domain.SharedKernel;

namespace VndbApi.Domain.CharacterAggregate
{
    public enum CharacterRoleEnum
    {
        /// <summary>
        /// Main protagonist.
        /// </summary>
        [StringRepresentation(CharacterConstants.VisualNovelRoles.Main)]
        Main,

        /// <summary>
        /// Main character.
        /// </summary>
        [StringRepresentation(CharacterConstants.VisualNovelRoles.Primary)]
        Primary,

        /// <summary>
        /// Side character.
        /// </summary>
        [StringRepresentation(CharacterConstants.VisualNovelRoles.Side)]
        Side,

        /// <summary>
        /// Appears.
        /// </summary>
        [StringRepresentation(CharacterConstants.VisualNovelRoles.Appears)]
        Appears
    }

}