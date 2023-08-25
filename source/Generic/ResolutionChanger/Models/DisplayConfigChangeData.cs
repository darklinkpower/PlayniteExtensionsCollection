using DisplayHelper.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Models
{
    public class DisplayConfigChangeData
    {
        public readonly DEVMODE DevMode;
        public readonly string TargetDisplayName;
        public readonly string PrimaryDisplayName;
        public readonly bool RestoreResolutionValues;
        public readonly bool RestoreRefreshRate;
        public bool RestorePrimaryDisplay => TargetDisplayName != PrimaryDisplayName;

        public DisplayConfigChangeData(DEVMODE devMode, string targetDisplayName, string primaryDisplayName, bool restoreResolutionValues, bool restoreRefreshRate)
        {
            DevMode = devMode;
            TargetDisplayName = targetDisplayName;
            PrimaryDisplayName = primaryDisplayName;
            RestoreResolutionValues = restoreResolutionValues;
            RestoreRefreshRate = restoreRefreshRate;
        }
    }
}