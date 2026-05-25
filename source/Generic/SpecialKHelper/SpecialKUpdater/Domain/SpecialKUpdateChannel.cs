using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.SpecialKUpdater.Domain
{
    public enum SpecialKUpdateChannel : int
    {
        [Description(LOC.UpdateChannelWebsiteDescription)]
        Website = 0,
        [Description(LOC.UpdateChannelDiscordDescription)]
        Discord = 1,
        [Description(LOC.UpdateChannelAncientDescription)]
        Ancient = 2
    }
}