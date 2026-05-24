using DisplayHelper.Application.Displays.DTOs;
using DisplayHelper.Application.Displays.Services;
using DisplayHelper.Domain.Displays.Entities;
using DisplayHelper.Domain.Displays.Interfaces;
using DisplayHelper.Domain.Displays.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Tests.IntegrationTests.Win32.Services.Fixtures
{
    public sealed class DisplayTestEnvironment : IDisposable
    {
        private readonly IDisplayTransactionService _transactionService;

        private readonly DisplaySnapshot _snapshot;

        private bool _disposed;

        public DisplayTestEnvironment(
            IDisplaySnapshotService snapshotService,
            IDisplayTransactionService transactionService)
        {
            _transactionService = transactionService;
            _snapshot =
                snapshotService.Capture();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            try
            {
                var rollbackConfigurations =
                    _snapshot.Configurations
                        .Select(x =>
                            new ApplyDisplayConfigurationRequest(
                                x.DisplayId,
                                x.SetAsPrimary,
                                new Resolution(x.State.Mode.Resolution.Width, x.State.Mode.Resolution.Height),
                                new RefreshRate(x.State.Mode.RefreshRate.Value),
                                new DisplayPosition(x.State.Position.X, x.State.Position.Y)))
                        .ToList();

                _transactionService.Apply(
                    rollbackConfigurations);
            }
            catch
            {

            }
        }
    }
}
