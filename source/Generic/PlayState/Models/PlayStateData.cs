using Playnite.SDK.Models;
using PlayState.Enums;
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
        private bool hasProcesses = true;
        public bool HasProcesses { get => hasProcesses; set => SetValue(ref hasProcesses, value); }

        private bool isSuspended = true;
        public bool IsSuspended { get => isSuspended; set => SetValue(ref isSuspended, value); }

        private SuspendModes suspendMode;
        public SuspendModes SuspendMode { get => suspendMode; set => SetValue(ref suspendMode, value); }

        public PlayStateData(Game game, List<ProcessItem> gameProcesses, SuspendModes suspendMode)
        {
            Game = game;
            StartDate = DateTime.Now;
            Stopwatch = new Stopwatch();
            GameProcesses = gameProcesses;
            IsSuspended = false;
            HasProcesses = gameProcesses.HasItems();
            SuspendMode = suspendMode;
        }
    }
}
