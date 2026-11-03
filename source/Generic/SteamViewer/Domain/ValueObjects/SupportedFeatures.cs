using SteamCommon.Models;

namespace SteamShortcuts.Domain.ValueObjects
{
    public class SupportedFeatures
    {
        public bool Achievements { get; set; } = false;
        public bool PointsShop { get; set; } = false;
        public bool Workshop { get; set;  } = false;

        public static SupportedFeatures All = new SupportedFeatures
        {
            Achievements = true,
            PointsShop = true,
            Workshop = true,
        };
    }
}