using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtraMetadataLoader.Models
{
    public partial class FfprobeVideoInfoOutput
    {
        [JsonProperty("programs", NullValueHandling = NullValueHandling.Ignore)]
        public Program[] Programs { get; set; }

        [JsonProperty("streams", NullValueHandling = NullValueHandling.Include)]
        public VideoStream[] Streams { get; set; }
    }

    public class Program
    {
    }

    public class VideoStream
    {
        [JsonProperty("width", NullValueHandling = NullValueHandling.Include)]
        public long Width { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Include)]
        public long Height { get; set; }

        [JsonProperty("pix_fmt", NullValueHandling = NullValueHandling.Include)]
        public string PixFmt { get; set; }

        [JsonProperty("duration", NullValueHandling = NullValueHandling.Include)]
        public string Duration { get; set; }
    }

    public enum VideoActionNeeded : int
    {
        Nothing = 0,
        Conversion = 1,
        Invalid = 2
    }
}
