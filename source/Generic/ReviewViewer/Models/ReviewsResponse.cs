using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK.Data;

namespace ReviewViewer.Models
{
    public partial class ReviewsResponse
    {
        [SerializationPropertyName("success")]
        public int Success { get; set; }

        [SerializationPropertyName("query_summary")]
        public QuerySummary QuerySummary { get; set; }

        [SerializationPropertyName("reviews")]
        public Review[] Reviews { get; set; }

        [SerializationPropertyName("cursor")]
        public string Cursor { get; set; }
    }

    public partial class QuerySummary
    {
        [SerializationPropertyName("num_reviews")]
        public long NumReviews { get; set; }

        [SerializationPropertyName("review_score")]
        public long ReviewScore { get; set; }

        [SerializationPropertyName("review_score_desc")]
        public string ReviewScoreDesc { get; set; }

        [SerializationPropertyName("total_positive")]
        public long TotalPositive { get; set; }

        [SerializationPropertyName("total_negative")]
        public long TotalNegative { get; set; }

        [SerializationPropertyName("total_reviews")]
        public long TotalReviews { get; set; }
    }

    public partial class Review
    {
        [SerializationPropertyName("recommendationid")]
        public long Recommendationid { get; set; }

        [SerializationPropertyName("author")]
        public Author Author { get; set; }

        [SerializationPropertyName("language")]
        public string Language { get; set; }

        [SerializationPropertyName("review")]
        public string ReviewReview { get; set; }

        [SerializationPropertyName("timestamp_created")]
        public long TimestampCreated { get; set; }

        [SerializationPropertyName("timestamp_updated")]
        public long TimestampUpdated { get; set; }

        [SerializationPropertyName("voted_up")]
        public bool VotedUp { get; set; }

        [SerializationPropertyName("votes_up")]
        public long VotesUp { get; set; }

        [SerializationPropertyName("votes_funny")]
        public long VotesFunny { get; set; }

        [SerializationPropertyName("weighted_vote_score")]
        public string WeightedVoteScore { get; set; }

        [SerializationPropertyName("comment_count")]
        public long CommentCount { get; set; }

        [SerializationPropertyName("steam_purchase")]
        public bool SteamPurchase { get; set; }

        [SerializationPropertyName("received_for_free")]
        public bool ReceivedForFree { get; set; }

        [SerializationPropertyName("written_during_early_access")]
        public bool WrittenDuringEarlyAccess { get; set; }
    }

    public partial class Author
    {
        [SerializationPropertyName("steamid")]
        public string Steamid { get; set; }

        [SerializationPropertyName("num_games_owned")]
        public long NumGamesOwned { get; set; }

        [SerializationPropertyName("num_reviews")]
        public long NumReviews { get; set; }

        [SerializationPropertyName("playtime_forever")]
        public long PlaytimeForever { get; set; }

        [SerializationPropertyName("playtime_last_two_weeks")]
        public long PlaytimeLastTwoWeeks { get; set; }

        [SerializationPropertyName("playtime_at_review")]
        public long PlaytimeAtReview { get; set; }

        [SerializationPropertyName("last_played")]
        public long LastPlayed { get; set; }
    }
}