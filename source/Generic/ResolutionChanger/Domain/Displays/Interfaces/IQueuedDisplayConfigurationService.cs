using DisplayHelper.Domain.Common;
using DisplayHelper.Domain.Displays.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Domain.Displays.Interfaces
{
    public interface IQueuedDisplayConfigurationService
    {
        Result QueueConfiguration(
            DisplayDevice configuration);

        Result Commit();

        void Clear();
    }
}
