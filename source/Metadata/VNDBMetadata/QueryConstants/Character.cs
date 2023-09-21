using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.QueryConstants
{
    public static class Character
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
            /// String, see vns.role field. If this filter is used when nested inside a visual novel filter, then this matches the role of the particular visual novel. Otherwise, this matches the role of any linked visual novel.
            /// Filter: m
            /// </summary>
            public const string Role = "role";

            /// <summary>
            /// String.
            /// </summary>
            public const string BloodType = "blood_type";

            /// <summary>
            /// String.
            /// </summary>
            public const string Sex = "sex";

            /// <summary>
            /// Integer, cm.
            /// Filter: o, n, i
            /// </summary>
            public const string Height = "height";

            /// <summary>
            /// Integer, kg.
            /// Filter: o, n, i
            /// </summary>
            public const string Weight = "weight";

            /// <summary>
            /// Integer, cm.
            /// Filter: o, n, i
            /// </summary>
            public const string Bust = "bust";

            /// <summary>
            /// Integer, cm.
            /// Filter: o, n, i
            /// </summary>
            public const string Waist = "waist";

            /// <summary>
            /// Integer, cm.
            /// Filter: o, n, i
            /// </summary>
            public const string Hips = "hips";

            /// <summary>
            /// String, cup size.
            /// Filter: o, n, i
            /// </summary>
            public const string Cup = "cup";

            /// <summary>
            /// Integer.
            /// Filter: o, n, i
            /// </summary>
            public const string Age = "age";

            /// <summary>
            /// Traits applied to this character, also matches parent traits. See below for more details.
            /// Filter: m
            /// </summary>
            public const string Trait = "trait";

            /// <summary>
            /// Traits applied directly to this character, does not match parent traits. See below for details.
            /// Filter: m
            /// </summary>
            public const string DTrait = "dtrait";

            /// <summary>
            /// Array of two integers, month and day. Day may be 0 to find characters whose birthday is in a given month.
            /// </summary>
            public const string Birthday = "birthday";

            /// <summary>
            /// Match characters that are voiced by the matching staff filters. Voice actor information is actually specific to visual novels, but this filter does not (currently) correlate against the parent entry when nested inside a visual novel filter.
            /// Filter: m
            /// </summary>
            public const string Seiyuu = "seiyuu";

            /// <summary>
            /// Match characters linked to visual novels described by visual novel filters.
            /// Filter: m
            /// </summary>
            public const string Vn = "vn";
        }

        public static class Fields
        {
            /// <summary>
            /// vndbid.
            /// </summary>
            public const string Id = "id";

            /// <summary>
            /// String.
            /// </summary>
            public const string Name = "name";

            /// <summary>
            /// String, possibly null, name in the original script.
            /// </summary>
            public const string Original = "original";

            /// <summary>
            /// Array of strings.
            /// </summary>
            public const string Aliases = "aliases";

            /// <summary>
            /// String, possibly null, may contain formatting codes.
            /// </summary>
            public const string Description = "description";

            /// <summary>
            /// Object, possibly null, same sub-fields as the image visual novel field.
            /// </summary>
            public const string Image = "image.*";

            /// <summary>
            /// String, possibly null, "a", "b", "ab" or "o".
            /// </summary>
            public const string BloodType = "blood_type";

            /// <summary>
            /// Integer, possibly null, cm.
            /// </summary>
            public const string Height = "height";

            /// <summary>
            /// Integer, possibly null, kg.
            /// </summary>
            public const string Weight = "weight";

            /// <summary>
            /// Integer, possibly null, cm.
            /// </summary>
            public const string Bust = "bust";

            /// <summary>
            /// Integer, possibly null, cm.
            /// </summary>
            public const string Waist = "waist";

            /// <summary>
            /// Integer, possibly null, cm.
            /// </summary>
            public const string Hips = "hips";

            /// <summary>
            /// String, possibly null, "AAA", "AA", or any single letter in the alphabet.
            /// </summary>
            public const string Cup = "cup";

            /// <summary>
            /// Integer, possibly null, years.
            /// </summary>
            public const string Age = "age";

            /// <summary>
            /// Possibly null, otherwise an array of two integers: month and day, respectively.
            /// </summary>
            public const string Birthday = "birthday";

            /// <summary>
            /// Possibly null, otherwise an array of two strings: the character’s apparent (non-spoiler) sex and the character’s real (spoiler) sex. Possible values are null, "m", "f" or "b" (meaning “both”).
            /// </summary>
            public const string Sex = "sex";

            /// <summary>
            /// Array of objects, visual novels this character appears in. The same visual novel may be listed multiple times with a different release; the spoiler level and role can be different per release.
            /// </summary>
            public const string Vns = "vns";

            /// <summary>
            /// Integer.
            /// </summary>
            public const string VnsSpoiler = "vns.spoiler";

            /// <summary>
            /// String, "main" for protagonist, "primary" for main characters, "side" or "appears".
            /// </summary>
            public const string VnsRole = "vns.role";

            /// <summary>
            /// All visual novel fields are available here.
            /// </summary>
            public const string VnsAll = "vns.*";

            /// <summary>
            /// Object, usually null, specific release that this character appears in. All release fields are available here.
            /// </summary>
            public const string VnsRelease = "vns.release.*";

            /// <summary>
            /// Array of objects, possibly empty.
            /// </summary>
            public const string Traits = "traits";

            /// <summary>
            /// Integer, 0, 1 or 2, spoiler level.
            /// </summary>
            public const string TraitsSpoiler = "traits.spoiler";

            /// <summary>
            /// Boolean.
            /// </summary>
            public const string TraitsLie = "traits.lie";

            /// <summary>
            /// All trait fields are available here.
            /// </summary>
            public const string TraitsAll = "traits.*";
        }
    }
}