using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameRelations.Models
{
    public class SimilarGamesControlSettings : GameRelationsControlSettings
    {
        private HashSet<Guid> tagsToIgnore = new HashSet<Guid>();
        public HashSet<Guid> TagsToIgnore { get => tagsToIgnore; set => SetValue(ref tagsToIgnore, value); }
        private bool excludeGamesSameSeries = true;
        public bool ExcludeGamesSameSeries { get => excludeGamesSameSeries; set => SetValue(ref excludeGamesSameSeries, value); }
        public SimilarGamesControlSettings()
        {

        }

        public SimilarGamesControlSettings(bool displayGameNames, int maxItems, bool isEnabled) : base(displayGameNames, maxItems, isEnabled)
        {

        }
    }
}