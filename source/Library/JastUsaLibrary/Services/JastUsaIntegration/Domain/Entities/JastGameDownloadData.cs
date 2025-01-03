using JastUsaLibrary.Services.JastUsaIntegration.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Services.JastUsaIntegration.Domain.Entities
{
    public class JastGameDownloadData
    {
        public int GameId { get; set; }
        public int GameLinkId { get; set; }
        public string Label { get; set; }
        public List<JastPlatform> JastPlatforms { get; set; }
        public string Version { get; set; }
        public JastDownloadType JastDownloadType { get; set; }

        public JastGameDownloadData(
            int gameId,
            int gameLinkId,
            string label,
            List<JastPlatform> jastPlatforms,
            string version,
            JastDownloadType jastDownloadType)
        {
            GameId = gameId;
            GameLinkId = gameLinkId;
            Label = label;
            JastPlatforms = jastPlatforms;
            Version = version;
            JastDownloadType = jastDownloadType;
        }
    }
}
