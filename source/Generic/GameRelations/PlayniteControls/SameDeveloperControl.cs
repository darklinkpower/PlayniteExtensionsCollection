using GameRelations.Interfaces;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TemporaryCache;

namespace GameRelations.PlayniteControls
{
    public partial class SameDeveloperControl : GameRelationsBase
    {
        public SameDeveloperControl(CacheManager<string, BitmapImage> _imagesCacheManager, IPlayniteAPI playniteApi, GameRelationsSettings settings, IGameRelationsControlSettings controlSettings)
            : base(_imagesCacheManager, playniteApi, settings, controlSettings)
        {

        }

        public override IEnumerable<Game> GetMatchingGames(Game game)
        {
            if (!game.Developers.HasItems())
            {
                return Enumerable.Empty<Game>();
            }
            
            var sourceHashSet = game.Developers.ToHashSet();
            var similarGamesDict = new Dictionary<Game, string>();
            foreach (var otherGame in PlayniteApi.Database.Games)
            {
                if (otherGame.Id == game.Id)
                {
                    continue;
                }

                if (!game.Hidden && otherGame.Hidden)
                {
                    continue;
                }

                var commonItem = GetAnyCommonItem(otherGame.Developers, sourceHashSet);
                if (commonItem != default(Company))
                {
                    similarGamesDict.Add(otherGame, commonItem.Name);
                }
            }

            var similarGames = similarGamesDict
                .OrderBy(pair => pair.Value)
                .ThenBy(x => !x.Key.SortingName.IsNullOrEmpty() ? x.Key.SortingName : x.Key.Name)
                .Select(x => x.Key);

            return similarGames;
        }
    }
}