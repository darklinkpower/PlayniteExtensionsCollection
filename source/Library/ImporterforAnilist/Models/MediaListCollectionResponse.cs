using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK.Data;

namespace ImporterforAnilist.Models
{
    public class MediaList
    {
        [SerializationPropertyName("data")]
        public MediaListData Data { get; set; }
    }

    public class MediaListData
    {
        [SerializationPropertyName("list")]
        public DataList List { get; set; }
    }

    public class DataList
    {
        [SerializationPropertyName("lists")]
        public List<ListElement> Lists { get; set; }

    }

    public class ListElement
    {
        [SerializationPropertyName("status")]
        public EntryStatus? Status { get; set; }

        [SerializationPropertyName("entries")]
        public List<Entry> Entries { get; set; }
    }

    public class Entry
    {
        [SerializationPropertyName("id")]
        public int Id { get; set; }

        [SerializationPropertyName("progress")]
        public int Progress { get; set; }

        [SerializationPropertyName("score")]
        public int Score { get; set; }

        [SerializationPropertyName("status")]
        public EntryStatus? Status { get; set; }

        [SerializationPropertyName("updatedAt")]
        public int UpdatedAt { get; set; }

        [SerializationPropertyName("media")]
        public Media Media { get; set; }
    }

    public partial class Media
    {
        [SerializationPropertyName("id")]
        public int Id { get; set; }

        [SerializationPropertyName("idMal")]
        public int? IdMal { get; set; }

        [SerializationPropertyName("siteUrl")]
        public Uri SiteUrl { get; set; }

        [SerializationPropertyName("type")]
        public TypeEnum Type { get; set; }

        [SerializationPropertyName("format")]
        public Format? Format { get; set; }

        [SerializationPropertyName("episodes")]
        public int? Episodes { get; set; }

        [SerializationPropertyName("chapters")]
        public int? Chapters { get; set; }

        [SerializationPropertyName("averageScore")]
        public int? AverageScore { get; set; }

        [SerializationPropertyName("title")]
        public Title Title { get; set; }

        [SerializationPropertyName("startDate")]
        public StartDate StartDate { get; set; }

        [SerializationPropertyName("description")]
        public string Description { get; set; }

        [SerializationPropertyName("genres")]
        public List<string> Genres { get; set; }

        [SerializationPropertyName("tags")]
        public List<Tag> Tags { get; set; }

        [SerializationPropertyName("season")]
        public Season? Season { get; set; }

        [SerializationPropertyName("status")]
        public MediaStatus Status { get; set; }

        [SerializationPropertyName("studios")]
        public Studios Studios { get; set; }

        [SerializationPropertyName("staff")]
        public Staff Staff { get; set; }

        [SerializationPropertyName("coverImage")]
        public CoverImage CoverImage { get; set; }

        [SerializationPropertyName("bannerImage")]
        public string BannerImage { get; set; }

    }

    public partial class CoverImage
    {
        [SerializationPropertyName("extraLarge")]
        public string ExtraLarge { get; set; }
    }

    public partial class Staff
    {
        [SerializationPropertyName("nodes")]
        public StaffNode[] Nodes { get; set; }
    }

    public partial class StaffNode
    {
        [SerializationPropertyName("name")]
        public StaffName Name { get; set; }
    }

    public partial class StaffName
    {
        [SerializationPropertyName("full")]
        public string Full { get; set; }
    }

    public partial class Studios
    {
        [SerializationPropertyName("nodes")]
        public StudiosNode[] Nodes { get; set; }
    }

    public partial class StudiosNode
    {
        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("isAnimationStudio")]
        public bool IsAnimationStudio { get; set; }
    }

    public partial class StartDate
    {
        [SerializationPropertyName("year")]
        public int? Year { get; set; }

        [SerializationPropertyName("month")]
        public int? Month { get; set; }

        [SerializationPropertyName("day")]
        public int? Day { get; set; }
    }

    public partial class Tag
    {
        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("isGeneralSpoiler")]
        public bool IsGeneralSpoiler { get; set; }

        [SerializationPropertyName("isMediaSpoiler")]
        public bool IsMediaSpoiler { get; set; }
    }

    public partial class Title
    {
        [SerializationPropertyName("romaji")]
        public string Romaji { get; set; }

        [SerializationPropertyName("english")]
        public string English { get; set; }

        [SerializationPropertyName("native")]
        public string Native { get; set; }
    }

    public enum Format { Tv, Tv_Short, Movie, Special, Ova, Ona, Music, Manga, Novel, One_Shot };

    public enum Season { Fall, Spring, Summer, Winter };

    public enum MediaStatus { Finished, Releasing, Not_Yet_Released, Cancelled, Hiatus };

    public enum TypeEnum { Anime, Manga };

    public enum EntryStatus { Current, Planning, Completed, Dropped, Paused, Repeating };
}