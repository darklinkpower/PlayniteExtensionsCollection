using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.Models
{
    public enum SpecialKExecutionMode
    {
        [Description("Global")]
        Global,
        [Description("Selective")]
        Selective
    }

    public enum SteamOverlay
    {
        [Description("Desktop")]
        Desktop,
        [Description("Big Picture Mode")]
        BigPictureMode
    }
}