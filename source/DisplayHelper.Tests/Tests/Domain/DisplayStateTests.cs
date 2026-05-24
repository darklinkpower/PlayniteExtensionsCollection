using DisplayHelper.Domain.Displays.Entities;
using DisplayHelper.Domain.Displays.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Tests.Tests.Domain
{
    public sealed class DisplayStateTests
    {
        [Fact]
        public void State_Should_StoreValuesCorrectly()
        {
            var state = new DisplayState(
                new DisplayMode(new Resolution(2560, 1440), new RefreshRate(144)),
                new DisplayPosition(10, 20),
                DisplayOrientation.Landscape,
                DisplayScaling.Default);

            Assert.Equal(2560, state.Mode.Resolution.Width);
            Assert.Equal(144, state.Mode.RefreshRate.Value);
            Assert.Equal(10, state.Position.X);
            Assert.Equal(20, state.Position.Y);
        }
    }
}
