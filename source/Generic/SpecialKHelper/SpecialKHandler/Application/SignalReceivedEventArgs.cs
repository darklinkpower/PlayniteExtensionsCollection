using SpecialKHelper.SpecialKHandler.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.SpecialKHandler.Application
{
    public class SignalReceivedEventArgs : EventArgs
    {
        public SignalType SignalType { get; }

        public SignalReceivedEventArgs(SignalType signalType)
        {
            SignalType = signalType;
        }
    }
}
