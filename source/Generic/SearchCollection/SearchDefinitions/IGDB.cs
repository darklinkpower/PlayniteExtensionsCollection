using SearchCollection.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchCollection.SearchDefinitions
{
    public class IGDB : BaseSearchDefinition
    {
        public override string Name => "IGDB";
        public override string Icon => "IGDB.png";

        protected override string UrlFormat => @"https://www.igdb.com/search?utf8=✓&type=1&q={0}";
    }
}