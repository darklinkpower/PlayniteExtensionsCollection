using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCriticMetadata.Domain.Entities
{
    public class OpenCriticGameData
    {
        [SerializationPropertyName("reviewSummary")]
        public SummaryObject ReviewSummary { get; set; }

        [SerializationPropertyName("images")]
        public Images Images { get; set; }

        [SerializationPropertyName("Rating")]
        public Rating Rating { get; set; }

        [SerializationPropertyName("isPre2015")]
        public bool IsPre2015 { get; set; }

        [SerializationPropertyName("hasLootBoxes")]
        public bool? HasLootBoxes { get; set; }

        [SerializationPropertyName("isMajorTitle")]
        public bool IsMajorTitle { get; set; }

        [SerializationPropertyName("percentRecommended")]
        public double PercentRecommended { get; set; }

        [SerializationPropertyName("numReviews")]
        public uint NumReviews { get; set; }

        [SerializationPropertyName("numTopCriticReviews")]
        public uint NumTopCriticReviews { get; set; }

        [SerializationPropertyName("medianScore")]
        public double MedianScore { get; set; }

        [SerializationPropertyName("topCriticScore")]
        public double TopCriticScore { get; set; }

        [SerializationPropertyName("percentile")]
        public double Percentile { get; set; }

        [SerializationPropertyName("tier")]
        public string Tier { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("description")]
        public string Description { get; set; }

        [SerializationPropertyName("screenshots")]
        public OpencrocketScreenshot[] Screenshots { get; set; }

        [SerializationPropertyName("trailers")]
        public List<Trailer> Trailers { get; set; }

        [SerializationPropertyName("embargoDate")]
        public DateTimeOffset? EmbargoDate { get; set; }

        [SerializationPropertyName("monetizationFeatures")]
        public MonetizationFeatures MonetizationFeatures { get; set; }

        [SerializationPropertyName("Companies")]
        public List<Company> Companies { get; set; }

        [SerializationPropertyName("Platforms")]
        public List<Platform> Platforms { get; set; }

        [SerializationPropertyName("Genres")]
        public List<Genre> Genres { get; set; }

        [SerializationPropertyName("Affiliates")]
        public List<Affiliate> Affiliates { get; set; }

        [SerializationPropertyName("id")]
        public uint Id { get; set; }

        [SerializationPropertyName("firstReleaseDate")]
        public DateTimeOffset FirstReleaseDate { get; set; }

        [SerializationPropertyName("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [SerializationPropertyName("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }

        [SerializationPropertyName("firstReviewDate")]
        public DateTimeOffset? FirstReviewDate { get; set; }

        [SerializationPropertyName("latestReviewDate")]
        public DateTimeOffset? LatestReviewDate { get; set; }

        [SerializationPropertyName("bannerScreenshot")]
        public BannerScreenshotClass BannerScreenshot { get; set; }

        [SerializationPropertyName("oldObject")]
        public SummaryObject OldObject { get; set; }

        [SerializationPropertyName("squareScreenshot")]
        public SquareScreenshotClass SquareScreenshot { get; set; }

        [SerializationPropertyName("verticalLogoScreenshot")]
        public SquareScreenshotClass VerticalLogoScreenshot { get; set; }

        [SerializationPropertyName("mastheadScreenshot")]
        public BannerScreenshotClass MastheadScreenshot { get; set; }

        [SerializationPropertyName("imageMigrationComplete")]
        public bool ImageMigrationComplete { get; set; }

        [SerializationPropertyName("tenthReviewDate")]
        public DateTimeOffset TenthReviewDate { get; set; }

        [SerializationPropertyName("criticalReviewDate")]
        public DateTimeOffset CriticalReviewDate { get; set; }

        [SerializationPropertyName("biggestDiscountDollars")]
        public long BiggestDiscountDollars { get; set; }

        [SerializationPropertyName("biggestDiscountPercentage")]
        public long BiggestDiscountPercentage { get; set; }

        [SerializationPropertyName("needsAdminDealReview")]
        public bool NeedsAdminDealReview { get; set; }

        [SerializationPropertyName("tags")]
        public List<string> Tags { get; set; }

        [SerializationPropertyName("url")]
        public Uri Url { get; set; }
    }

    public class Affiliate
    {
        [SerializationPropertyName("externalUrl")]
        public Uri ExternalUrl { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }
    }

    public class BannerScreenshotClass
    {
        [SerializationPropertyName("fullRes")]
        public string FullRes { get; set; }
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

    public class Images
    {
        [SerializationPropertyName("box")]
        public Banner Box { get; set; }

        [SerializationPropertyName("square")]
        public Square Square { get; set; }

        [SerializationPropertyName("masthead")]
        public Masthead Masthead { get; set; }

        [SerializationPropertyName("banner")]
        public Banner Banner { get; set; }

        [SerializationPropertyName("screenshots")]
        public ImagesScreenshot[] Screenshots { get; set; }
    }

    public class Banner
    {
        [SerializationPropertyName("og")]
        public string Og { get; set; }

        [SerializationPropertyName("sm")]
        public string Sm { get; set; }
    }

    public class Masthead
    {
        [SerializationPropertyName("og")]
        public string Og { get; set; }

        [SerializationPropertyName("xs")]
        public string Xs { get; set; }

        [SerializationPropertyName("sm")]
        public string Sm { get; set; }

        [SerializationPropertyName("md")]
        public string Md { get; set; }

        [SerializationPropertyName("lg")]
        public string Lg { get; set; }

        [SerializationPropertyName("xl")]
        public string Xl { get; set; }
    }

    public class ImagesScreenshot
    {
        [SerializationPropertyName("og")]
        public string Og { get; set; }

        [SerializationPropertyName("sm")]
        public string Sm { get; set; }

        [SerializationPropertyName("_id")]
        public string Id { get; set; }
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

    public class MonetizationFeatures
    {
        [SerializationPropertyName("summary")]
        public string Summary { get; set; }

        [SerializationPropertyName("hasLootBoxes")]
        public bool HasLootBoxes { get; set; }
    }

    public partial class SummaryObject
    {
        [SerializationPropertyName("summary")]
        public string Summary { get; set; }
    }

    public class Platform
    {
        [SerializationPropertyName("id")]
        public long Id { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("shortName")]
        public string ShortName { get; set; }

        [SerializationPropertyName("imageSrc")]
        public Uri ImageSrc { get; set; }

        [SerializationPropertyName("releaseDate")]
        public DateTimeOffset ReleaseDate { get; set; }

        [SerializationPropertyName("displayRelease")]
        public object DisplayRelease { get; set; }
    }

    public class Rating
    {
        [SerializationPropertyName("value")]
        public string Value { get; set; }

        [SerializationPropertyName("imageSrc")]
        public Uri ImageSrc { get; set; }
    }

    public class OpencrocketScreenshot
    {
        [SerializationPropertyName("_id")]
        public string Id { get; set; }

        [SerializationPropertyName("fullRes")]
        public string FullRes { get; set; }

        [SerializationPropertyName("thumbnail")]
        public string Thumbnail { get; set; }
    }

    public class SquareScreenshotClass
    {
        [SerializationPropertyName("fullRes")]
        public string FullRes { get; set; }

        [SerializationPropertyName("thumbnail")]
        public string Thumbnail { get; set; }
    }

    public class Trailer
    {
        [SerializationPropertyName("title")]
        public string Title { get; set; }

        [SerializationPropertyName("videoId")]
        public string VideoId { get; set; }

        [SerializationPropertyName("channelId")]
        public string ChannelId { get; set; }

        [SerializationPropertyName("description")]
        public string Description { get; set; }

        [SerializationPropertyName("externalUrl")]
        public Uri ExternalUrl { get; set; }

        [SerializationPropertyName("channelTitle")]
        public string ChannelTitle { get; set; }

        [SerializationPropertyName("publishedDate")]
        public DateTimeOffset PublishedDate { get; set; }

        [SerializationPropertyName("isOpenCritic")]
        public bool IsOpenCritic { get; set; }

        [SerializationPropertyName("isSpecial")]
        public string IsSpecial { get; set; }
    }
}