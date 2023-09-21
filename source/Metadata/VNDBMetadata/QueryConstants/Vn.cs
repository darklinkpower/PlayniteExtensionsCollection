using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.QueryConstants
{
    public static class Vn
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
            /// String, main title as displayed on the site, typically romanized from the original script.
            /// </summary>
            public const string Title = "title";

            /// <summary>
            /// String, can be null. Alternative title, typically the same as title but in the original script.
            /// </summary>
            public const string AltTitle = "alttitle";

            /// <summary>
            /// Array of objects, full list of titles associated with the VN, always contains at least one title.
            /// </summary>
            public const string Titles = "titles";

            /// <summary>
            /// String, language. Each language appears at most once in the titles list.
            /// </summary>
            public const string Lang = "titles.lang";

            /// <summary>
            /// String, title in the original script.
            /// </summary>
            public const string LangTitle = "titles.title";

            /// <summary>
            /// String, can be null, romanized version of title.
            /// </summary>
            public const string LangLatin = "titles.latin";

            /// <summary>
            /// Boolean.
            /// </summary>
            public const string LangOfficial = "titles.official";

            /// <summary>
            /// Boolean, whether this is the “main” title for the visual novel entry. Exactly one title has this flag set in the titles array and it’s always the title whose lang matches the VN’s olang field. This field is included for convenience, you can of course also use the olang field to grab the main title.
            /// </summary>
            public const string LangMain = "titles.main";

            /// <summary>
            /// Array of strings, list of aliases.
            /// </summary>
            public const string Aliases = "aliases";

            /// <summary>
            /// String, language the VN has originally been written in.
            /// </summary>
            public const string OLang = "olang";

            /// <summary>
            /// Integer, development status. 0 meaning ‘Finished’, 1 is ‘In development’ and 2 for ‘Cancelled’.
            /// </summary>
            public const string DevStatus = "devstatus";

            /// <summary>
            /// Release date, possibly null.
            /// </summary>
            public const string Released = "released";

            /// <summary>
            /// Array of strings, list of languages this VN is available in. Does not include machine translations.
            /// </summary>
            public const string Languages = "languages";

            /// <summary>
            /// Array of strings, list of platforms for which this VN is available.
            /// </summary>
            public const string Platforms = "platforms";

            /// <summary>
            /// Object, can be null.
            /// </summary>
            public const string Image = "image";

            /// <summary>
            /// String, image identifier.
            /// </summary>
            public const string ImageId = "image.id";

            /// <summary>
            /// String.
            /// </summary>
            public const string ImageUrl = "image.url";

            /// <summary>
            /// Pixel dimensions of the image, array with two integer elements indicating the width and height.
            /// </summary>
            public const string ImageDims = "image.dims";

            /// <summary>
            /// Number between 0 and 2 (inclusive), average image flagging vote for sexual content.
            /// </summary>
            public const string ImageSexual = "image.sexual";

            /// <summary>
            /// Number between 0 and 2 (inclusive), average image flagging vote for violence.
            /// </summary>
            public const string ImageViolence = "image.violence";

            /// <summary>
            /// Integer, number of image flagging votes.
            /// </summary>
            public const string ImageVoteCount = "image.votecount";

            /// <summary>
            /// Integer, possibly null, rough length estimate of the VN between 1 (very short) and 5 (very long). This field is only used as a fallback for when there are no length votes, so you’ll probably want to fetch length_minutes too.
            /// </summary>
            public const string Length = "length";

            /// <summary>
            /// Integer, possibly null, average of user-submitted play times in minutes.
            /// </summary>
            public const string LengthMinutes = "length_minutes";

            /// <summary>
            /// Integer, number of submitted play times.
            /// </summary>
            public const string LengthVotes = "length_votes";

            /// <summary>
            /// String, possibly null, may contain formatting codes.
            /// </summary>
            public const string Description = "description";

            /// <summary>
            /// Number between 10 and 100, null if nobody voted.
            /// </summary>
            public const string Rating = "rating";

            /// <summary>
            /// Integer, number of votes.
            /// </summary>
            public const string VoteCount = "votecount";

            /// <summary>
            /// Array of objects, possibly empty.
            /// </summary>
            public const string Screenshots = "screenshots";

            /// <summary>
            /// String, URL to the thumbnail.
            /// </summary>
            public const string ScreenshotsThumbnail = "screenshots.thumbnail";

            /// <summary>
            /// Pixel dimensions of the thumbnail, array with two integer elements.
            /// </summary>
            public const string ScreenshotsThumbnailDims = "screenshots.thumbnail_dims";

            /// <summary>
            /// Release object. All release fields can be selected. It is very common for all screenshots of a VN to be assigned to the same release, so the fields you select here are likely to get duplicated several times in the response. If you want to fetch more than just a few fields, it is more efficient to only select release.id here and then grab detailed release info with a separate request.
            /// </summary>
            public const string ScreenshotsRelease = "screenshots.release";

            /// <summary>
            /// Array of objects, possibly empty. Only directly applied tags are returned, parent tags are not included.
            /// </summary>
            public const string Tags = "tags";

            /// <summary>
            /// Number, tag rating between 0 (exclusive) and 3 (inclusive).
            /// </summary>
            public const string TagsRating = "tags.rating";

            /// <summary>
            /// Integer, 0, 1, or 2, spoiler level.
            /// </summary>
            public const string TagsSpoiler = "tags.spoiler";

            /// <summary>
            /// Boolean.
            /// </summary>
            public const string TagsLie = "tags.lie";

            /// <summary>
            /// All tag fields can be used here. If you’re fetching tags for more than a single visual novel, it’s usually more efficient to only select tags.id here and then fetch (and cache) further tag information as a separate request. Otherwise, the same tag info may get duplicated many times in the response.
            /// </summary>
            public const string TagsAll = "tags.*";

            /// <summary>
            /// Array of objects. The developers of a VN are all producers with a “developer” role on a release linked to the VN. You can get this same information by fetching all relevant release entries, but if all you need is the list of developers then querying this field is faster.
            /// </summary>
            public const string Developers = "developers";

            /// <summary>
            /// All producer fields can be used here.
            /// </summary>
            public const string DevelopersAll = "developers.*";

        }
    }
}