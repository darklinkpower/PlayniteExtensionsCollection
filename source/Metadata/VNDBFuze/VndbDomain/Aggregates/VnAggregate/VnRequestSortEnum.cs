using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Attributes;

namespace VNDBFuze.VndbDomain.Aggregates.VnAggregate
{
    [Flags]
    public enum VnRequestSortEnum : ulong
    {
        /// <summary>
        /// Sort by ID.
        /// </summary>
        [StringRepresentation(VnConstants.RequestSort.Id)]
        Id = 0,

        /// <summary>
        /// Sort by title.
        /// </summary>
        [StringRepresentation(VnConstants.RequestSort.Title)]
        Title = 1UL << 1,

        /// <summary>
        /// Sort by release date.
        /// </summary>
        [StringRepresentation(VnConstants.RequestSort.Released)]
        Released = 1UL << 2,

        /// <summary>
        /// Sort by rating.
        /// </summary>
        [StringRepresentation(VnConstants.RequestSort.Rating)]
        Rating = 1UL << 3,

        /// <summary>
        /// Sort by vote count.
        /// </summary>
        [StringRepresentation(VnConstants.RequestSort.VoteCount)]
        VoteCount = 1UL << 4,

        /// <summary>
        /// Sort by search rank.
        /// </summary>
        [StringRepresentation(VnConstants.RequestSort.SearchRank)]
        SearchRank = 1UL << 5
    }
}