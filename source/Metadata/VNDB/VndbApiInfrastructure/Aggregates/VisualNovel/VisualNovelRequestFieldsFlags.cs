using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;

namespace VndbApiInfrastructure.VisualNovelAggregate
{
    [Flags]
    public enum VnRequestFieldsFlags : ulong
    {
        /// <summary>
        /// No fields selected.
        /// </summary>
        None = 0,

        /// <summary>
        /// vndbid.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.Id)]
        Id = 1 << 0,

        /// <summary>
        /// Main title as displayed on the site, typically romanized from the original script.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.Title)]
        Title = 1 << 1,

        /// <summary>
        /// Alternative title, typically the same as title but in the original script.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.AltTitle)]
        AltTitle = 1 << 2,

        /// <summary>
        /// Language of the title.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.TitlesLang)]
        TitlesLang = 1 << 3,

        /// <summary>
        /// Title in the original script.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.TitlesTitle)]
        TitlesTitle = 1 << 4,

        /// <summary>
        /// Romanized version of title, can be null.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.TitlesLatin)]
        TitlesLatin = 1 << 5,

        /// <summary>
        /// Whether this is the official title.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.TitlesOfficial)]
        TitlesOfficial = 1 << 6,

        /// <summary>
        /// Whether this is the main title for the visual novel entry.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.TitlesMain)]
        TitlesMain = 1 << 7,

        /// <summary>
        /// List of aliases.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.Aliases)]
        Aliases = 1 << 8,

        /// <summary>
        /// Language the VN has originally been written in.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.Olang)]
        Olang = 1 << 9,

        /// <summary>
        /// Development status: 0 = Finished, 1 = In development, 2 = Cancelled.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.DevStatus)]
        DevStatus = 1 << 10,

        /// <summary>
        /// Release date, possibly null.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.Released)]
        ReleaseDate = 1 << 11,

        /// <summary>
        /// List of languages this VN is available in. Does not include machine translations.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.Languages)]
        Languages = 1 << 12,

        /// <summary>
        /// List of platforms for which this VN is available.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.Platforms)]
        Platforms = 1 << 13,

        /// <summary>
        /// Rough length estimate of the VN between 1 (very short) and 5 (very long). Possibly null.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.Length)]
        Length = 1 << 14,

        /// <summary>
        /// Average of user-submitted play times in minutes, possibly null.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.LengthMinutes)]
        LengthMinutes = 1 << 15,

        /// <summary>
        /// Number of submitted play times.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.LengthVotes)]
        LengthVotes = 1 << 16,

        /// <summary>
        /// Description, possibly null, may contain formatting codes.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.Description)]
        Description = 1 << 17,

        /// <summary>
        /// Rating between 10 and 100, null if nobody voted.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.Rating)]
        Rating = 1 << 18,

        /// <summary>
        /// Number of votes.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.VoteCount)]
        VoteCount = 1 << 19,

        /// <summary>
        /// Relation type.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.RelationsRelation)]
        RelationsRelation = 1 << 20,

        /// <summary>
        /// Whether this VN relation is official.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.RelationsRelationOfficial)]
        RelationsRelationOfficial = 1 << 21,

        /// <summary>
        /// Tag rating between 0 (exclusive) and 3 (inclusive).
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.TagsRating)]
        TagsRating = 1 << 22,

        /// <summary>
        /// Spoiler level: 0, 1 or 2.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.TagsSpoiler)]
        TagsSpoiler = 1 << 23,

        /// <summary>
        /// Indicates if the tag is a lie.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.TagsLie)]
        TagsLie = 1 << 24,

        /// <summary>
        /// Edition identifier.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.EditionsEid)]
        EditionsEid = 1 << 25,

        /// <summary>
        /// Edition language, possibly null.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.EditionsLang)]
        EditionsLang = 1 << 26,

        /// <summary>
        /// English name / label identifying this edition.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.EditionsName)]
        EditionsName = 1 << 27,

        /// <summary>
        /// Indicates if this is the official edition.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.EditionsOfficial)]
        EditionsOfficial = 1 << 28,

        /// <summary>
        /// Edition identifier for the staff or null if they worked on the original version.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.StaffEid)]
        StaffEid = 1UL << 29,

        /// <summary>
        /// Role of the staff.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.StaffRole)]
        StaffRole = 1UL << 30,

        /// <summary>
        /// Note about the staff, possibly null.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.StaffNote)]
        StaffNote = 1UL << 31,

        /// <summary>
        /// Note about the voice actor, possibly null.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.VoiceActorNote)]
        VoiceActorNote = 1UL << 32,

        /// <summary>
        /// Raw vote average, between 10 and 100, null if nobody voted (cached, may be out of date by an hour).
        /// </summary>
        [StringRepresentation(VisualNovelConstants.Fields.Average)]
        Average = 1UL << 33
    }

    // Excluded fields: screenshots.*, screenshots.release.* relations.*, tags.*, developers.*, staff.*
}
