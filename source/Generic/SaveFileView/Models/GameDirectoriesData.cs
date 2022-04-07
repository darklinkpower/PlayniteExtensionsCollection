using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK.Data;
using PlayState.Enums;

namespace SaveFileView.Models
{
    public class GameDirectoriesData
    {
        [SerializationPropertyName("PcgwPageId")]
        public string PcgwPageId { get; set; }

        [SerializationPropertyName("PathsData")]
        public List<PathData> PathsData { get; set; }
    }

    public class PathData
    {
        [SerializationPropertyName("Path")]
        public string Path { get; set; }
        [SerializationPropertyName("PathType")]
        public PathType Type { get; set; }
    }
}