using JastUsaLibrary.ProgramsHelper.Models;
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
        public Guid EventId { get; }
        public DateTime CreatedAtUtc { get; }
        public Game Game { get; }
        public Program Program { get; }

        public GameInstallationAppliedEventArgs(Game game, Program program)
        {
            EventId = Guid.NewGuid();
            CreatedAtUtc = DateTime.UtcNow;
            Game = game;
            Program = program;
        }
    }
}
