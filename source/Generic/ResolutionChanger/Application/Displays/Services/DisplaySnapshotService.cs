using DisplayHelper.Application.Displays.DTOs;
using DisplayHelper.Domain.Common;
using DisplayHelper.Domain.Displays.Entities;
using DisplayHelper.Domain.Displays.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Application.Displays.Services
{
    public sealed class DisplaySnapshotService : IDisplaySnapshotService
    {
        private readonly IDisplayQueryService _queryService;

        public DisplaySnapshotService(
            IDisplayQueryService queryService)
        {
            _queryService = queryService;
        }

        public DisplaySnapshot Capture()
        {
            var displays =
                _queryService.GetDisplays();

            var configurations =
                displays
                    .Select(display =>
                        new DisplayConfiguration(
                            display.AdapterName,
                            display.CurrentState,
                            display.IsPrimary))
                    .ToList();

            // TODO:
            // Replace Unknown with real topology detection later.
            return new DisplaySnapshot(
                DisplayTopology.Unknown,
                configurations);
        }
    }
}
