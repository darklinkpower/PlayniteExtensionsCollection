using SpecialKHelper.EasyAnticheat.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.EasyAnticheat.Persistence
{
    public interface IEasyAnticheatCache
    {
        GameDataCache LoadCache(Guid gameId);
        void SaveCache(GameDataCache cache);
    }
}