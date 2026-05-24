using DisplayHelper.Application.Displays.DTOs;
using DisplayHelper.Domain.Displays.Entities;
using DisplayHelper.Domain.Displays.ValueObjects;
using DisplayHelper.Infrastructure.Win32.Services;
using DisplayHelper.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Tests.IntegrationTests.Win32.Services
{
    public sealed class Win32DisplayConfigurationServiceTests
    {
        [Fact]
        public void ApplyConfiguration_Should_MapResolutionCorrectly()
        {
            var fake = new FakeUser32Interop();
            var service = new Win32DisplayConfigurationService(fake);

            var request = new ApplyDisplayConfigurationRequest(
                "DISPLAY1",
                false,
                new Resolution(1920, 1080),
                null);

            var result = service.ApplyConfiguration(request, DisplayApplyMode.Immediate);

            Assert.True(result.Success);
            Assert.Contains(fake.Calls, c => c.Contains("1920x1080"));
        }

        [Fact]
        public void ApplyConfiguration_Should_MapRefreshRate()
        {
            var fake = new FakeUser32Interop();
            var service = new Win32DisplayConfigurationService(fake);

            var request = new ApplyDisplayConfigurationRequest(
                "DISPLAY1",
                false,
                new Resolution(1920, 1080),
                new RefreshRate(144));

            var result = service.ApplyConfiguration(request, DisplayApplyMode.Immediate);

            Assert.True(result.Success);
            Assert.Contains(fake.Calls, c => c.Contains("144"));
        }
    }
}
