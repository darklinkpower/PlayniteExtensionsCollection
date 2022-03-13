using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeCommon.Models
{
    public class YoutubeEmbeddedResponse
    {

        [JsonProperty("contents")]
        public Contents Contents { get; set; }

        [JsonProperty("refinements")]
        public string[] Refinements { get; set; }
    }

    public class Contents
    {
        [JsonProperty("twoColumnSearchResultsRenderer")]
        public TwoColumnSearchResultsRenderer TwoColumnSearchResultsRenderer { get; set; }
    }

    public class TwoColumnSearchResultsRenderer
    {
        [JsonProperty("primaryContents")]
        public PrimaryContents PrimaryContents { get; set; }
    }

    public class PrimaryContents
    {
        [JsonProperty("sectionListRenderer")]
        public SectionListRenderer SectionListRenderer { get; set; }
    }

    public class SectionListRenderer
    {
        [JsonProperty("contents")]
        public SectionListRendererContent[] Contents { get; set; }
    }

    public class SectionListRendererContent
    {
        [JsonProperty("itemSectionRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public ItemSectionRenderer ItemSectionRenderer { get; set; }
    }

    public class ItemSectionRenderer
    {
        [JsonProperty("contents")]
        public List<ItemSectionRendererContent> Contents { get; set; }
    }

    public class ItemSectionRendererContent
    {
        [JsonProperty("videoRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public VideoRenderer VideoRenderer { get; set; }
    }

    public class VideoRenderer
    {
        [JsonProperty("videoId")]
        public string VideoId { get; set; }

        [JsonProperty("thumbnail")]
        public SearchRefinementCardRendererThumbnail Thumbnail { get; set; }

        [JsonProperty("title")]
        public CollapsedStateButtonTextClass Title { get; set; }

        [JsonProperty("longBylineText")]
        public LongBylineText LongBylineText { get; set; }

        [JsonProperty("publishedTimeText")]
        public SubtitleClass PublishedTimeText { get; set; }

        [JsonProperty("lengthText")]
        public LengthText LengthText { get; set; }

        [JsonProperty("viewCountText")]
        public SubtitleClass ViewCountText { get; set; }

        [JsonProperty("badges", NullValueHandling = NullValueHandling.Ignore)]
        public Badge[] Badges { get; set; }

        [JsonProperty("ownerBadges", NullValueHandling = NullValueHandling.Ignore)]
        public Badge[] OwnerBadges { get; set; }

        [JsonProperty("ownerText")]
        public LongBylineText OwnerText { get; set; }


        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }

        [JsonProperty("showActionMenu")]
        public bool ShowActionMenu { get; set; }

        [JsonProperty("shortViewCountText")]
        public LengthText ShortViewCountText { get; set; }
    }

    public class Text
    {
        [JsonProperty("runs")]
        public TextRun[] Runs { get; set; }
    }

    public class TextRun
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }
    public class ThumbnailElement
    {
        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }
    }

    public class IconImage
    {
        [JsonProperty("iconType")]
        public string IconType { get; set; }
    }


    public class Badge
    {

    }

    public class LengthText
    {

        [JsonProperty("simpleText")]
        public string SimpleText { get; set; }
    }

    public class SearchRefinementCardRendererThumbnail
    {
        [JsonProperty("thumbnails")]
        public List<ThumbnailElement> Thumbnails { get; set; }
    }

    public class CollapsedStateButtonTextClass
    {
        [JsonProperty("runs")]
        public TextRun[] Runs { get; set; }
    }

    public class LongBylineTextRun
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class LongBylineText
    {
        [JsonProperty("runs")]
        public LongBylineTextRun[] Runs { get; set; }
    }

    public class SubtitleClass
    {
        [JsonProperty("simpleText")]
        public string SimpleText { get; set; }
    }
}