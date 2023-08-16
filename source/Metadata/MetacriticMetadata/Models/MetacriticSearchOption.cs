using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetacriticMetadata.Models
{
    public class MetacriticSearchOption : GenericItemOption
    {
        public MetacriticSearchResult MetacriticSearchResult { get; set; }

        public MetacriticSearchOption(string name, string description, MetacriticSearchResult metacriticSearchResult)
        {
            Name = name;
            Description = description;
            MetacriticSearchResult = metacriticSearchResult;
        }
    }
}