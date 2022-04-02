using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayState.Models
{
    public class PlayStateData : ObservableObject
    {
        private Game game;
        public Game Game { get => game; set => SetValue(ref game, value); }
        public DateTime StartDate { get; set; }
        private Stopwatch stopwatch;
        public Stopwatch Stopwatch { get => stopwatch; set => SetValue(ref stopwatch, value); }
        public List<ProcessItem> GameProcesses { get; set; }

        private bool isSuspended = true;
        public bool IsSuspended { get => isSuspended; set => SetValue(ref isSuspended, value); }

        private bool processesSuspended = true;
        public bool ProcessesSuspended { get => processesSuspended; set => SetValue(ref processesSuspended, value); }

        private bool suspendPlaytimeOnly = true;
        public bool SuspendPlaytimeOnly { get => suspendPlaytimeOnly; set => SetValue(ref suspendPlaytimeOnly, value); }

        public PlayStateData(Game game, List<ProcessItem> gameProcesses, bool suspendPlaytimeOnly)
        {
            Game = game;
            StartDate = DateTime.Now;
            Stopwatch = new Stopwatch();
            GameProcesses = gameProcesses;
            IsSuspended = false;
            ProcessesSuspended = false;
            SuspendPlaytimeOnly = suspendPlaytimeOnly;
        }
    }
}
