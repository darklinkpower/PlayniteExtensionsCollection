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
        public readonly DEVMODE DevMode;
        public readonly string DisplayDeviceName;
        public readonly bool ResolutionChanged;
        public readonly bool RefreshRateChanged;

        public DisplayConfigChangeData(DEVMODE devMode, string displayDeviceName, bool resolutionChanged, bool refreshRateChanged)
        {
            DevMode = devMode;
            DisplayDeviceName = displayDeviceName;
            ResolutionChanged = resolutionChanged;
            RefreshRateChanged = refreshRateChanged;
        }
    }
}