using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.Core.Domain
{
    public enum SpecialKServiceStopMode
    {
        [Description(LOC.SpecialKServiceStopModeOnInjection)]
        OnInjection = 0,
        [Description(LOC.SpecialKServiceStopModeOnGameStop)]
        OnGameStop = 1,
        [Description(LOC.SpecialKServiceStopModeNever)]
        Never = 2
    }
}
