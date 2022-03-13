using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeCommon.Models
{
    public class YoutubeSearchItem
    {
        public Uri ThumbnailUrl { get; set; }
        public string VideoTitle { get; set; }
        public string VideoId { get; set; }
        public string VideoLenght { get; set; }
        public string ChannelName { get; set; }
    }
}
