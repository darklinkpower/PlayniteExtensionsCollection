using DisplayHelper.Application.Displays.DTOs;
using DisplayHelper.Application.Mapping;
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
    public sealed class DisplayRestoreService : IDisplaySnapshotService
    {
        private readonly IDisplayQueryService _queryService;
        private readonly IDisplayTopologyService _displayTopologyService;
        private readonly IDisplayConfigurationService _configurationService;

        public DisplayRestoreService(
            IDisplayQueryService queryService,
            IDisplayTopologyService displayTopologyService,
            IDisplayConfigurationService configurationService)
        {
            _queryService = queryService;
            _displayTopologyService = displayTopologyService;
            _configurationService = configurationService;
        }

        public DisplaySnapshot Capture()
        {
            var displays = _queryService.GetDisplays();
            var topology = _displayTopologyService.GetCurrentTopology();
            var configurations = displays
                .Select(d => new DisplayConfiguration(
                    d.AdapterId,
                    d.CurrentState,
                    d.IsPrimary))
                .ToList();

            return new DisplaySnapshot(topology, configurations);
        }

        public Result Restore(DisplaySnapshot snapshot)
        {
            foreach (var configuration in snapshot.Configurations)
            {
                var result = _configurationService.ApplyConfiguration(
                    ApplyDisplayConfigurationRequestMapper.Map(configuration),
                    DisplayApplyMode.Immediate);
                if (!result.Success)
                {
                    return result;
                }
            }

            return Result.Ok();
        }
    }
}
