using GamesSizeCalculator.Models;

namespace GamesSizeCalculator.Models
{
    public class DepotGroupingInfo
    {
        public string BaseName { get; set; }
        public string RegionWord { get; set; }
        public int Rank { get; set; }
        public DepotInfo Depot { get; set; }
    }
}