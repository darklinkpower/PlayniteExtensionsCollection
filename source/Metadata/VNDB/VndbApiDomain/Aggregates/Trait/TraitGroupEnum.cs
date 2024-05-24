using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;

namespace VndbApiDomain.TraitAggregate
{
    public enum TraitGroupEnum
    {
        /// <summary>
        /// Trait category for character's body features.
        /// </summary>
        [StringRepresentation(TraitConstants.Categories.Body)]
        Body,

        /// <summary>
        /// Trait category for character's clothing.
        /// </summary>
        [StringRepresentation(TraitConstants.Categories.Clothes)]
        Clothes,

        /// <summary>
        /// Trait category for character's eye features.
        /// </summary>
        [StringRepresentation(TraitConstants.Categories.Eyes)]
        Eyes,

        /// <summary>
        /// Trait category for character's activities.
        /// </summary>
        [StringRepresentation(TraitConstants.Categories.EngagesIn)]
        EngagesIn,

        /// <summary>
        /// Trait category for character's sexual activities.
        /// </summary>
        [StringRepresentation(TraitConstants.Categories.EngagesInSexual)]
        EngagesInSexual,

        /// <summary>
        /// Trait category for character's hair features.
        /// </summary>
        [StringRepresentation(TraitConstants.Categories.Hair)]
        Hair,

        /// <summary>
        /// Trait category for items associated with the character.
        /// </summary>
        [StringRepresentation(TraitConstants.Categories.Items)]
        Items,

        /// <summary>
        /// Trait category for character's personality traits.
        /// </summary>
        [StringRepresentation(TraitConstants.Categories.Personality)]
        Personality,

        /// <summary>
        /// Trait category for character's roles.
        /// </summary>
        [StringRepresentation(TraitConstants.Categories.Role)]
        Role,

        /// <summary>
        /// Trait category for subjects related to the character.
        /// </summary>
        [StringRepresentation(TraitConstants.Categories.SubjectOf)]
        SubjectOf,

        /// <summary>
        /// Trait category for subjects related to the character sexually.
        /// </summary>
        [StringRepresentation(TraitConstants.Categories.SubjectOfSexual)]
        SubjectOfSexual
    }
}