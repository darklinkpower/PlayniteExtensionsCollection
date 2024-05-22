using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBFuze.VndbDomain.Aggregates.ImageAggregate
{
    public static class ImageConstants
    {
        public static class Fields
        {
            /// <summary>
            /// Image identifier.
            /// </summary>
            public const string Id = "id";

            /// <summary>
            ///  URL.
            /// </summary>
            public const string Url = "url";

            /// <summary>
            /// Pixel dimensions of the , array with two integer elements indicating the width and height.
            /// </summary>
            public const string Dims = "dims";

            /// <summary>
            /// Average flagging vote for sexual content (0 to 2).
            /// </summary>
            public const string Sexual = "sexual";

            /// <summary>
            /// Average flagging vote for violence (0 to 2).
            /// </summary>
            public const string Violence = "violence";

            /// <summary>
            /// Number of flagging votes.
            /// </summary>
            public const string VoteCount = "votecount";

            /// <summary>
            /// URL to the thumbnail.
            /// </summary>
            public const string Thumbnail = "thumbnail";

            /// <summary>
            /// Pixel dimensions of the thumbnail, array with two integer elements.
            /// </summary>
            public const string ThumbnailDims = "thumbnail_dims";
        }
    }


}