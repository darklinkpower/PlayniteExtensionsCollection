using DisplayHelper.Domain.Common;
using DisplayHelper.Domain.Displays.Entities;
using DisplayHelper.Domain.Displays.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Tests.Fixtures
{
    public sealed class FakeTopologyService : IDisplayTopologyService
    {
        public bool ShouldFail { get; set; }

        public DisplayTopology GetCurrentTopology()
        {
            throw new NotImplementedException();
        }

        public Result SetTopology(DisplayTopology topology)
        {
            return ShouldFail? Result.Fail("Topology failed") : Result.Ok();
        }
    }
}
