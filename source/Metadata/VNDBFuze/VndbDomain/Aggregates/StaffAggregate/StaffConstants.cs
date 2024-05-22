using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBFuze.VndbDomain.Aggregates.StaffAggregate
{
    /// <summary>
    /// Constants for staff-related fields.
    /// </summary>
    public static class StaffConstants
    {
        public static class Filters
        {
            /// <summary>
            /// Filter by VNDB ID.
            /// </summary>
            public const string Id = "id";

            /// <summary>
            /// Filter by alias identifier.
            /// </summary>
            public const string Aid = "aid";

            /// <summary>
            /// Filter by string search.
            /// </summary>
            public const string Search = "search";

            /// <summary>
            /// Filter by language.
            /// </summary>
            public const string Lang = "lang";

            /// <summary>
            /// Filter by gender.
            /// </summary>
            public const string Gender = "gender";

            /// <summary>
            /// Filter by role. Can either be "seiyuu" or one of the values from enums.staff_role in the schema JSON. If nested inside a visual novel filter, matches the role of that visual novel. Otherwise, matches the role of any linked visual novel.
            /// </summary>
            public const string Role = "role";

            /// <summary>
            /// Filter by external links. Works similar to the exlink filter for releases.
            /// </summary>
            public const string ExtLink = "extlink";

            /// <summary>
            /// Filter by whether the entry is the main name. Only accepts a single value, integer 1.
            /// </summary>
            public const string IsMain = "ismain";
        }

        /// <summary>
        /// Fields related to staff.
        /// </summary>
        public static class Fields
        {
            /// <summary>
            /// Staff ID.
            /// </summary>
            public const string Id = "id";

            /// <summary>
            /// Staff's VNDB ID (AID).
            /// </summary>
            public const string Aid = "aid";

            /// <summary>
            /// Boolean, whether the 'name' and 'original' fields represent the main name.
            /// </summary>
            public const string IsMain = "ismain";

            /// <summary>
            /// Staff's name.
            /// </summary>
            public const string Name = "name";

            /// <summary>
            /// Staff's name in the original script.
            /// </summary>
            public const string Original = "original";

            /// <summary>
            /// Staff's primary language.
            /// </summary>
            public const string Lang = "lang";

            /// <summary>
            /// Staff's gender.
            /// </summary>
            public const string Gender = "gender";

            /// <summary>
            /// Staff's description.
            /// </summary>
            public const string Description = "description";

            /// <summary>
            /// Array of external links associated with the staff.
            /// </summary>
            public const string ExtLinks = "extlinks";

            /// <summary>
            /// Array of aliases used by the staff.
            /// </summary>
            public const string Aliases = "aliases";

            /// <summary>
            /// Alias ID.
            /// </summary>
            public const string AliasesAid = "aliases.aid";

            /// <summary>
            /// Alias's name in the original script.
            /// </summary>
            public const string AliasesName = "aliases.name";

            /// <summary>
            /// Romanized version of the alias's name.
            /// </summary>
            public const string AliasesLatin = "aliases.latin";

            /// <summary>
            /// Boolean, whether the alias is used as the main name.
            /// </summary>
            public const string AliasesIsMain = "aliases.ismain";
        }

        public static class RequestSort
        {
            /// <summary>
            /// Sort type: Id
            /// </summary>
            public const string Id = "id";

            /// <summary>
            /// Sort type: Name
            /// </summary>
            public const string Name = "name";

            /// <summary>
            /// Sort type: Search Rank
            /// </summary>
            public const string SearchRank = "searchrank";
        }

        public static class Gender
        {
            public const string Male = "m";
            public const string Female = "f";
        }

        public static class Role
        {
            public const string Seiyuu = "seiyuu";
            public const string Scenario = "scenario";
            public const string Director = "director";
            public const string CharacterDesign = "chardesign";
            public const string Artist = "art";
            public const string Composer = "music";
            public const string Songs = "songs";
            public const string Translator = "translator";
            public const string Editor = "editor";
            public const string QualityAssurance = "qa";
            public const string Staff = "staff";
        }
    }
}
