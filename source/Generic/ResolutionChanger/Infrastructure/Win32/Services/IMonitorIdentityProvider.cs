using DisplayHelper.Domain.Displays.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Infrastructure.Win32.Services
{
    public interface IMonitorIdentityProvider
    {
        MonitorIdentity Get(string monitorDeviceId);
    }
}
