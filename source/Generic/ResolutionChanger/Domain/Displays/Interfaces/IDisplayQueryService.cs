using DisplayHelper.Domain.Displays.Entities;
using DisplayHelper.Domain.Displays.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Domain.Displays.Interfaces
{
    public interface IDisplayQueryService
    {
        IReadOnlyList<DisplayDevice> GetDisplays();

        DisplayDevice GetPrimaryDisplay();

        DisplayDevice GetDisplayByAdapterId(string displayId);

        IReadOnlyList<DisplayMode> GetSupportedModes(string displayId);
    }
}
