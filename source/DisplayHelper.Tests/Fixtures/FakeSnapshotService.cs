using DisplayHelper.Application.Displays.DTOs;
using DisplayHelper.Domain.Common;
using DisplayHelper.Domain.Displays.Entities;
using DisplayHelper.Domain.Displays.Interfaces;
using DisplayHelper.Domain.Displays.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Tests.Fixtures
{
    public sealed class FakeSnapshotService : IDisplaySnapshotService
    {
        public bool RollbackTriggered { get; private set; }

        public DisplaySnapshot Capture()
        {
            return new DisplaySnapshot(
                DisplayTopology.Internal,
                new List<DisplayConfiguration>
                {
                        new DisplayConfiguration(
                            "FakeDisplayId",
                            new DisplayState(
                                new DisplayMode(
                                    new Resolution(1920, 1080),
                                    new RefreshRate(60)
                                ),
                                new DisplayPosition(0, 0),
                                DisplayOrientation.Landscape,
                                DisplayScaling.Default
                            ),
                            true
                        )
                });
        }

        public Result Restore(DisplaySnapshot snapshot)
        {
            RollbackTriggered = true;
            return Result.Ok();
        }
    }
}
