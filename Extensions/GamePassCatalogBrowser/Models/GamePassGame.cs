using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassCatalogBrowser.Models
{
    public class GamePassGame
    {
        public string ProductId { get; set; }
        public string DeveloperName { get; set; }
        public string PublisherName { get; set; }
        public string ProductDescription { get; set; }
        public string BackgroundImage { get; set; }
        public string IconUrl { get; set; }
        public string CoverUrl { get; set; }
        public string Icon { get; set; }
        public string Cover { get; set; }
        public string CoverLowRes { get; set; }
        public string CoverLowResUrl { get; set; }
        public string GameName { get; set; }
        public string GameId { get; set; }
        public string[] Categories { get; set; }
        public string MsStoreLaunchUri { get; set; }
    }
}