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
    public sealed class Win32DisplayConfigurationServiceSequenceTests
    {
        [Fact]
        public void Should_CallEnumDisplaySettingsBeforeApply()
        {
            var fake = new FakeUser32Interop();
            var service = new Win32DisplayConfigurationService(fake);

            service.ApplyConfiguration(
                new ApplyDisplayConfigurationRequest(
                    "DISPLAY1",
                    false,
                    new Resolution(1920, 1080),
                    new RefreshRate(60)),
                DisplayApplyMode.Immediate);

            Assert.True(fake.Calls.IndexOf("EnumDisplaySettings") < fake.Calls.IndexOf("ChangeDisplaySettingsEx"));
        }
    }
}
