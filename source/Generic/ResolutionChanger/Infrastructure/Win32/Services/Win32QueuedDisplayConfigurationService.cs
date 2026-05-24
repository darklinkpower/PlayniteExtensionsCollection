using DisplayHelper.Domain.Common;
using DisplayHelper.Domain.Displays.Entities;
using DisplayHelper.Domain.Displays.Interfaces;
using DisplayHelper.Infrastructure.Win32.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WinApi.Enums;
using static WinApi.Flags;

namespace DisplayHelper.Infrastructure.Win32.Services
{
    public sealed class Win32QueuedDisplayConfigurationService
        : IQueuedDisplayConfigurationService
    {
        private readonly List<DisplayDevice> _queuedConfigurations =
            new List<DisplayDevice>();

        public Result QueueConfiguration(
            DisplayDevice configuration)
        {
            _queuedConfigurations.Add(configuration);

            return Result.Ok();
        }

        public Result Commit()
        {
            try
            {
                foreach (var configuration in _queuedConfigurations)
                {
                    var devMode = DevModeFactory.Create();

                    devMode.dmPelsWidth =
                        configuration.CurrentState.Mode.Resolution.Width;

                    devMode.dmPelsHeight =
                        configuration.CurrentState.Mode.Resolution.Height;

                    devMode.dmDisplayFrequency =
                        configuration.CurrentState.Mode.RefreshRate.Value;

                    devMode.dmPosition.x =
                        configuration.CurrentState.Position.X;

                    devMode.dmPosition.y =
                        configuration.CurrentState.Position.Y;

                    devMode.dmFields =
                        DeviceModeFieldsFlags.DM_PELSWIDTH |
                        DeviceModeFieldsFlags.DM_PELSHEIGHT |
                        DeviceModeFieldsFlags.DM_DISPLAYFREQUENCY |
                        DeviceModeFieldsFlags.DM_POSITION;

                    var testResult =
                        User32Interop.ChangeDisplaySettingsEx(
                            configuration.AdapterId,
                            ref devMode,
                            IntPtr.Zero,
                            ChangeDisplaySettingsFlags.CDS_TEST,
                            IntPtr.Zero);

                    if (testResult != DISP_CHANGE.Successful)
                    {
                        return Result.Fail(
                            $"Display test failed: {testResult}");
                    }

                    var queueResult =
                        User32Interop.ChangeDisplaySettingsEx(
                            configuration.AdapterId,
                            ref devMode,
                            IntPtr.Zero,
                            ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY |
                            ChangeDisplaySettingsFlags.CDS_NORESET,
                            IntPtr.Zero);

                    if (queueResult != DISP_CHANGE.Successful)
                    {
                        return Result.Fail(
                            $"Queue operation failed: {queueResult}");
                    }
                }

                // Actual Commit. Previous changes are staged only.
                var commitResult =
                    User32Interop.ChangeDisplaySettingsEx(
                        null,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        ChangeDisplaySettingsFlags.CDS_NONE,
                        IntPtr.Zero);

                _queuedConfigurations.Clear();

                return commitResult == DISP_CHANGE.Successful
                    ? Result.Ok()
                    : Result.Fail(
                        $"Commit failed: {commitResult}");
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToString());
            }
        }

        public void Clear()
        {
            _queuedConfigurations.Clear();
        }
    }
}
