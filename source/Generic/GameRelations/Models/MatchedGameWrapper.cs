using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GameRelations.Models
{
    public class MatchedGameWrapper
    {
        public Game Game { get; private set; }
        public BitmapImage CoverImage { get; private set; }

        public MatchedGameWrapper(Game game, BitmapImage coverImage)
        {
            this.Game = game;
            this.CoverImage = coverImage;
        }
    }
}