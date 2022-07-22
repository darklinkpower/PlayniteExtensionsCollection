using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Models
{
    public class SteamWishlistItem
    {
        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("capsule")]
        public Uri Capsule { get; set; }

        [SerializationPropertyName("review_score")]
        public long ReviewScore { get; set; }

        [SerializationPropertyName("review_desc")]
        public string ReviewDesc { get; set; }

        [SerializationPropertyName("reviews_total")]
        public string ReviewsTotal { get; set; }

        [SerializationPropertyName("reviews_percent")]
        public long ReviewsPercent { get; set; }

        [SerializationPropertyName("release_date")]
        public double? ReleaseDate { get; set; }

        [SerializationPropertyName("release_string")]
        public string ReleaseString { get; set; }

        [SerializationPropertyName("platform_icons")]
        public string PlatformIcons { get; set; }

        [SerializationPropertyName("subs")]
        public Sub[] Subs { get; set; }

        [SerializationPropertyName("type")]
        public StoreItemType Type { get; set; }

        [SerializationPropertyName("screenshots")]
        public string[] Screenshots { get; set; }

        [SerializationPropertyName("review_css")]
        public string ReviewCss { get; set; }

        [SerializationPropertyName("priority")]
        public long Priority { get; set; }

        [SerializationPropertyName("added")]
        public long Added { get; set; }

        [SerializationPropertyName("background")]
        public Uri Background { get; set; }

        [SerializationPropertyName("rank")]
        public long Rank { get; set; }

        [SerializationPropertyName("tags")]
        public string[] Tags { get; set; }

        [SerializationPropertyName("is_free_game")]
        public bool IsFreeGame { get; set; }

        [SerializationPropertyName("win")]
        public long Win { get; set; }
    }

    public partial class Sub
    {
        [SerializationPropertyName("id")]
        public double Id { get; set; }

        [SerializationPropertyName("discount_block")]
        public string DiscountBlock { get; set; }

        [SerializationPropertyName("discount_pct")]
        public double DiscountPct { get; set; }

        [SerializationPropertyName("price")]
        public double Price { get; set; }
    }

    public enum ReviewCss { Mixed, NoReviews, Positive };

    public enum ReviewDesc { Mixed, MostlyPositive, NoUserReviews, Overwhelmingly_Positive, OverwhelminglyPositive, Positive, VeryPositive };

    public enum StoreItemType
    {
        Game = 0,
        Dlc = 1,
        Music = 2,
        Application = 3,
        Series = 4,
        Video = 5,
        Hardware = 6,
        Mod = 7,
        Demo = 8,
        Tool = 9
    };

    public partial struct ReleaseDate
    {
        public long? Integer;
        public string String;

        public static implicit operator ReleaseDate(long Integer) => new ReleaseDate { Integer = Integer };
        public static implicit operator ReleaseDate(string String) => new ReleaseDate { String = String };
    }
}