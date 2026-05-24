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
    public sealed class Win32DisplayConfigurationServiceFlagsTests
    {
        [Fact]
        public void TransactionalMode_Should_IncludeNORESET()
        {
            var fake = new FakeUser32Interop();
            var service = new Win32DisplayConfigurationService(fake);

            service.ApplyConfiguration(
                new ApplyDisplayConfigurationRequest(
                    "DISPLAY1",
                    false,
                    new Resolution(1920, 1080),
                    null),
                DisplayApplyMode.Transactional);

            Assert.Contains(fake.Calls, c => c.Contains("NORESET"));
        }

        [Fact]
        public void Primary_Should_IncludeSetPrimaryFlag()
        {
            var fake = new FakeUser32Interop();
            var service = new Win32DisplayConfigurationService(fake);

            var request = new ApplyDisplayConfigurationRequest(
                "DISPLAY1",
                false,
                new Resolution(1920, 1080));

            service.ApplyConfiguration(request, DisplayApplyMode.Immediate);

            Assert.Contains(fake.Calls, c => c.Contains("SET_PRIMARY"));
        }
    }
}
