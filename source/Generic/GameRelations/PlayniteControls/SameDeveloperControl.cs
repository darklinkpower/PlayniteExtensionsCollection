using GameRelations.Interfaces;
using GameRelations.Models;
using Playnite.SDK;
using System.Collections.Generic;

namespace GameRelations.PlayniteControls
{
    public partial class SameDeveloperControl : GameRelationsBase
    {
        public SameDeveloperControl(IPlayniteAPI playniteApi, GameRelationsSettings settings, IGameRelationsControlSettings controlSettings)
            : base(playniteApi, settings, controlSettings)
        {

        }

        internal override IEnumerable<GameRelationSnapshot> GetMatchingGames(GameRelationSnapshot game, List<GameRelationSnapshot> libraryGames, object matchingSettings)
        {
            return GetMatchingGamesByRelation(game, libraryGames, x => x.Developers);
        }
    }
}
