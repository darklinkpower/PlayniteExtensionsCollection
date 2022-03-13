using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayState.Models
{
    public class PlayStateData
    {
        public Game Game { get; set; }
        public DateTime StartDate { get; set; }
        public Stopwatch Stopwatch { get; set; }
        public List<ProcessItem> GameProcesses { get; set; }
        public bool IsSuspended { get; set; }
        public bool ProcessesSuspended { get; set; }

        public PlayStateData(Game game, List<ProcessItem> gameProcesses)
        {
            Game = game;
            StartDate = DateTime.Now;
            Stopwatch = new Stopwatch();
            GameProcesses = gameProcesses;
            IsSuspended = false;
            ProcessesSuspended = false;
        }
    }
}
