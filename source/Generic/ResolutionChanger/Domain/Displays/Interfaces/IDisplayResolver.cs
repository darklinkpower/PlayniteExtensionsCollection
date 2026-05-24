using DisplayHelper.Domain.Displays.Entities;
using DisplayHelper.Domain.Displays.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Domain.Displays.Interfaces
{
    public interface IDisplayResolver
    {
        DisplayDevice Resolve(MonitorIdentity identity);
    }
}
