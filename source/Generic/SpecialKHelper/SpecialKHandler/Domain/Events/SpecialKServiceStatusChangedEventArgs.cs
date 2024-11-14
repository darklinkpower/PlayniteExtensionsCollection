using SpecialKHelper.SpecialKHandler.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.SpecialKHandler.Domain.Events
{
    public class SpecialKServiceStatusChangedEventArgs : EventArgs
    {
        public SpecialKServiceStatus Status { get; }
        public CpuArchitecture Architecture { get; }

        public SpecialKServiceStatusChangedEventArgs(SpecialKServiceStatus status, CpuArchitecture architecture)
        {
            Status = status;
            Architecture = architecture;
        }
    }
}