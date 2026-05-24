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
    public sealed class FakeThrowingConfigurationService : IDisplayConfigurationService
    {
        public Result ApplyConfiguration(
            ApplyDisplayConfigurationRequest configuration,
            DisplayApplyMode immediate)
        {
            throw new Exception("Simulated failure in ApplyConfiguration");
        }

        public Result SetPrimaryDisplay(string displayId)
        {
            throw new Exception("Simulated failure in SetPrimaryDisplay");
        }
    }
}
