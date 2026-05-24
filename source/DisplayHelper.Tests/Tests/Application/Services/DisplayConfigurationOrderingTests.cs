using DisplayHelper.Domain.Displays.Entities;
using DisplayHelper.Domain.Displays.ValueObjects;
using DisplayHelper.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Tests.Tests.Application.Services
{
    public sealed class DisplayConfigurationOrderingTests
    {
        [Fact]
        public void Primary_Should_AlwaysBeLast()
        {
            var list = new List<DisplayConfiguration>
        {
            new("A", CreateState(), false),
            new("B", CreateState(), true),
            new("C", CreateState(), false)
        };

            var ordered = DisplayTransactionTestHelper.Order(list);

            Assert.Equal("B", ordered[^1].DisplayId);
        }

        private static DisplayState CreateState()
            => new(
                new DisplayMode(new Resolution(1920, 1080), new RefreshRate(60)),
                new DisplayPosition(0, 0),
                DisplayOrientation.Landscape,
                DisplayScaling.Default);
    }
}
