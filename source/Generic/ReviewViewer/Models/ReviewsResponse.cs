using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewViewer.Models
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class ReviewsResponse
    {
        [JsonProperty("success")]
        public int Success { get; set; }

        [JsonProperty("query_summary")]
        public QuerySummary QuerySummary { get; set; }

        [JsonProperty("reviews")]
        public Review[] Reviews { get; set; }

        [JsonProperty("cursor")]
        public string Cursor { get; set; }
    }

    public partial class QuerySummary
    {
        [JsonProperty("num_reviews")]
        public long NumReviews { get; set; }

        [JsonProperty("review_score")]
        public long ReviewScore { get; set; }

        [JsonProperty("review_score_desc")]
        public string ReviewScoreDesc { get; set; }

        [JsonProperty("total_positive")]
        public long TotalPositive { get; set; }

        [JsonProperty("total_negative")]
        public long TotalNegative { get; set; }

        [JsonProperty("total_reviews")]
        public long TotalReviews { get; set; }
    }

    public partial class Review
    {
        [JsonProperty("recommendationid")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Recommendationid { get; set; }

        [JsonProperty("author")]
        public Author Author { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("review")]
        public string ReviewReview { get; set; }

        [JsonProperty("timestamp_created")]
        public long TimestampCreated { get; set; }

        [JsonProperty("timestamp_updated")]
        public long TimestampUpdated { get; set; }

        [JsonProperty("voted_up")]
        public bool VotedUp { get; set; }

        [JsonProperty("votes_up")]
        public long VotesUp { get; set; }

        [JsonProperty("votes_funny")]
        public long VotesFunny { get; set; }

        [JsonProperty("weighted_vote_score")]
        public string WeightedVoteScore { get; set; }

        [JsonProperty("comment_count")]
        public long CommentCount { get; set; }

        [JsonProperty("steam_purchase")]
        public bool SteamPurchase { get; set; }

        [JsonProperty("received_for_free")]
        public bool ReceivedForFree { get; set; }

        [JsonProperty("written_during_early_access")]
        public bool WrittenDuringEarlyAccess { get; set; }
    }

    public partial class Author
    {
        [JsonProperty("steamid")]
        public string Steamid { get; set; }

        [JsonProperty("num_games_owned")]
        public long NumGamesOwned { get; set; }

        [JsonProperty("num_reviews")]
        public long NumReviews { get; set; }

        [JsonProperty("playtime_forever")]
        public long PlaytimeForever { get; set; }

        [JsonProperty("playtime_last_two_weeks")]
        public long PlaytimeLastTwoWeeks { get; set; }

        [JsonProperty("playtime_at_review")]
        public long PlaytimeAtReview { get; set; }

        [JsonProperty("last_played")]
        public long LastPlayed { get; set; }
    }

    public enum Language { English };

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
        {
            LanguageConverter.Singleton,
            new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
        },
        };
    }

    internal class LanguageConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Language) || t == typeof(Language?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "english")
            {
                return Language.English;
            }
            throw new Exception("Cannot unmarshal type Language");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Language)untypedValue;
            if (value == Language.English)
            {
                serializer.Serialize(writer, "english");
                return;
            }
            throw new Exception("Cannot marshal type Language");
        }

        public static readonly LanguageConverter Singleton = new LanguageConverter();
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}
