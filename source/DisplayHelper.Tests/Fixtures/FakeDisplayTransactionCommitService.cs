using DisplayHelper.Application.Displays.Services;
using DisplayHelper.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Tests.Fixtures
{
    public sealed class FakeDisplayTransactionCommitService : IDisplayTransactionCommitService
    {
        public Result Commit()
        {
            return Result.Ok();
        }
    }
}
