using DisplayHelper.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Application.Displays.Services
{
    public interface IDisplayTransactionCommitService
    {
        Result Commit();
    }
}
