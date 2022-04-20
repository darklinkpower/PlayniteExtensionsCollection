using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ResolutionChanger.DisplayUtilities;

namespace ResolutionChanger.Models
{
    public class DisplayConfigChangeData
    {
        public DEVMODE DevMode { get; }
        public bool ResolutionChanged { get; }
        public bool RefreshRateChanged { get; }

        public DisplayConfigChangeData(DEVMODE devMode, bool resolutionChanged, bool refreshRateChanged)
        {
            DevMode = devMode;
            ResolutionChanged = resolutionChanged;
            RefreshRateChanged = refreshRateChanged;
        }
    }
}