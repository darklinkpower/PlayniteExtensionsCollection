using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;

namespace VndbApiInfrastructure.VisualNovelAggregate
{
    [Flags]
    public enum VnRequestSortEnum : ulong
    {
        /// <summary>
        /// Sort by ID.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.RequestSort.Id)]
        Id = 0,

        /// <summary>
        /// Sort by title.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.RequestSort.Title)]
        Title = 1UL << 1,

        /// <summary>
        /// Sort by release date.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.RequestSort.Released)]
        Released = 1UL << 2,

        /// <summary>
        /// Sort by rating.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.RequestSort.Rating)]
        Rating = 1UL << 3,

        /// <summary>
        /// Sort by vote count.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.RequestSort.VoteCount)]
        VoteCount = 1UL << 4,

        /// <summary>
        /// Sort by search rank.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.RequestSort.SearchRank)]
        SearchRank = 1UL << 5
    }
}