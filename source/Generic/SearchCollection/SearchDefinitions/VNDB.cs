using SearchCollection.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchCollection.SearchDefinitions
{
    public class VNDB : BaseSearchDefinition
    {
        public override string Name => "VNDB";
        public override string Icon => "Vndb.png";

        protected override string UrlFormat => @"https://vndb.org/v/all?q={0}";
    }
}