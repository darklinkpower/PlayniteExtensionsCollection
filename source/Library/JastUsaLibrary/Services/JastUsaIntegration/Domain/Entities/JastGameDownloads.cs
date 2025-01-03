using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Services.JastUsaIntegration.Domain.Entities
{
    public class JastGameDownloads
    {
        public int Id { get; }
        public List<JastGameDownloadData> GameDownloads { get; set; }
        public List<JastGameDownloadData> ExtraDownloads { get; set; }
        public List<JastGameDownloadData> PatchDownloads { get; set; }

        public JastGameDownloads(
            int id,
            List<JastGameDownloadData> gameDownloads,
            List<JastGameDownloadData> extraDownloads,
            List<JastGameDownloadData> patchDownloads)
        {
            Id = id;
            GameDownloads = gameDownloads ?? new List<JastGameDownloadData>();
            ExtraDownloads = extraDownloads ?? new List<JastGameDownloadData>();
            PatchDownloads = patchDownloads ?? new List<JastGameDownloadData>();
        }
    }
}
