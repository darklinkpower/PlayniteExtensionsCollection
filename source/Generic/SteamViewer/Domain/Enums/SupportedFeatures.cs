using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamShortcuts.Domain.Enums
{
    [Flags]
    public enum SupportedFeatures
    {
        None = 0,
        Achievements = 1,
        PointsShop = 2,
        Workshop = 4,

        All = Achievements | PointsShop | Workshop
    }
}
