using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.EasyAnticheat.Domain
{
    public class GameDataCache
    {
        public Guid Id { get; set; }
        public EasyAnticheatStatus EasyAnticheatStatus { get; set; } = EasyAnticheatStatus.Unknown;
        public string SteamId { get; set; } = string.Empty;
    }
}