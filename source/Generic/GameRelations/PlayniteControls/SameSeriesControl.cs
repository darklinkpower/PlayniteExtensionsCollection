using GameRelations.Interfaces;
using GameRelations.Models;
using Playnite.SDK;
using System.Collections.Generic;

namespace GameRelations.PlayniteControls
{
    public partial class SameSeriesControl : GameRelationsBase
    {
        public SameSeriesControl(IPlayniteAPI playniteApi, GameRelationsSettings settings, IGameRelationsControlSettings controlSettings)
            : base(playniteApi, settings, controlSettings)
        {

        }

        internal override IEnumerable<GameRelationSnapshot> GetMatchingGames(GameRelationSnapshot game, List<GameRelationSnapshot> libraryGames, object matchingSettings)
        {
            return GetMatchingGamesByRelation(game, libraryGames, x => x.Series);
        }

    }
}
