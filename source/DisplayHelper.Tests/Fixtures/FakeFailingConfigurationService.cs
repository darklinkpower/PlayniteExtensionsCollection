using DisplayHelper.Application.Displays.DTOs;
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
    public sealed class FakeFailingConfigurationService : IDisplayConfigurationService
    {
        public Result ApplyConfiguration(ApplyDisplayConfigurationRequest configuration, DisplayApplyMode immediate)
        {
            return Result.Fail("Simulated failure in ApplyConfiguration");
        }

        public Result SetPrimaryDisplay(string displayId)
        {
            return Result.Fail("Simulated failure in SetPrimaryDisplay");
        }
    }
}
