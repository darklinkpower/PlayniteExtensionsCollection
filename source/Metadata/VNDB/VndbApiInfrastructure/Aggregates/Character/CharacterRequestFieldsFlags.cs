using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;

namespace VndbApiInfrastructure.CharacterAggregate
{
    [Flags]
    public enum CharacterRequestFieldsFlags
    {
        None = 0,

        /// <summary>
        /// vndbid.
        /// </summary>
        [StringRepresentation(CharacterConstants.Fields.Id)]
        Id = 1 << 0,

        /// <summary>
        /// String.
        /// </summary>
        [StringRepresentation(CharacterConstants.Fields.Name)]
        Name = 1 << 1,

        /// <summary>
        /// String, possibly null, name in the original script.
        /// </summary>
        [StringRepresentation(CharacterConstants.Fields.Original)]
        Original = 1 << 2,

        /// <summary>
        /// Array of strings.
        /// </summary>
        [StringRepresentation(CharacterConstants.Fields.Aliases)]
        Aliases = 1 << 3,

        /// <summary>
        /// String, possibly null, may contain formatting codes.
        /// </summary>
        [StringRepresentation(CharacterConstants.Fields.Description)]
        Description = 1 << 4,

        /// <summary>
        /// String, possibly null, "a", "b", "ab" or "o".
        /// </summary>
        [StringRepresentation(CharacterConstants.Fields.BloodType)]
        BloodType = 1 << 5,

        /// <summary>
        /// Integer, possibly null, cm.
        /// </summary>
        [StringRepresentation(CharacterConstants.Fields.Height)]
        Height = 1 << 6,

        /// <summary>
        /// Integer, possibly null, kg.
        /// </summary>
        [StringRepresentation(CharacterConstants.Fields.Weight)]
        Weight = 1 << 7,

        /// <summary>
        /// Integer, possibly null, cm.
        /// </summary>
        [StringRepresentation(CharacterConstants.Fields.Bust)]
        Bust = 1 << 8,

        /// <summary>
        /// Integer, possibly null, cm.
        /// </summary>
        [StringRepresentation(CharacterConstants.Fields.Waist)]
        Waist = 1 << 9,

        /// <summary>
        /// Integer, possibly null, cm.
        /// </summary>
        [StringRepresentation(CharacterConstants.Fields.Hips)]
        Hips = 1 << 10,

        /// <summary>
        /// String, possibly null, "AAA", "AA", or any single letter in the alphabet.
        /// </summary>
        [StringRepresentation(CharacterConstants.Fields.Cup)]
        Cup = 1 << 11,

        /// <summary>
        /// Integer, possibly null, years.
        /// </summary>
        [StringRepresentation(CharacterConstants.Fields.Age)]
        Age = 1 << 12,

        /// <summary>
        /// Possibly null, otherwise an array of two integers: month and day, respectively.
        /// </summary>
        [StringRepresentation(CharacterConstants.Fields.Birthday)]
        Birthday = 1 << 13,

        /// <summary>
        /// Possibly null, otherwise an array of two strings: the character’s apparent (non-spoiler) sex and the character’s real (spoiler) sex. Possible values are null, "m", "f" or "b" (meaning “both”).
        /// </summary>
        [StringRepresentation(CharacterConstants.Fields.Sex)]
        Sex = 1 << 14,

        /// <summary>
        /// Integer.
        /// </summary>
        [StringRepresentation(CharacterConstants.Fields.VNSSpoiler)]
        VnsSpoiler = 1 << 16,

        /// <summary>
        /// String, "main" for protagonist, "primary" for main characters, "side" or "appears".
        /// </summary>
        [StringRepresentation(CharacterConstants.Fields.VNSRole)]
        VnsRole = 1 << 17,

        /// <summary>
        /// Integer, 0, 1 or 2, spoiler level.
        /// </summary>
        [StringRepresentation(CharacterConstants.Fields.TraitsSpoiler)]
        TraitsSpoiler = 1 << 20,

        /// <summary>
        /// Boolean.
        /// </summary>
        [StringRepresentation(CharacterConstants.Fields.TraitsLie)]
        TraitsLie = 1 << 21
    }

    // Excluded fields: 
    // image.*, vns.*, vns.release.*, traits.*
}
