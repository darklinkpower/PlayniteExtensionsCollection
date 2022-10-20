using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCriticMetadata.Models
{
    public class OpenCriticGameData
    {
        [SerializationPropertyName("images")]
        public Images Images { get; set; }

        [SerializationPropertyName("mainChannel")]
        public MainChannel MainChannel { get; set; }

        [SerializationPropertyName("Rating")]
        public Rating Rating { get; set; }

        [SerializationPropertyName("imageMigrationComplete")]
        public bool ImageMigrationComplete { get; set; }

        [SerializationPropertyName("hasLootBoxes")]
        public bool? HasLootBoxes { get; set; }

        [SerializationPropertyName("percentRecommended")]
        public double PercentRecommended { get; set; }

        [SerializationPropertyName("numReviews")]
        public int NumReviews { get; set; }

        [SerializationPropertyName("numTopCriticReviews")]
        public int NumTopCriticReviews { get; set; }

        [SerializationPropertyName("medianScore")]
        public double MedianScore { get; set; }

        [SerializationPropertyName("topCriticScore")]
        public double TopCriticScore { get; set; }

        [SerializationPropertyName("percentile")]
        public double Percentile { get; set; }

        [SerializationPropertyName("tier")]
        public string Tier { get; set; }

        [SerializationPropertyName("Platforms")]
        public Platform[] Platforms { get; set; }

        [SerializationPropertyName("Genres")]
        public Genre[] Genres { get; set; }

        [SerializationPropertyName("Companies")]
        public Company[] Companies { get; set; }

        [SerializationPropertyName("trailers")]
        public Trailer[] Trailers { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("description")]
        public string Description { get; set; }

        [SerializationPropertyName("id")]
        public long Id { get; set; }

        [SerializationPropertyName("firstReleaseDate")]
        public DateTimeOffset? FirstReleaseDate { get; set; }

        [SerializationPropertyName("Affiliates")]
        public Affiliate[] Affiliates { get; set; }

        [SerializationPropertyName("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [SerializationPropertyName("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }

        [SerializationPropertyName("firstReviewDate")]
        public DateTimeOffset? FirstReviewDate { get; set; }

        [SerializationPropertyName("latestReviewDate")]
        public DateTimeOffset? LatestReviewDate { get; set; }

        [SerializationPropertyName("url")]
        public Uri Url { get; set; }
    }

    public partial class Affiliate
    {
        [SerializationPropertyName("externalUrl")]
        public Uri ExternalUrl { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }
    }

    public class Company
    {
        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("type")]
        public string Type { get; set; }
    }

    public class Genre
    {
        [SerializationPropertyName("id")]
        public long Id { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }
    }

    public partial class Images
    {
        [SerializationPropertyName("box")]
        public Banner Box { get; set; }

        [SerializationPropertyName("square")]
        public Square Square { get; set; }

        [SerializationPropertyName("banner")]
        public Banner Banner { get; set; }

        [SerializationPropertyName("screenshots")]
        public Screenshot[] Screenshots { get; set; }
    }

    public class Banner
    {
        [SerializationPropertyName("og")]
        public string Og { get; set; }

        [SerializationPropertyName("sm")]
        public string Sm { get; set; }
    }

    public class Screenshot
    {
        [SerializationPropertyName("_id")]
        public string Id { get; set; }

        [SerializationPropertyName("og")]
        public string Og { get; set; }

        [SerializationPropertyName("sm")]
        public string Sm { get; set; }
    }

    public class Square
    {
        [SerializationPropertyName("og")]
        public string Og { get; set; }

        [SerializationPropertyName("xs")]
        public string Xs { get; set; }

        [SerializationPropertyName("sm")]
        public string Sm { get; set; }

        [SerializationPropertyName("lg")]
        public string Lg { get; set; }
    }

    public class MainChannel
    {
        [SerializationPropertyName("channelId")]
        public string ChannelId { get; set; }

        [SerializationPropertyName("channelTitle")]
        public string ChannelTitle { get; set; }

        [SerializationPropertyName("description")]
        public string Description { get; set; }

        [SerializationPropertyName("title")]
        public string Title { get; set; }

        [SerializationPropertyName("image")]
        public Uri Image { get; set; }

        [SerializationPropertyName("externalUrl")]
        public Uri ExternalUrl { get; set; }
    }

    public class Platform
    {
        [SerializationPropertyName("id")]
        public long Id { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("shortName")]
        public string ShortName { get; set; }

        [SerializationPropertyName("releaseDate")]
        public DateTimeOffset ReleaseDate { get; set; }
    }

    public class Rating
    {
        [SerializationPropertyName("value")]
        public string Value { get; set; }
    }

    public class Trailer
    {
        [SerializationPropertyName("publishedDate")]
        public DateTimeOffset PublishedDate { get; set; }

        [SerializationPropertyName("title")]
        public string Title { get; set; }

        [SerializationPropertyName("videoId")]
        public string VideoId { get; set; }

        [SerializationPropertyName("externalUrl")]
        public Uri ExternalUrl { get; set; }

        [SerializationPropertyName("channelTitle")]
        public string ChannelTitle { get; set; }

        [SerializationPropertyName("channelId")]
        public string ChannelId { get; set; }

        [SerializationPropertyName("description")]
        public string Description { get; set; }
    }
}