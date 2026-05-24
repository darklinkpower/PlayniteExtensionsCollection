using DisplayHelper.Application.Displays.Services;
using DisplayHelper.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WinApi.Enums;
using static WinApi.Flags;

namespace DisplayHelper.Infrastructure.Win32.Services
{
    public sealed class Win32DisplayTransactionCommitService
        : IDisplayTransactionCommitService
    {
        private readonly IWin32DisplayApi _api;

        public Win32DisplayTransactionCommitService(
            IWin32DisplayApi api)
        {
            _api = api;
        }

        public Result Commit()
        {
            var result =
                _api.ChangeDisplaySettingsEx(
                    null,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    ChangeDisplaySettingsFlags.CDS_NONE,
                    IntPtr.Zero);

            return result == DISP_CHANGE.Successful
                ? Result.Ok()
                : Result.Fail(
                    $"Failed committing display transaction: {result}");
        }
    }
}
