using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassCatalogBrowser.Models
{
    public class GamePassGame
    {
        public string BackgroundImage { get; set; }
        public string BackgroundImageUrl { get; set; }
        public string[] Categories { get; set; }
        public string CoverImage { get; set; }
        public string CoverImageUrl { get; set; }
        public string CoverImageLowRes { get; set; }
        public string Description { get; set; }
        public string[] Developers { get; set; }
        public string GameId { get; set; }
        public string Icon { get; set; }
        public string IconUrl { get; set; }
        public string Name { get; set; }
        public string ProductId { get; set; }
        public string[] Publishers { get; set; }
        public ProductType ProductType { get; set; }
    }

    public enum ProductType { Collection, Game, EaGame };
}