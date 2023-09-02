using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Models
{
    public class ModeDisplaySettings : ObservableObject
    {
        private string targetDisplayName = string.Empty;
        public string TargetDisplayName { get => targetDisplayName; set => SetValue(ref targetDisplayName, value); }

        private bool targetSpecificDisplay = false;
        public bool TargetSpecificDisplay { get => targetSpecificDisplay; set => SetValue(ref targetSpecificDisplay, value); }

        private bool changeResolution = false;
        public bool ChangeResolution { get => changeResolution; set => SetValue(ref changeResolution, value); }
        private int? width = null;
        public int? Width { get => width; set => SetValue(ref width, value); }
        private int? height = null;
        public int? Height { get => height; set => SetValue(ref height, value); }
        private bool changeRefreshRate = false;
        public bool ChangeRefreshRate { get => changeRefreshRate; set => SetValue(ref changeRefreshRate, value); }

        private int? refreshRate = null;
        public int? RefreshRate { get => refreshRate; set => SetValue(ref refreshRate, value); }
    }
}