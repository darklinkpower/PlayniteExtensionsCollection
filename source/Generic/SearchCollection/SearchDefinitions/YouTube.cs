using SearchCollection.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchCollection.SearchDefinitions
{
    public class YouTube : BaseSearchDefinition
    {
        public override string Name => "YouTube";
        public override string Icon => "Youtube.png";

        protected override string UrlFormat => @"https://www.youtube.com/results?search_query={0}";
    }
}