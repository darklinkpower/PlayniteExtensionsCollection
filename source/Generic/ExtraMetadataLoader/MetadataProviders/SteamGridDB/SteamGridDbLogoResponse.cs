using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtraMetadataLoader.Models
{
    public class SteamGridDbLogoResponse
    {
        [SerializationPropertyName("success")]
        public bool Success { get; set; }

        [SerializationPropertyName("data")]
        public List<Data> Data { get; set; }
    }

    public class Data
    {
        [SerializationPropertyName("id")]
        public long Id { get; set; }

        [SerializationPropertyName("score")]
        public long Score { get; set; }

        [SerializationPropertyName("style")]
        public string Style { get; set; }

        [SerializationPropertyName("width")]
        public long Width { get; set; }

        [SerializationPropertyName("height")]
        public long Height { get; set; }

        [SerializationPropertyName("nsfw")]
        public bool Nsfw { get; set; }

        [SerializationPropertyName("humor")]
        public bool Humor { get; set; }

        [SerializationPropertyName("notes")]
        public object Notes { get; set; }

        [SerializationPropertyName("mime")]
        public string Mime { get; set; }

        [SerializationPropertyName("language")]
        public string Language { get; set; }

        [SerializationPropertyName("url")]
        public string Url { get; set; }

        [SerializationPropertyName("thumb")]
        public string Thumb { get; set; }

        [SerializationPropertyName("lock")]
        public bool Lock { get; set; }

        [SerializationPropertyName("epilepsy")]
        public bool Epilepsy { get; set; }

        [SerializationPropertyName("upvotes")]
        public long Upvotes { get; set; }

        [SerializationPropertyName("downvotes")]
        public long Downvotes { get; set; }

        [SerializationPropertyName("author")]
        public Author Author { get; set; }
    }

    public partial class Author
    {
        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("steam64")]
        public string Steam64 { get; set; }

        [SerializationPropertyName("avatar")]
        public Uri Avatar { get; set; }
    }
}
