using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.EasyAnticheat.Application
{
    public interface IEasyAnticheatService
    {
        bool IsGameEacEnabled(Game game);
    }
}