using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.EasyAnticheat.Domain
{
    public enum EasyAnticheatStatus : int
    {
        Detected = 0,
        NotDetected = 1,
        ErrorOnDetection = 2,
        Unknown = 3
    }
}
