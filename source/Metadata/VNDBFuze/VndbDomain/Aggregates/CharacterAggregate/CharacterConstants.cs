using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBFuze.VndbDomain.Aggregates.CharacterAggregate
{
    public static class CharacterConstants
    {
        public static class Filters
        {
            /// <summary>
            /// vndbid
            /// </summary>
            public const string Id = "id";

            /// <summary>
            /// String search.
            /// </summary>
            public const string Search = "search";

            /// <summary>
            /// String, see vns.role field. If this filter is used when nested inside a visual novel filter, then this matches the role of the particular visual novel. Otherwise, this matches the role of any linked visual novel.
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
            /// </summary>
            public const string Height = "height";

            /// <summary>
            /// Integer, kg.
            /// </summary>
            public const string Weight = "weight";

            /// <summary>
            /// Integer, cm.
            /// </summary>
            public const string Bust = "bust";

            /// <summary>
            /// Integer, cm.
            /// </summary>
            public const string Waist = "waist";

            /// <summary>
            /// Integer, cm.
            /// </summary>
            public const string Hips = "hips";

            /// <summary>
            /// String, cup size.
            /// </summary>
            public const string Cup = "cup";

            /// <summary>
            /// Integer.
            /// </summary>
            public const string Age = "age";

            /// <summary>
            /// Traits applied to this character, also matches parent traits.
            /// </summary>
            public const string Trait = "trait";

            /// <summary>
            /// Traits applied directly to this character, does not match parent traits.
            /// </summary>
            public const string DirectTrait = "dtrait";

            /// <summary>
            /// Array of two integers, month and day. Day may be 0 to find characters whose birthday is in a given month.
            /// </summary>
            public const string Birthday = "birthday";

            /// <summary>
            /// Match characters that are voiced by the matching staff filters. Voice actor information is actually specific to visual novels, but this filter does not (currently) correlate against the parent entry when nested inside a visual novel filter.
            /// </summary>
            public const string Seiyuu = "seiyuu";

            /// <summary>
            /// Match characters linked to visual novels described by visual novel filters.
            /// </summary>
            public const string VisualNovel = "vn";
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
            public const string VNSSpoiler = "vns.spoiler";

            /// <summary>
            /// String, "main" for protagonist, "primary" for main characters, "side" or "appears".
            /// </summary>
            public const string VNSRole = "vns.role";

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
            /// Object, possibly null, same sub-fields as the image visual novel field. 
            /// (Except for thumbnail and thumbnail_dims because character images are currently always limited to 256x300px, but that is subject to change in the future).
            /// </summary>
            public const string ImageAllFields = "image.";

            /// <summary>
            /// All visual novel fields are available here.
            /// </summary>
            public const string VnsAllFields = "vns.";

            /// <summary>
            /// Object, usually null, specific release that this character appears in. All release fields are available here.
            /// </summary>
            public const string VnsReleaseAllFields = "vns.release.";

            /// <summary>
            /// All trait fields are available here.
            /// </summary>
            public const string TraitsAllFields = "traits.";
        }

        public static class RequestSort
        {
            /// <summary>
            /// Id
            /// </summary>
            public const string Id = "id";

            /// <summary>
            /// Name
            /// </summary>
            public const string Name = "name";

            /// <summary>
            /// Search rank
            /// </summary>
            public const string SearchRank = "searchrank";
        }

        public static class BloodType
        {
            /// <summary>
            /// String, possibly null, "a".
            /// </summary>
            public const string A = "a";

            /// <summary>
            /// String, possibly null, "b".
            /// </summary>
            public const string B = "b";

            /// <summary>
            /// String, possibly null, "ab".
            /// </summary>
            public const string AB = "ab";

            /// <summary>
            /// String, possibly null, "o".
            /// </summary>
            public const string O = "o";
        }

        public static class CupSize
        {
            /// <summary>
            /// String, possibly null, "AAA".
            /// </summary>
            public const string AAA = "AAA";

            /// <summary>
            /// String, possibly null, "AA".
            /// </summary>
            public const string AA = "AA";

            /// <summary>
            /// String, possibly null, "A".
            /// </summary>
            public const string A = "A";

            /// <summary>
            /// String, possibly null, "B".
            /// </summary>
            public const string B = "B";

            /// <summary>
            /// String, possibly null, "C".
            /// </summary>
            public const string C = "C";

            /// <summary>
            /// String, possibly null, "D".
            /// </summary>
            public const string D = "D";

            /// <summary>
            /// String, possibly null, "E".
            /// </summary>
            public const string E = "E";

            /// <summary>
            /// String, possibly null, "F".
            /// </summary>
            public const string F = "F";

            /// <summary>
            /// String, possibly null, "G".
            /// </summary>
            public const string G = "G";

            /// <summary>
            /// String, possibly null, "H".
            /// </summary>
            public const string H = "H";

            /// <summary>
            /// String, possibly null, "I".
            /// </summary>
            public const string I = "I";

            /// <summary>
            /// String, possibly null, "J".
            /// </summary>
            public const string J = "J";

            /// <summary>
            /// String, possibly null, "K".
            /// </summary>
            public const string K = "K";

            /// <summary>
            /// String, possibly null, "L".
            /// </summary>
            public const string L = "L";

            /// <summary>
            /// String, possibly null, "M".
            /// </summary>
            public const string M = "M";

            /// <summary>
            /// String, possibly null, "N".
            /// </summary>
            public const string N = "N";

            /// <summary>
            /// String, possibly null, "O".
            /// </summary>
            public const string O = "O";

            /// <summary>
            /// String, possibly null, "P".
            /// </summary>
            public const string P = "P";

            /// <summary>
            /// String, possibly null, "Q".
            /// </summary>
            public const string Q = "Q";

            /// <summary>
            /// String, possibly null, "R".
            /// </summary>
            public const string R = "R";

            /// <summary>
            /// String, possibly null, "S".
            /// </summary>
            public const string S = "S";

            /// <summary>
            /// String, possibly null, "T".
            /// </summary>
            public const string T = "T";

            /// <summary>
            /// String, possibly null, "U".
            /// </summary>
            public const string U = "U";

            /// <summary>
            /// String, possibly null, "V".
            /// </summary>
            public const string V = "V";

            /// <summary>
            /// String, possibly null, "W".
            /// </summary>
            public const string W = "W";

            /// <summary>
            /// String, possibly null, "X".
            /// </summary>
            public const string X = "X";

            /// <summary>
            /// String, possibly null, "Y".
            /// </summary>
            public const string Y = "Y";

            /// <summary>
            /// String, possibly null, "Z".
            /// </summary>
            public const string Z = "Z";
        }

        public static class Sex
        {
            /// <summary>
            /// Male.
            /// </summary>
            public const string Male = "m";

            /// <summary>
            /// Female.
            /// </summary>
            public const string Female = "f";

            /// <summary>
            /// Both.
            /// </summary>
            public const string Both = "b";
        }

        public static class VnRoles
        {
            /// <summary>
            /// String, "main" for protagonist.
            /// </summary>
            public const string Main = "main";

            /// <summary>
            /// String, "primary" for main characters.
            /// </summary>
            public const string Primary = "primary";

            /// <summary>
            /// String, "side".
            /// </summary>
            public const string Side = "side";

            /// <summary>
            /// String, "appears".
            /// </summary>
            public const string Appears = "appears";
        }

    }
}