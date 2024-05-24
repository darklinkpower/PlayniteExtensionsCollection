using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VndbApi.Domain.VisualNovelAggregate
{
    public static class VisualNovelConstants
    {
        /// <summary>
        /// Constants representing different lengths of visual novels.
        /// </summary>
        public static class Length
        {
            /// <summary>
            /// Indicates a very short visual novel.
            /// </summary>
            public const int VeryShort = 1;

            /// <summary>
            /// Indicates a short visual novel.
            /// </summary>
            public const int Short = 2;

            /// <summary>
            /// Indicates a medium-length visual novel.
            /// </summary>
            public const int Medium = 3;

            /// <summary>
            /// Indicates a long visual novel.
            /// </summary>
            public const int Long = 4;

            /// <summary>
            /// Indicates a very long visual novel.
            /// </summary>
            public const int VeryLong = 5;
        }

        public static class DevelopmentStatus
        {
            /// <summary>
            /// Indicates that the visual novel is finished.
            /// </summary>
            public const int Finished = 0;

            /// <summary>
            /// Indicates that the visual novel is in development.
            /// </summary>
            public const int InDevelopment = 1;

            /// <summary>
            /// Indicates that the visual novel has been cancelled.
            /// </summary>
            public const int Cancelled = 2;
        }

        /// <summary>
        /// Constants representing different types of relationships of visual novels.
        /// </summary>
        public static class RelationType
        {
            /// <summary>
            /// Alternative version of the same visual novel.
            /// </summary>
            public const string AlternativeVersion = "alt";

            /// <summary>
            /// Visual novel that shares characters with another visual novel.
            /// </summary>
            public const string SharesCharacters = "char";

            /// <summary>
            /// Fandisc, usually additional content or a side story for an existing visual novel.
            /// </summary>
            public const string Fandisc = "fan";

            /// <summary>
            /// The original game that a visual novel is based on.
            /// </summary>
            public const string OriginalGame = "orig";

            /// <summary>
            /// Parent story from which the current visual novel is derived.
            /// </summary>
            public const string ParentStory = "par";

            /// <summary>
            /// Prequel, a story that precedes the current visual novel.
            /// </summary>
            public const string Prequel = "preq";

            /// <summary>
            /// Sequel, a story that follows the current visual novel.
            /// </summary>
            public const string Sequel = "seq";

            /// <summary>
            /// Visual novel that is part of the same series.
            /// </summary>
            public const string SameSeries = "ser";

            /// <summary>
            /// Visual novel that is set in the same universe or setting.
            /// </summary>
            public const string SameSetting = "set";

            /// <summary>
            /// Side story, a narrative that runs parallel or provides additional context to the main story.
            /// </summary>
            public const string SideStory = "side";
        }
    }
}