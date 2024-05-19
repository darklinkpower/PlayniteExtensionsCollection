using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.VndbDomain.Aggregates.VnAggregate
{
    [Flags]
    public enum VnRequestFieldsFlags
    {
        /// <summary>
        /// No fields selected.
        /// </summary>
        None = 0,

        /// <summary>
        /// Visual novel ID.
        /// </summary>
        Id = 1 << 0,

        /// <summary>
        /// Main title as displayed on the site, typically romanized from the original script.
        /// </summary>
        Title = 1 << 1,

        /// <summary>
        /// Alternative title, typically the same as title but in the original script.
        /// </summary>
        AltTitle = 1 << 2,

        /// <summary>
        /// Full list of titles associated with the VN.
        /// </summary>
        Titles = 1 << 3,

        /// <summary>
        /// Language of the titles.
        /// </summary>
        TitlesLang = 1 << 4,

        /// <summary>
        /// Title in the original script.
        /// </summary>
        TitlesTitle = 1 << 5,

        /// <summary>
        /// Romanized version of title.
        /// </summary>
        TitlesLatin = 1 << 6,

        /// <summary>
        /// Boolean.
        /// </summary>
        TitlesOfficial = 1 << 7,

        /// <summary>
        /// Whether this is the “main” title for the visual novel entry.
        /// </summary>
        TitlesMain = 1 << 8,

        /// <summary>
        /// List of aliases.
        /// </summary>
        Aliases = 1 << 9,

        /// <summary>
        /// Language the VN has originally been written in.
        /// </summary>
        OLang = 1 << 10,

        /// <summary>
        /// Development status.
        /// </summary>
        DevStatus = 1 << 11,

        /// <summary>
        /// Release date.
        /// </summary>
        Released = 1 << 12,

        /// <summary>
        /// List of languages this VN is available in.
        /// </summary>
        Languages = 1 << 13,

        /// <summary>
        /// List of platforms for which this VN is available.
        /// </summary>
        Platforms = 1 << 14,

        /// <summary>
        /// Image information.
        /// </summary>
        Image = 1 << 15,

        /// <summary>
        /// Image identifier.
        /// </summary>
        ImageId = 1 << 16,

        /// <summary>
        /// URL of the image.
        /// </summary>
        ImageUrl = 1 << 17,

        /// <summary>
        /// Pixel dimensions of the image.
        /// </summary>
        ImageDims = 1 << 18,

        /// <summary>
        /// Average image flagging vote for sexual content.
        /// </summary>
        ImageSexual = 1 << 19,

        /// <summary>
        /// Average image flagging vote for violence.
        /// </summary>
        ImageViolence = 1 << 20,

        /// <summary>
        /// Number of image flagging votes.
        /// </summary>
        ImageVoteCount = 1 << 21,

        /// <summary>
        /// URL to the thumbnail.
        /// </summary>
        ImageThumbnail = 1 << 22,

        /// <summary>
        /// Pixel dimensions of the thumbnail.
        /// </summary>
        ImageThumbnailDims = 1 << 23,

        /// <summary>
        /// Rough length estimate of the VN between 1 (very short) and 5 (very long).
        /// </summary>
        Length = 1 << 24,

        /// <summary>
        /// Average of user-submitted play times in minutes.
        /// </summary>
        LengthMinutes = 1 << 25,

        /// <summary>
        /// Number of submitted play times.
        /// </summary>
        LengthVotes = 1 << 26,

        /// <summary>
        /// Description of the visual novel.
        /// </summary>
        Description = 1 << 27,

        /// <summary>
        /// Rating of the visual novel.
        /// </summary>
        Rating = 1 << 28,

        /// <summary>
        /// Number of votes.
        /// </summary>
        VoteCount = 1 << 29,

        /// <summary>
        /// Array of objects, possibly empty.
        /// </summary>
        Screenshots = 1 << 30,

        /// <summary>
        /// List of VNs directly related to this entry.
        /// </summary>
        Relations = 1 << 31,

        /// <summary>
        /// Relation type.
        /// </summary>
        RelationsRelation = 1 << 32,

        /// <summary>
        /// Whether this VN relation is official.
        /// </summary>
        RelationsRelationOfficial = 1 << 33,

        /// <summary>
        /// Array of tags.
        /// </summary>
        Tags = 1 << 34,

        /// <summary>
        /// Tag rating between 0 (exclusive) and 3 (inclusive).
        /// </summary>
        TagsRating = 1 << 35,

        /// <summary>
        /// Spoiler level of the tag.
        /// </summary>
        TagsSpoiler = 1 << 36,

        /// <summary>
        /// Whether the tag is a lie.
        /// </summary>
        TagsLie = 1 << 37,

        /// <summary>
        /// List of developers.
        /// </summary>
        Developers = 1 << 38,

        /// <summary>
        /// Array of editions.
        /// </summary>
        Editions = 1 << 39,

        /// <summary>
        /// Edition identifier.
        /// </summary>
        EditionsEid = 1 << 40,

        /// <summary>
        /// Language of the edition.
        /// </summary>
        EditionsLang = 1 << 41,

        /// <summary>
        /// English name / label identifying this edition.
        /// </summary>
        EditionsName = 1 << 42,

        /// <summary>
        /// Whether the edition is official.
        /// </summary>
        EditionsOfficial = 1 << 43,

        /// <summary>
        /// Array of staff members.
        /// </summary>
        Staff = 1 << 44,

        /// <summary>
        /// Edition identifier or null when the staff has worked on the “original” version of the visual novel.
        /// </summary>
        StaffEid = 1 << 45,

        /// <summary>
        /// Role of the staff member.
        /// </summary>
        StaffRole = 1 << 46,

        /// <summary>
        /// Note associated with the staff member.
        /// </summary>
        StaffNote = 1 << 47
    }

    // Excluded fields: screenshots.*, screenshots.release.* relations.*, tags.*, developers.*, staff.*
}
