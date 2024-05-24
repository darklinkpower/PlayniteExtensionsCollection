using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VndbApi.Domain.ReleaseAggregate
{
    public static class ReleaseConstants
    {
        public static class ReleaseType
        {
            /// <summary>
            /// The release type for this visual novel, can be "trial", "partial" or "complete".
            /// </summary>
            public const string Trial = "trial";

            /// <summary>
            /// The release type for this visual novel, can be "trial", "partial" or "complete".
            /// </summary>
            public const string Partial = "partial";

            /// <summary>
            /// The release type for this visual novel, can be "trial", "partial" or "complete".
            /// </summary>
            public const string Complete = "complete";
        }

        public static class Voiced
        {
            /// <summary>
            /// Not voiced.
            /// </summary>
            public const int NotVoiced = 1;

            /// <summary>
            /// Only ero scenes voiced.
            /// </summary>
            public const int EroScenesVoiced = 2;

            /// <summary>
            /// Partially voiced.
            /// </summary>
            public const int PartiallyVoiced = 3;

            /// <summary>
            /// Fully voiced.
            /// </summary>
            public const int FullyVoiced = 4;
        }
    }
}