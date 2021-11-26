using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ImporterforAnilist.Models
{
    public class MediaList
    {
        [JsonProperty("data")]
        public MediaListData Data { get; set; }
    }

    public class MediaListData
    {
        [JsonProperty("list")]
        public DataList List { get; set; }
    }

    public class DataList
    {
        [JsonProperty("lists")]
        public List<ListElement> Lists { get; set; }

    }

    public class ListElement
    {
        [JsonProperty("status")]
        public EntryStatus? Status { get; set; }

        [JsonProperty("entries")]
        public List<Entry> Entries { get; set; }
    }

    public class Entry
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("progress")]
        public int Progress { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("status")]
        public EntryStatus? Status { get; set; }

        [JsonProperty("updatedAt")]
        public int UpdatedAt { get; set; }

        [JsonProperty("media")]
        public Media Media { get; set; }
    }

    public partial class Media
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("idMal")]
        public int? IdMal { get; set; }

        [JsonProperty("siteUrl")]
        public Uri SiteUrl { get; set; }

        [JsonProperty("type")]
        public TypeEnum Type { get; set; }

        [JsonProperty("format")]
        public Format? Format { get; set; }

        [JsonProperty("episodes")]
        public int? Episodes { get; set; }

        [JsonProperty("chapters")]
        public int? Chapters { get; set; }

        [JsonProperty("averageScore")]
        public int? AverageScore { get; set; }

        [JsonProperty("title")]
        public Title Title { get; set; }

        [JsonProperty("startDate")]
        public StartDate StartDate { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("genres")]
        public List<string> Genres { get; set; }

        [JsonProperty("tags")]
        public List<Tag> Tags { get; set; }

        [JsonProperty("season")]
        public Season? Season { get; set; }

        [JsonProperty("status")]
        public MediaStatus Status { get; set; }

        [JsonProperty("studios")]
        public Studios Studios { get; set; }

        [JsonProperty("staff")]
        public Staff Staff { get; set; }

        [JsonProperty("coverImage")]
        public CoverImage CoverImage { get; set; }

        [JsonProperty("bannerImage")]
        public string BannerImage { get; set; }

    }

    public partial class CoverImage
    {
        [JsonProperty("extraLarge")]
        public string ExtraLarge { get; set; }
    }

    public partial class Staff
    {
        [JsonProperty("nodes")]
        public StaffNode[] Nodes { get; set; }
    }

    public partial class StaffNode
    {
        [JsonProperty("name")]
        public StaffName Name { get; set; }
    }

    public partial class StaffName
    {
        [JsonProperty("full")]
        public string Full { get; set; }
    }

    public partial class Studios
    {
        [JsonProperty("nodes")]
        public StudiosNode[] Nodes { get; set; }
    }

    public partial class StudiosNode
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("isAnimationStudio")]
        public bool IsAnimationStudio { get; set; }
    }

    public partial class StartDate
    {
        [JsonProperty("year")]
        public int? Year { get; set; }

        [JsonProperty("month")]
        public int? Month { get; set; }

        [JsonProperty("day")]
        public int? Day { get; set; }
    }

    public partial class Tag
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("isGeneralSpoiler")]
        public bool IsGeneralSpoiler { get; set; }

        [JsonProperty("isMediaSpoiler")]
        public bool IsMediaSpoiler { get; set; }
    }

    public partial class Title
    {
        [JsonProperty("romaji")]
        public string Romaji { get; set; }

        [JsonProperty("english")]
        public string English { get; set; }

        [JsonProperty("native")]
        public string Native { get; set; }
    }

    public enum Format { Tv, Tv_Short, Movie, Special, Ova, Ona, Music, Manga, Novel, One_Shot };

    public enum Season { Fall, Spring, Summer, Winter };

    public enum MediaStatus { Finished, Releasing, Not_Yet_Released, Cancelled, Hiatus };

    public enum TypeEnum { Anime, Manga };

    public enum EntryStatus { Current, Planning, Completed, Dropped, Paused, Repeating };
}