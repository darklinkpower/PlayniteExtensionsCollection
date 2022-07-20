using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamCommon.Models
{
    public class StoreSearchResult : GenericItemOption
    {
        public string GameId { get; set; }
        public double PriceOriginal { get; set; }
        public double PriceFinal { get; set; }
        public bool IsDiscounted { get; set; }
        public bool IsFree { get; set; }
        public bool IsReleased { get; set; }
        public int DiscountPercentage { get; set; }
        public string StoreUrl { get; set; }
        public string BannerImageUrl { get; set; }
        public string Currency { get; set; }
    }

    public partial class SteamAppDetails
    {

        public class AppDetails
        {
            public class Category
            {
                public int id;
                public string description;
            }

            public class Genre
            {
                public string id;
                public string description;
            }

            public class ReleaseDate
            {
                public bool comming_soon;
                public string date;
            }

            public class Requirement
            {
                public string minimum;
                public string recommended;
            }

            public class Platforms
            {
                public bool windows;
                public bool mac;
                public bool linux;
            }

            public class Metacritic
            {
                public int score;
                public string url;
            }

            public class Screenshot
            {
                public int id;
                public string path_thumbnail;
                public string path_full;
            }

            public class Movie
            {
                [SerializationPropertyName("id")]
                public uint Id { get; set; }

                [SerializationPropertyName("name")]
                public string Name { get; set; }

                [SerializationPropertyName("thumbnail")]
                public Uri Thumbnail { get; set; }

                [SerializationPropertyName("webm")]
                public Mp4 Webm { get; set; }

                [SerializationPropertyName("mp4")]
                public Mp4 Mp4 { get; set; }

                [SerializationPropertyName("highlight")]
                public bool Highlight { get; set; }
            }

            public class Support
            {
                public string url;
                public string email;
            }

            public string type;
            public string name;
            public int steam_appid;
            public string required_age;
            public bool is_free;
            public List<int> dlc;
            public string header_image;
            public string background;
            public string detailed_description;
            public string about_the_game;
            public string short_description;
            public string supported_languages;
            public string website;
            public object pc_requirements;
            public object mac_requirements;
            public object linux_requirements;
            public List<Genre> genres;
            public ReleaseDate release_date;
            public List<string> developers;
            public List<string> publishers;
            public Platforms platforms;
            public Metacritic metacritic;
            public List<Category> categories;
            public List<Screenshot> screenshots;
            public Support support_info;

            [SerializationPropertyName("movies")]
            public List<Movie> Movies;
        }

        public bool success
        {
            get; set;
        }

        public AppDetails data
        {
            get; set;
        }

        public class Mp4
        {
            [SerializationPropertyName("480")]
            public Uri Q480 { get; set; }

            [SerializationPropertyName("max")]
            public Uri Max { get; set; }
        }
    }
}
