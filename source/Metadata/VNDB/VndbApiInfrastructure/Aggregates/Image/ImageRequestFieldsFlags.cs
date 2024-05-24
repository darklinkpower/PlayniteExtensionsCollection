using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;

namespace VndbApiDomain.ImageAggregate
{
    [Flags]
    public enum ImageRequestFieldsFlags
    {
        /// <summary>
        /// Image identifier.
        /// </summary>
        [StringRepresentation(ImageConstants.Fields.Id)]
        Id = 1 << 0,

        /// <summary>
        /// URL.
        /// </summary>
        [StringRepresentation(ImageConstants.Fields.Url)]
        Url = 1 << 1,

        /// <summary>
        /// Pixel dimensions of the image, array with two integer elements indicating the width and height.
        /// </summary>
        [StringRepresentation(ImageConstants.Fields.Dims)]
        Dimensions = 1 << 2,

        /// <summary>
        /// Average flagging vote for sexual content (0 to 2).
        /// </summary>
        [StringRepresentation(ImageConstants.Fields.Sexual)]
        Sexual = 1 << 3,

        /// <summary>
        /// Average flagging vote for violence (0 to 2).
        /// </summary>
        [StringRepresentation(ImageConstants.Fields.Violence)]
        Violence = 1 << 4,

        /// <summary>
        /// Number of flagging votes.
        /// </summary>
        [StringRepresentation(ImageConstants.Fields.VoteCount)]
        VoteCount = 1 << 5,

        /// <summary>
        /// URL to the thumbnail.
        /// </summary>
        [StringRepresentation(ImageConstants.Fields.Thumbnail)]
        ThumbnailUrl = 1 << 6,

        /// <summary>
        /// Pixel dimensions of the thumbnail, array with two integer elements.
        /// </summary>
        [StringRepresentation(ImageConstants.Fields.ThumbnailDims)]
        ThumbnailDims = 1 << 7
    }
}