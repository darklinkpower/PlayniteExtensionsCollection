using DisplayHelper.Domain.Displays.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Application.Displays.Services
{
    public sealed class DisplayTopologyService
    {
        public DisplayTopology CurrentTopology { get; private set; }

        public void Set(DisplayTopology topology)
        {
            CurrentTopology = topology;
        }
    }
}
