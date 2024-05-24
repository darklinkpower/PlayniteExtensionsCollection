using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VndbApi.Infrastructure.VisualNovelAggregate
{
    public static class VisualNovelConstants
    {
        public static class Filters
        {
            /// <summary>
            /// vndbid.
            /// </summary>
            public const string Id = "id";

            /// <summary>
            /// String search, matches on the VN titles, aliases, and release titles.
            /// Filter: m
            /// </summary>
            public const string Search = "search";

            /// <summary>
            /// Language availability.
            /// Filter: m
            /// </summary>
            public const string Language = "lang";

            /// <summary>
            /// Original language.
            /// </summary>
            public const string OriginalLanguage = "olang";

            /// <summary>
            /// Platform availability.
            /// Filter: m
            /// </summary>
            public const string Platform = "platform";

            /// <summary>
            /// Playtime estimate, integer between 1 (Very short) and 5 (Very long). This filter uses the length votes average when available but falls back to the entries’ length field when there are no votes.
            /// Filter: o
            /// </summary>
            public const string Length = "length";

            /// <summary>
            /// Release date.
            /// Filter: o,n
            /// </summary>
            public const string Released = "released";

            /// <summary>
            /// Bayesian rating, integer between 10 and 100.
            /// Filter: o,i
            /// </summary>
            public const string Rating = "rating";

            /// <summary>
            /// Integer, number of votes.
            /// Filter: o
            /// </summary>
            public const string VoteCount = "votecount";

            /// <summary>
            /// Only accepts a single value, integer 1. Can, of course, still be negated with the != operator.
            /// </summary>
            public const string HasDescription = "has_description";

            /// <summary>
            /// See has_description.
            /// </summary>
            public const string HasAnime = "has_anime";

            /// <summary>
            /// See has_description.
            /// </summary>
            public const string HasScreenshot = "has_screenshot";

            /// <summary>
            /// See has_description.
            /// </summary>
            public const string HasReview = "has_review";

            /// <summary>
            /// Development status, integer. See devstatus field.
            /// </summary>
            public const string DevStatus = "devstatus";

            /// <summary>
            /// Tags applied to this VN, also matches parent tags. See below for more details.
            /// Filter: m
            /// </summary>
            public const string Tag = "tag";

            /// <summary>
            /// Tags applied directly to this VN, does not match parent tags. See below for details.
            /// Filter: m
            /// </summary>
            public const string DirectTag = "dtag";

            /// <summary>
            /// Integer, AniDB anime identifier.
            /// </summary>
            public const string AnimeId = "anime_id";

            /// <summary>
            /// User labels applied to this VN. Accepts a two-element array containing a user ID and label ID. When authenticated or if the "user" request parameter has been set, then it also accepts just a label ID.
            /// Filter: m
            /// </summary>
            public const string Label = "label";

            /// <summary>
            /// Match visual novels that have at least one release matching the given release filters.
            /// Filter: m
            /// </summary>
            public const string Release = "release";

            /// <summary>
            /// Match visual novels that have at least one character matching the given character filters.
            /// Filter: m
            /// </summary>
            public const string Character = "character";

            /// <summary>
            /// Match visual novels that have at least one staff member matching the given staff filters.
            /// Filter: m
            /// </summary>
            public const string Staff = "staff";

            /// <summary>
            /// Match visual novels developed by the given producer filters.
            /// Filter: m
            /// </summary>
            public const string Developer = "developer";
        }

        public static class Fields
        {
            /// <summary>
            /// vndbid.
            /// </summary>
            public const string Id = "id";

            /// <summary>
            /// Main title as displayed on the site, typically romanized from the original script.
            /// </summary>
            public const string Title = "title";

            /// <summary>
            /// Alternative title, typically the same as title but in the original script.
            /// </summary>
            public const string AltTitle = "alttitle";

            /// <summary>
            /// Full list of titles associated with the VN, always contains at least one title.
            /// </summary>
            public const string Titles = "titles";

            /// <summary>
            /// Language of the title.
            /// </summary>
            public const string TitlesLang = "titles.lang";

            /// <summary>
            /// Title in the original script.
            /// </summary>
            public const string TitlesTitle = "titles.title";

            /// <summary>
            /// Romanized version of title, can be null.
            /// </summary>
            public const string TitlesLatin = "titles.latin";

            /// <summary>
            /// Whether this is the official title.
            /// </summary>
            public const string TitlesOfficial = "titles.official";

            /// <summary>
            /// Whether this is the main title for the visual novel entry.
            /// </summary>
            public const string TitlesMain = "titles.main";

            /// <summary>
            /// List of aliases.
            /// </summary>
            public const string Aliases = "aliases";

            /// <summary>
            /// Language the VN has originally been written in.
            /// </summary>
            public const string Olang = "olang";

            /// <summary>
            /// Development status: 0 = Finished, 1 = In development, 2 = Cancelled.
            /// </summary>
            public const string DevStatus = "devstatus";

            /// <summary>
            /// Release date, possibly null.
            /// </summary>
            public const string Released = "released";

            /// <summary>
            /// List of languages this VN is available in. Does not include machine translations.
            /// </summary>
            public const string Languages = "languages";

            /// <summary>
            /// List of platforms for which this VN is available.
            /// </summary>
            public const string Platforms = "platforms";

            /// <summary>
            /// Rough length estimate of the VN between 1 (very short) and 5 (very long). Possibly null.
            /// </summary>
            public const string Length = "length";

            /// <summary>
            /// Average of user-submitted play times in minutes, possibly null.
            /// </summary>
            public const string LengthMinutes = "length_minutes";

            /// <summary>
            /// Number of submitted play times.
            /// </summary>
            public const string LengthVotes = "length_votes";

            /// <summary>
            /// Description, possibly null, may contain formatting codes.
            /// </summary>
            public const string Description = "description";

            /// <summary>
            /// Rating between 10 and 100, null if nobody voted.
            /// </summary>
            public const string Rating = "rating";

            /// <summary>
            /// Number of votes.
            /// </summary>
            public const string VoteCount = "votecount";

            /// <summary>
            /// Relation type.
            /// </summary>
            public const string RelationsRelation = "relations.relation";

            /// <summary>
            /// Whether this VN relation is official.
            /// </summary>
            public const string RelationsRelationOfficial = "relations.relation_official";

            /// <summary>
            /// Tag rating between 0 (exclusive) and 3 (inclusive).
            /// </summary>
            public const string TagsRating = "tags.rating";

            /// <summary>
            /// Spoiler level: 0, 1 or 2.
            /// </summary>
            public const string TagsSpoiler = "tags.spoiler";

            /// <summary>
            /// Indicates if the tag is a lie.
            /// </summary>
            public const string TagsLie = "tags.lie";

            /// <summary>
            /// Edition identifier.
            /// </summary>
            public const string EditionsEid = "editions.eid";

            /// <summary>
            /// Edition language, possibly null.
            /// </summary>
            public const string EditionsLang = "editions.lang";

            /// <summary>
            /// English name / label identifying this edition.
            /// </summary>
            public const string EditionsName = "editions.name";

            /// <summary>
            /// Indicates if this is the official edition.
            /// </summary>
            public const string EditionsOfficial = "editions.official";

            /// <summary>
            /// Edition identifier for the staff or null if they worked on the original version.
            /// </summary>
            public const string StaffEid = "staff.eid";

            /// <summary>
            /// Role of the staff.
            /// </summary>
            public const string StaffRole = "staff.role";

            /// <summary>
            /// Note about the staff, possibly null.
            /// </summary>
            public const string StaffNote = "staff.note";

            /// <summary>
            /// Note about the voice actor, possibly null.
            /// </summary>
            public const string VaNote = "va.note";




            /// <summary>
            /// Image object, can be null.
            /// </summary>
            public const string Image = "image.";

            /// <summary>
            /// Array of objects, possibly empty.
            /// The above image.* fields are also available for screenshots. 
            /// </summary>
            public const string Screenshots = "screenshots.";

            /// <summary>
            /// Release object for the screenshots. All release fields can be selected.
            /// </summary>
            public const string ScreenshotsRelease = "screenshots.release.";

            /// <summary>
            /// Array of objects, list of VNs directly related to this entry.
            /// All visual novel fields can be selected here.
            /// </summary>
            public const string Relations = "relations.";

            /// <summary>
            /// Array of objects, possibly empty. Only directly applied tags are returned, parent tags are not included.
            /// All tag fields can be used here. If you’re fetching tags for more than a single visual novel, it’s usually more efficient to only select tags.id here and then fetch (and cache) further tag information as a separate request. Otherwise the same tag info may get duplicated many times in the response.
            /// </summary>
            public const string Tags = "tags.";

            /// <summary>
            /// Array of objects. The developers of a VN are all producers with a “developer” role on a release linked to the VN. You can get this same information by fetching all relevant release entries, but if all you need is the list of developers then querying this field is faster.
            /// All producer fields can be used here.
            /// </summary>
            public const string Developers = "developers.";

            /// <summary>
            /// All staff fields can be used here.
            /// </summary>
            public const string Staff = "staff.";

            /// <summary>
            /// Person who voiced the character, all staff fields can be used here.
            /// </summary>
            public const string VaStaff = "va.staff.";

            /// <summary>
            /// VN character being voiced, all character fields can be used here.
            /// </summary>
            public const string VaCharacter = "va.character.";

        }

        /// <summary>
        /// Provides constants for sorting fields in requests.
        /// </summary>
        public static class RequestSort
        {
            /// <summary>
            /// Sort by ID.
            /// </summary>
            public const string Id = "id";

            /// <summary>
            /// Sort by title.
            /// </summary>
            public const string Title = "title";

            /// <summary>
            /// Sort by release date.
            /// </summary>
            public const string Released = "released";

            /// <summary>
            /// Sort by rating.
            /// </summary>
            public const string Rating = "rating";

            /// <summary>
            /// Sort by vote count.
            /// </summary>
            public const string VoteCount = "votecount";

            /// <summary>
            /// Sort by search rank.
            /// </summary>
            public const string SearchRank = "searchrank";
        }
    }
}