using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThrottlerSharp
{
    public enum RateLimitMode
    {
        WaitForSlot,
        Abort
    }
}
