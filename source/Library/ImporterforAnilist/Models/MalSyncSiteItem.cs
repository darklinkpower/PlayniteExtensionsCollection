using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImporterforAnilist.Models
{
    public class MalSyncResponse
    {
        [SerializationPropertyName("id")]
        public int Id { get; set; }

        [SerializationPropertyName("malId")]
        public int? MalId { get; set; }

        [SerializationPropertyName("type")]
        public string Type { get; set; }

        [SerializationPropertyName("title")]
        public string Title { get; set; }

        [SerializationPropertyName("url")]
        public Uri Url { get; set; }

        [SerializationPropertyName("image")]
        public Uri Image { get; set; }

        [SerializationPropertyName("Sites")]
        public Dictionary<string, Dictionary<string, MalSyncSiteItem>> Sites { get; set; }

        [SerializationPropertyName("ttl")]
        public long Ttl { get; set; }
    }

    public class MalSyncSiteItem
    {
        [SerializationPropertyName("identifier")]
        public string Identifier { get; set; }

        [SerializationPropertyName("type")]
        public string Type { get; set; }

        [SerializationPropertyName("page")]
        public string Page { get; set; }

        [SerializationPropertyName("title")]
        public string Title { get; set; }

        [SerializationPropertyName("url")]
        public string Url { get; set; }

        [SerializationPropertyName("image")]
        public string Image { get; set; }

        [SerializationPropertyName("malId")]
        public int? MalId { get; set; }

        [SerializationPropertyName("aniId")]
        public int? AniId { get; set; }
    }
}