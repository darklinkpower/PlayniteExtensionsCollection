using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VndbApiInfrastructure.ReleaseAggregate
{
    public static class ReleaseConstants
    {
        public static class Filters
        {
            /// <summary>
            /// vndbid
            /// Filter: o
            /// </summary>
            public const string Id = "id";

            /// <summary>
            /// String search.
            /// Filter: m
            /// </summary>
            public const string Search = "search";

            /// <summary>
            /// Match on available languages.
            /// Filter: m
            /// </summary>
            public const string Lang = "lang";

            /// <summary>
            /// Match on available platforms.
            /// Filter: m
            /// </summary>
            public const string Platform = "platform";

            /// <summary>
            /// Release date.
            /// Filter: o
            /// </summary>
            public const string Released = "released";

            /// <summary>
            /// Match on the image resolution, in pixels. Value must be a two-element integer array to which the width and height, respectively, are compared. For example, ["resolution","<=",[640,480]] matches releases with a resolution smaller than or equal to 640x480.
            /// Filter: o, i
            /// </summary>
            public const string Resolution = "resolution";

            /// <summary>
            /// Same as the resolution filter, but additionally requires that the aspect ratio matches that of the given resolution.
            /// Filter: o, i
            /// </summary>
            public const string ResolutionAspect = "resolution_aspect";

            /// <summary>
            /// Integer (0-18), age rating.
            /// Filter: o, n, i
            /// </summary>
            public const string MinAge = "minage";

            /// <summary>
            /// String.
            /// Filter: m, n
            /// </summary>
            public const string Medium = "medium";

            /// <summary>
            /// Integer, see voiced field.
            /// Filter: n
            /// </summary>
            public const string Voiced = "voiced";

            /// <summary>
            /// String.
            /// Filter: n
            /// </summary>
            public const string Engine = "engine";

            /// <summary>
            /// String, see vns.rtype field. If this filter is used when nested inside a visual novel filter, then this matches the rtype of the particular visual novel. Otherwise, this matches the rtype of any linked visual novel.
            /// Filter: m
            /// </summary>
            public const string RType = "rtype";

            /// <summary>
            /// Match on external links, see below for details.
            /// Filter: m
            /// </summary>
            public const string ExtLink = "extlink";

            /// <summary>
            /// Integer, only accepts the value 1.
            /// </summary>
            public const string Patch = "patch";

            /// <summary>
            /// See patch.
            /// Filter: i
            /// </summary>
            public const string Freeware = "freeware";

            /// <summary>
            /// See patch.
            /// Filter: i
            /// </summary>
            public const string Uncensored = "uncensored";

            /// <summary>
            /// See patch.
            /// Filter: i
            /// </summary>
            public const string Official = "official";

            /// <summary>
            /// See patch.
            /// Filter: i
            /// </summary>
            public const string HasEro = "has_ero";

            /// <summary>
            /// Match releases that are linked to at least one visual novel matching the given visual novel filters.
            /// Filter: m
            /// </summary>
            public const string VisualNovel = "vn";

            /// <summary>
            /// Match releases that have at least one producer matching the given producer filters.
            /// Filter: m
            /// </summary>
            public const string Producer = "producer";
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
            /// Array of objects, languages this release is available in. There is always exactly one language that is considered the “main” language of this release, which is only used to select the titles for the title and alttitle fields.
            /// </summary>
            public const string Languages = "languages";

            /// <summary>
            /// String, language. Each language appears at most once.
            /// </summary>
            public const string LanguagesLang = "languages.lang";

            /// <summary>
            /// String, title in the original script. Can be null, in which case the title for this language is the same as the “main” language.
            /// </summary>
            public const string LanguagesTitle = "languages.title";

            /// <summary>
            /// String, can be null, romanized version of title.
            /// </summary>
            public const string LanguagesLatin = "languages.latin";

            /// <summary>
            /// Boolean, whether this is a machine translation.
            /// </summary>
            public const string LanguagesMtl = "languages.mtl";

            /// <summary>
            /// Boolean, whether this language is used to determine the “main” title for the release entry.
            /// </summary>
            public const string LanguagesMain = "languages.main";

            /// <summary>
            /// Array of strings.
            /// </summary>
            public const string Platforms = "platforms";

            /// <summary>
            /// Array of objects.
            /// </summary>
            public const string Media = "media";

            /// <summary>
            /// String.
            /// </summary>
            public const string MediaMedium = "media.medium";

            /// <summary>
            /// Integer, quantity. This is 0 for media where a quantity does not make sense, like “internet download”.
            /// </summary>
            public const string MediaQty = "media.qty";

            /// <summary>
            /// The release type for this visual novel, can be "trial", "partial" or "complete".
            /// </summary>
            public const string VisualNovelReleaseType = "vns.rtype";

            /// <summary>
            /// Boolean.
            /// </summary>
            public const string ProducersDeveloper = "producers.developer";

            /// <summary>
            /// Boolean.
            /// </summary>
            public const string ProducersPublisher = "producers.publisher";

            /// <summary>
            /// Image type, valid values are "pkgfront", "pkgback", "pkgcontent", "pkgside", "pkgmed" and "dig".
            /// </summary>
            public const string ImagesType = "images.type";

            /// <summary>
            /// Visual novel ID to which this image applies, usually null. This field is only useful for bundle releases that are linked to multiple VNs.
            /// </summary>
            public const string ImagesVn = "images.vn";

            /// <summary>
            /// Array of languages for which this image is valid, or null if the image is valid for all languages assigned to this release.
            /// </summary>
            public const string ImagesLanguages = "images.languages";

            /// <summary>
            /// Boolean.
            /// </summary>
            public const string ImagesPhoto = "images.photo";
            
            /// <summary>
            /// Release date.
            /// </summary>
            public const string Released = "released";

            /// <summary>
            /// Integer, possibly null, age rating.
            /// </summary>
            public const string MinAge = "minage";

            /// <summary>
            /// Boolean.
            /// </summary>
            public const string Patch = "patch";

            /// <summary>
            /// Boolean.
            /// </summary>
            public const string Freeware = "freeware";

            /// <summary>
            /// Boolean, can be null.
            /// </summary>
            public const string Uncensored = "uncensored";

            /// <summary>
            /// Boolean.
            /// </summary>
            public const string Official = "official";

            /// <summary>
            /// Boolean.
            /// </summary>
            public const string HasEro = "has_ero";

            /// <summary>
            /// Can either be null, the string "non-standard" or an array of two integers indicating the width and height.
            /// </summary>
            public const string Resolution = "resolution";

            /// <summary>
            /// String, possibly null.
            /// </summary>
            public const string Engine = "engine";

            /// <summary>
            /// Int, possibly null, 1 = not voiced, 2 = only ero scenes voiced, 3 = partially voiced, 4 = fully voiced.
            /// </summary>
            public const string Voiced = "voiced";

            /// <summary>
            /// String, possibly null, may contain formatting codes.
            /// </summary>
            public const string Notes = "notes";

            /// <summary>
            /// JAN/EAN/UPC code, formatted as a string, possibly null.
            /// </summary>
            public const string Gtin = "gtin";

            /// <summary>
            /// String, possibly null, catalog number.
            /// </summary>
            public const string Catalog = "catalog";

            /// <summary>
            /// Array, links to external websites. Works the same as the ‘extlinks’ release field.
            /// </summary>
            public const string ExternalLinks = "extlinks";

            /// <summary>
            /// String, URL.
            /// </summary>
            public const string ExtLinksUrl = "extlinks.url";

            /// <summary>
            /// String, English human-readable label for this link.
            /// </summary>
            public const string ExtLinksLabel = "extlinks.label";

            /// <summary>
            /// Internal identifier of the site, intended for applications that want to localize the label or to parse/format/extract remote identifiers. Keep in mind that the list of supported sites, their internal names, and their ID types are subject to change, but I’ll try to keep things stable.
            /// </summary>
            public const string ExtLinksName = "extlinks.name";

            /// <summary>
            /// Remote identifier for this link. Not all sites have a sensible identifier as part of their URL format, in such cases this field is simply equivalent to the URL.
            /// </summary>
            public const string ExtLinksId = "extlinks.id";


            /// <summary>
            /// Array of objects, the list of visual novels this release is linked to.
            /// </summary>
            public const string VnsAll = "vns.";

            /// <summary>
            /// Array of objects. All producer fields are available here.
            /// </summary>
            public const string ProducersAll = "producers.";
        }

        public static class RequestSort
        {
            /// <summary>
            /// Sort type: Id
            /// </summary>
            public const string Id = "id";

            /// <summary>
            /// Sort type: Title
            /// </summary>
            public const string Title = "title";

            /// <summary>
            /// Sort type: Released
            /// </summary>
            public const string Released = "released";

            /// <summary>
            /// Sort type: Search Rank
            /// </summary>
            public const string SearchRank = "searchrank";
        }
    }
}