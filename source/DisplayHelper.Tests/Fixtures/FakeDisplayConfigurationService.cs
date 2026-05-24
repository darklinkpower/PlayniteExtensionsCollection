
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
    public sealed class FakeDisplayConfigurationService : IDisplayConfigurationService
    {
        public bool FailTransactional { get; set; }
        public bool FailImmediate { get; set; }

        public List<DisplayConfiguration> AppliedOrder { get; } = [];

        public List<string> AppliedTransactional { get; } = [];
        public List<string> AppliedImmediate { get; } = [];

        public Result ApplyConfiguration(
            ApplyDisplayConfigurationRequest configuration,
            DisplayApplyMode mode)
        {
            if (mode == DisplayApplyMode.Transactional)
            {
                if (FailTransactional)
                {
                    return Result.Fail("transactional failure");
                }

                AppliedTransactional.Add(configuration.DisplayId);
                AppliedOrder.Add(
                    new DisplayConfiguration(
                    configuration.DisplayId,
                    null,
                    configuration.SetAsPrimary));
                return Result.Ok();
            }

            // Rollback path
            if (FailImmediate)
            {
                return Result.Fail("rollback failure");
            }

            AppliedImmediate.Add(configuration.DisplayId);
            AppliedOrder.Add(
                new DisplayConfiguration(
                configuration.DisplayId,
                null,
                configuration.SetAsPrimary));
            
            return Result.Ok();
            }

        public Result SetPrimaryDisplay(string displayId)
        {
            AppliedImmediate.Add($"PRIMARY:{displayId}");
            return Result.Ok();
        }
    }

}
