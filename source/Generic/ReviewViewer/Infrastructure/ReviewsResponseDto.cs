using Playnite.SDK.Data;
using System;
using System.Collections.Generic;

namespace ReviewViewer.Infrastructure
{
    public class ReviewsResponseDto
    {
        [SerializationPropertyName("success")]
        public int Success { get; set; }

        [SerializationPropertyName("query_summary")]
        public QuerySummary QuerySummary { get; set; }

        [SerializationPropertyName("reviews")]
        public List<Review> Reviews { get; set; }

        [SerializationPropertyName("cursor")]
        public string Cursor { get; set; }
    }

    public class QuerySummary
    {
        [SerializationPropertyName("num_reviews")]
        public long NumReviews { get; set; }

        [SerializationPropertyName("review_score")]
        public long ReviewScore { get; set; }

        [SerializationPropertyName("review_score_desc")]
        public string ReviewScoreDesc { get; set; }

        [SerializationPropertyName("total_positive")]
        public int TotalPositive { get; set; }

        [SerializationPropertyName("total_negative")]
        public int TotalNegative { get; set; }

        [SerializationPropertyName("total_reviews")]
        public int TotalReviews { get; set; }
    }

    public class Review
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
        public int VotesUp { get; set; }

        [SerializationPropertyName("votes_funny")]
        public int VotesFunny { get; set; }

        [SerializationPropertyName("weighted_vote_score")]
        public string WeightedVoteScore { get; set; }

        [SerializationPropertyName("comment_count")]
        public int CommentCount { get; set; }

        [SerializationPropertyName("steam_purchase")]
        public bool SteamPurchase { get; set; }

        [SerializationPropertyName("received_for_free")]
        public bool ReceivedForFree { get; set; }

        [SerializationPropertyName("written_during_early_access")]
        public bool WrittenDuringEarlyAccess { get; set; }

        [SerializationPropertyName("primarily_steam_deck")]
        public bool PrimarilySteamDeck { get; set; }

        [SerializationPropertyName("hardware")]
        public Hardware Hardware { get; set; }

        [SerializationPropertyName("csgo_disclaimer")]
        public bool CsgoDisclaimer { get; set; }
    }

    public class Author
    {
        [SerializationPropertyName("steamid")]
        public string Steamid { get; set; }

        [SerializationPropertyName("personaname")]
        public string PersonaName { get; set; }

        [SerializationPropertyName("persona_status")]
        public PersonaStatus PersonaStatus { get; set; }

        [SerializationPropertyName("profile_url")]
        public Uri ProfileUrl { get; set; }

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

        [SerializationPropertyName("avatar")]
        public Uri Avatar { get; set; }

        [SerializationPropertyName("deck_playtime_at_review")]
        public long? DeckPlaytimeAtReview { get; set; }
    }

    public partial class Reaction
    {
        [SerializationPropertyName("reaction_type")]
        public uint ReactionType { get; set; }

        [SerializationPropertyName("count")]
        public uint Count { get; set; }
    }

    public partial class Hardware
    {
        [SerializationPropertyName("manufacturer")]
        public string Manufacturer { get; set; }

        [SerializationPropertyName("model")]
        public string Model { get; set; }

        [SerializationPropertyName("dx_video_card")]
        public string DxVideoCard { get; set; }

        [SerializationPropertyName("dx_vendorid")]
        public uint DxVendorid { get; set; }

        [SerializationPropertyName("dx_deviceid")]
        public uint DxDeviceid { get; set; }

        [SerializationPropertyName("num_gpu")]
        public uint NumGpu { get; set; }

        [SerializationPropertyName("system_ram")]
        public string SystemRam { get; set; }

        [SerializationPropertyName("os")]
        public string Os { get; set; }

        [SerializationPropertyName("cpu_vendor")]
        public string CpuVendor { get; set; }

        [SerializationPropertyName("cpu_name")]
        public string CpuName { get; set; }

        [SerializationPropertyName("gaming_device_type")]
        public uint GamingDeviceType { get; set; }

        [SerializationPropertyName("dx_driver_version")]
        public string DxDriverVersion { get; set; }

        [SerializationPropertyName("dx_driver_name")]
        public string DxDriverName { get; set; }

        [SerializationPropertyName("adapter_description")]
        public string AdapterDescription { get; set; }

        [SerializationPropertyName("driver_version")]
        public string DriverVersion { get; set; }

        [SerializationPropertyName("driver_date")]
        public string DriverDate { get; set; }

        [SerializationPropertyName("vram_size")]
        public uint VramSize { get; set; }
    }

    public enum PersonaStatus { InGame, Offline, Online };
}