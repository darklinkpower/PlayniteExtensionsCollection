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

namespace DisplayHelper.Application.Displays.Services
{
    public sealed class DisplayManager
    {
        private readonly IDisplayConfigurationService _configurationService;

        public DisplayManager(IDisplayConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        public Result Apply(ApplyDisplayConfigurationRequest request)
        {
            //var mode = new DisplayMode(
            //    new Resolution(request.Width, request.Height),
            //    new RefreshRate(request.RefreshRate));

            //var configuration = new ApplyDisplayConfigurationRequest(
            //    request.DisplayId,
            //    mode,
            //    request.SetAsPrimary);

            if (request.SetAsPrimary == true)
            {
                var primaryResult = _configurationService.SetPrimaryDisplay(request.DisplayId);
                if (!primaryResult.Success)
                {
                    return primaryResult;
                }
            }

            return _configurationService.ApplyConfiguration(request, DisplayApplyMode.Immediate);
        }
    }
}
