using DisplayHelper.Application.Displays.DTOs;
using DisplayHelper.Domain.Displays.Entities;
using DisplayHelper.Domain.Displays.ValueObjects;
using DisplayHelper.Infrastructure.Win32.Native;
using DisplayHelper.Infrastructure.Win32.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Tests.IntegrationTests.Win32.Services
{
    public sealed class RealDisplayConfigurationTests
    {
        [Fact]
        public void ApplyConfiguration_Should_ChangeResolution_OnRealMonitor()
        {
            var api = new Win32DisplayApi(); // REAL IMPLEMENTATION!!!

            var service = new Win32DisplayConfigurationService(api);

            var result = service.ApplyConfiguration(
                new ApplyDisplayConfigurationRequest(
                    @"\\.\DISPLAY1",
                    false,
                    new Resolution(1920, 1080),
                    new RefreshRate(60)),
                DisplayApplyMode.Immediate);

            Assert.True(result.Success);
        }
    }
}
