using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VndbApi.Domain.TraitAggregate
{
    public static class TraitConstants
    {
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

    }
}