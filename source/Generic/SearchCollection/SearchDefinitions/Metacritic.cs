using SearchCollection.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchCollection.SearchDefinitions
{
    public class Metacritic : BaseSearchDefinition
    {
        public override string Name => "Metacritic";
        public override string Icon => "Metacritic.png";

        protected override string UrlFormat => @"https://www.metacritic.com/search/game/{0}/results";
    }
}