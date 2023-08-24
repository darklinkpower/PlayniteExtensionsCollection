using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResolutionChanger.Models
{
    public class ModeDisplaySettings : ObservableObject
    {
        private DisplayInfo targetDisplay = null;
        public DisplayInfo TargetDisplay { get => targetDisplay; set => SetValue(ref targetDisplay, value); }

        private bool targetSpecificDisplay = false;
        public bool TargetSpecificDisplay { get => targetSpecificDisplay; set => SetValue(ref targetSpecificDisplay, value); }

        private bool changeResolution = false;
        public bool ChangeResolution { get => changeResolution; set => SetValue(ref changeResolution, value); }

        public int Width = 0;
        public int Height = 0;

        private bool changeRefreshRate = false;
        public bool ChangeRefreshRate { get => changeRefreshRate; set => SetValue(ref changeRefreshRate, value); }
        public int RefreshRate = 0;
    }
}