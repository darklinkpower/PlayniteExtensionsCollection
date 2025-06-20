using System;
using DatabaseCommon;
using Playnite.SDK.Data;
using ReviewViewer.Infrastructure;

namespace ReviewViewer.Domain
{
    public class ReviewsResponseRecord : IDatabaseItem<ReviewsResponseRecord>
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CacheKey { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ReviewsResponseDto Response { get; set; }

        public ReviewsResponseRecord GetClone()
        {
            return new ReviewsResponseRecord
            {
                Id = this.Id,
                CacheKey = this.CacheKey,
                CreatedAt = this.CreatedAt,
                Response = Serialization.GetClone(Response)
            };
        }
    }
}