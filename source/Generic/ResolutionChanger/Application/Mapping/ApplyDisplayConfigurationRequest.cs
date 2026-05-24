using DisplayHelper.Application.Displays.DTOs;
using DisplayHelper.Domain.Displays.Entities;
using DisplayHelper.Domain.Displays.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WinApi.Structs;

namespace DisplayHelper.Application.Mapping
{
    internal static class ApplyDisplayConfigurationRequestMapper
    {
        public static ApplyDisplayConfigurationRequest Map(
            DisplayConfiguration configuration)
        {
            return new ApplyDisplayConfigurationRequest(
                        configuration.DisplayId,
                        configuration.SetAsPrimary,
                        configuration.State.Mode.Resolution,
                        configuration.State.Mode.RefreshRate);
        }
    }
}
