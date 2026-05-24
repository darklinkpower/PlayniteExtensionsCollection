using DisplayHelper.Application.Displays.DTOs;
using DisplayHelper.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Domain.Displays.Interfaces
{
    public interface IDisplaySnapshotService
    {
        DisplaySnapshot Capture();
    }
}
