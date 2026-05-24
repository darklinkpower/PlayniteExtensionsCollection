using DisplayHelper.Application.Displays.DTOs;
using DisplayHelper.Domain.Common;
using DisplayHelper.Domain.Displays.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Domain.Displays.Interfaces
{
    public interface IDisplayConfigurationService
    {
        Result ApplyConfiguration(
            ApplyDisplayConfigurationRequest configuration,
            DisplayApplyMode immediate);

        Result SetPrimaryDisplay(string displayId);
    }
}
