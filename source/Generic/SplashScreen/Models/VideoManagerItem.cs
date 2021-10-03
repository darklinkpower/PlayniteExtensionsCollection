using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplashScreen.Models
{
    class VideoManagerItem
    {
        public string Name { get; set; }
        public string VideoPath { get; set; }
        public SourceCollection SourceCollection { get; set; }
    }
    public enum SourceCollection { Game, Source, Platform, PlayniteMode };
}
