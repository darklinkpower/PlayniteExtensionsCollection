using DisplayHelper.Domain.Displays.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Application.Displays.DTOs
{
    public sealed class DisplaySnapshot
    {
        public DisplayTopology Topology { get; }

        public IReadOnlyList<DisplayConfiguration> Configurations { get; }

        public DisplaySnapshot(
            DisplayTopology topology,
            IReadOnlyList<DisplayConfiguration> configurations)
        {
            Topology = topology;
            Configurations = configurations;
        }
    }
}
