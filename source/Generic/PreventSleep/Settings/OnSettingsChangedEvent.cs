using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreventSleep.Settings
{
    public class OnSettingsChangedEvent
    {
        public PreventSleepSettings Settings { get; }

        public OnSettingsChangedEvent(PreventSleepSettings settings)
        {
            Settings = settings;
        }
    }
}
