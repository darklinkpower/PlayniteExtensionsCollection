using SearchCollection.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchCollection.SearchDefinitions
{
    public class Twitch : BaseSearchDefinition
    {
        public override string Name => "Twitch";
        public override string Icon => "Twitch.png";

        protected override string UrlFormat => @"hhttps://www.twitch.tv/search?term={0}";
    }
}