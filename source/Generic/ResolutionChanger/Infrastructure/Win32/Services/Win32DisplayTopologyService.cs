using DisplayHelper.Domain.Common;
using DisplayHelper.Domain.Displays.Entities;
using DisplayHelper.Domain.Displays.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Infrastructure.Win32.Services
{
    public sealed class Win32DisplayTopologyService : IDisplayTopologyService
    {
        public DisplayTopology GetCurrentTopology()
        {
            return DisplayTopology.Unknown;
        }

        public Result SetTopology(DisplayTopology topology)
        {
            throw new NotImplementedException();
        }
    }
}
