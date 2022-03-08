using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.Models
{
    public enum SpecialKExecutionMode
    {
        [Description(LOC.SpecialKExecutionModeGlobal)]
        Global,
        [Description(LOC.SpecialKExecutionModeSelective)]
        Selective
    }

    public enum SteamOverlay
    {
        [Description(LOC.SteamOverlayDesktop)]
        Desktop,
        [Description(LOC.SteamOverlayBpm)]
        BigPictureMode
    }
}