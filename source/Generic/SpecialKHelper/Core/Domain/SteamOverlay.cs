using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.Core.Domain
{
    public enum SteamOverlay
    {
        [Description(LOC.SteamOverlayDesktop)]
        Desktop,
        [Description(LOC.SteamOverlayBpm)]
        BigPictureMode
    }
}
