using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.DownloadManager.Domain.Events
{
    public class GameInstallationAppliedEventArgs : EventArgs
    {
        public Game Game { get; }
        public GameCache Cache { get; }

        public GameInstallationAppliedEventArgs(Game game, GameCache cache)
        {
            Game = game;
            Cache = cache;
        }
    }
}
