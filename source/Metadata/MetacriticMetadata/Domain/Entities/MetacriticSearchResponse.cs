using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetacriticMetadata.Domain.Entities
{
    public class MetacriticSearchResponse
    {
        [SerializationPropertyName("data")]
        public Data Data { get; set; }

        [SerializationPropertyName("links")]
        public Links Links { get; set; }

        [SerializationPropertyName("meta")]
        public MetacriticWebMeta Meta { get; set; }
    }

    public class Data
    {
        [SerializationPropertyName("id")]
        public Guid Id { get; set; }

        [SerializationPropertyName("totalResults")]
        public int TotalResults { get; set; }

        [SerializationPropertyName("items")]
        public Item[] Items { get; set; }
    }

    public class Item
    {
        [SerializationPropertyName("id")]
        public long Id { get; set; }

        [SerializationPropertyName("type")]
        public string Type { get; set; }

        [SerializationPropertyName("typeId")]
        public int TypeId { get; set; }

        [SerializationPropertyName("title")]
        public string Title { get; set; }

        [SerializationPropertyName("slug")]
        public string Slug { get; set; }

        [SerializationPropertyName("images")]
        public Image[] Images { get; set; }

        [SerializationPropertyName("criticScoreSummary")]
        public CriticScoreSummary CriticScoreSummary { get; set; }

        [SerializationPropertyName("rating")]
        public string Rating { get; set; }

        [SerializationPropertyName("releaseDate")]
        public string ReleaseDate { get; set; }

        [SerializationPropertyName("premiereYear")]
        public int PremiereYear { get; set; }

        [SerializationPropertyName("genres")]
        public MetacriticGenre[] Genres { get; set; }

        [SerializationPropertyName("platforms")]
        public MetacriticPlatform[] Platforms { get; set; }

        [SerializationPropertyName("seasonCount")]
        public int SeasonCount { get; set; }

        [SerializationPropertyName("description")]
        public string Description { get; set; }

        //[SerializationPropertyName("duration")]
        //public object Duration { get; set; }

        [SerializationPropertyName("mustSee")]
        public bool MustSee { get; set; }

        [SerializationPropertyName("mustWatch")]
        public bool MustWatch { get; set; }

        [SerializationPropertyName("mustPlay")]
        public bool MustPlay { get; set; }
    }

    public class CriticScoreSummary
    {
        [SerializationPropertyName("url")]
        public string Url { get; set; }

        [SerializationPropertyName("score")]
        public int? Score { get; set; }
    }

    public class MetacriticGenre
    {
        [SerializationPropertyName("id")]
        public string Id { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }
    }

    public class MetacriticPlatform
    {
        [SerializationPropertyName("id")]
        public string Id { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }
    }

    public class Image
    {
        [SerializationPropertyName("id")]
        public string Id { get; set; }

        [SerializationPropertyName("filename")]
        public string Filename { get; set; }

        //[SerializationPropertyName("dateCreated")]
        //public DateCreated DateCreated { get; set; }

        //[SerializationPropertyName("alt")]
        //public object Alt { get; set; }

        //[SerializationPropertyName("credits")]
        //public object Credits { get; set; }

        //[SerializationPropertyName("path")]
        //public object Path { get; set; }

        //[SerializationPropertyName("cropGravity")]
        //public object CropGravity { get; set; }

        //[SerializationPropertyName("crop")]
        //public object Crop { get; set; }

        //[SerializationPropertyName("caption")]
        //public object Caption { get; set; }

        [SerializationPropertyName("typeName")]
        public TypeName TypeName { get; set; }

        //[SerializationPropertyName("imageUrl")]
        //public object ImageUrl { get; set; }

        [SerializationPropertyName("width")]
        public long Width { get; set; }

        [SerializationPropertyName("height")]
        public long Height { get; set; }

        //[SerializationPropertyName("sType")]
        //public object SType { get; set; }

        [SerializationPropertyName("bucketType")]
        public BucketType BucketType { get; set; }

        [SerializationPropertyName("bucketPath")]
        public string BucketPath { get; set; }

        //[SerializationPropertyName("mediaType")]
        //public object MediaType { get; set; }

        [SerializationPropertyName("provider")]
        public int Provider { get; set; }
    }

    //public class DateCreated
    //{
    //    [SerializationPropertyName("date")]
    //    public object Date { get; set; }

    //    [SerializationPropertyName("timezone")]
    //    public object Timezone { get; set; }
    //}

    public class Links
    {
        [SerializationPropertyName("self")]
        public MetacriticLink Self { get; set; }

        [SerializationPropertyName("prev")]
        public MetacriticLink Prev { get; set; }

        [SerializationPropertyName("next")]
        public MetacriticLink Next { get; set; }

        [SerializationPropertyName("first")]
        public MetacriticLink First { get; set; }

        [SerializationPropertyName("last")]
        public MetacriticLink Last { get; set; }

        [SerializationPropertyName("sortOptions")]
        public SortOption[] SortOptions { get; set; }
    }

    public class MetacriticLink
    {
        [SerializationPropertyName("href")]
        public Uri Href { get; set; }

        [SerializationPropertyName("meta")]
        public FirstMeta Meta { get; set; }
    }

    public class FirstMeta
    {
        [SerializationPropertyName("pageNum")]
        public long PageNum { get; set; }

        [SerializationPropertyName("count")]
        public long Count { get; set; }
    }

    public class SortOption
    {
        [SerializationPropertyName("label")]
        public string Label { get; set; }

        [SerializationPropertyName("href")]
        public Uri Href { get; set; }
    }

    public class MetacriticWebMeta
    {
        [SerializationPropertyName("componentName")]
        public string ComponentName { get; set; }

        [SerializationPropertyName("componentDisplayName")]
        public string ComponentDisplayName { get; set; }

        [SerializationPropertyName("componentType")]
        public string ComponentType { get; set; }
    }

    public enum BucketType { Catalog };

    public enum TypeName { CardImage };

    public enum ItemType { GameTitle, Movie, Show };

}