using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VNDBFuze.VndbDomain.Aggregates.TraitAggregate
{
    public static class TraitConstants
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
        }

        public static class Fields
        {
            /// <summary>
            /// vndbid
            /// </summary>
            public const string Id = "id";

            /// <summary>
            /// String. Trait names are not necessarily self-describing, so they should always be displayed together with their “group” (see below), which is the top-level parent that the trait belongs to.
            /// </summary>
            public const string Name = "name";

            /// <summary>
            /// Array of strings.
            /// </summary>
            public const string Aliases = "aliases";

            /// <summary>
            /// String, may contain formatting codes.
            /// </summary>
            public const string Description = "description";

            /// <summary>
            /// Bool.
            /// </summary>
            public const string Searchable = "searchable";

            /// <summary>
            /// Bool.
            /// </summary>
            public const string Applicable = "applicable";

            /// <summary>
            /// vndbid
            /// </summary>
            public const string GroupId = "group_id";

            /// <summary>
            /// String
            /// </summary>
            public const string GroupName = "group_name";

            /// <summary>
            /// Integer, number of characters this trait has been applied to, including child traits.
            /// </summary>
            public const string CharCount = "char_count";
        }

        /// <summary>
        /// Static class containing constants for VNDB character trait categories.
        /// </summary>
        public static class Categories
        {
            /// <summary>
            /// Trait category for character's body features.
            /// </summary>
            public const string Body = "Body";

            /// <summary>
            /// Trait category for character's clothing.
            /// </summary>
            public const string Clothes = "Clothes";

            /// <summary>
            /// Trait category for character's eye features.
            /// </summary>
            public const string Eyes = "Eyes";

            /// <summary>
            /// Trait category for character's activities.
            /// </summary>
            public const string EngagesIn = "Engages in";

            /// <summary>
            /// Trait category for character's sexual activities.
            /// </summary>
            public const string EngagesInSexual = "Engages in (Sexual)";

            /// <summary>
            /// Trait category for character's hair features.
            /// </summary>
            public const string Hair = "Hair";

            /// <summary>
            /// Trait category for items associated with the character.
            /// </summary>
            public const string Items = "Items";

            /// <summary>
            /// Trait category for character's personality traits.
            /// </summary>
            public const string Personality = "Personality";

            /// <summary>
            /// Trait category for character's roles.
            /// </summary>
            public const string Role = "Role";

            /// <summary>
            /// Trait category for subjects related to the character.
            /// </summary>
            public const string SubjectOf = "Subject of";

            /// <summary>
            /// Trait category for subjects related to the character sexually.
            /// </summary>
            public const string SubjectOfSexual = "Subject of (Sexual)";
        }

        public static class RequestSort
        {
            public const string Id = "id";
            public const string Name = "name";
            public const string CharCount = "char_count";
            public const string SearchRank = "searchrank";
        }

    }
}