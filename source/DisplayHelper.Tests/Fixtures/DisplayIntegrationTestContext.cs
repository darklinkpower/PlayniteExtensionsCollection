using DisplayHelper.Application.Displays.DTOs;
using DisplayHelper.Application.Displays.Services;
using DisplayHelper.Domain.Displays.Entities;
using DisplayHelper.Domain.Displays.Interfaces;
using DisplayHelper.Infrastructure.Win32.Native;
using DisplayHelper.Infrastructure.Win32.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Tests.Fixtures
{
    public sealed class DisplayIntegrationTestContext
    {
        public IDisplayQueryService QueryService { get; }

        public IDisplaySnapshotService SnapshotService { get; }

        public IDisplayTransactionService TransactionService { get; }

        public DisplayIntegrationTestContext()
        {
            var api =
                new Win32DisplayApi();

            var registryService =
                new RegistryService();

            var identityProvider =
                new Win32MonitorIdentityProvider(
                    registryService);

            QueryService =
                new Win32DisplayQueryService(
                    api,
                    identityProvider);

            SnapshotService =
                new DisplaySnapshotService(
                    QueryService);

            var topologyService =
                new Win32DisplayTopologyService();

            var configurationService =
                new Win32DisplayConfigurationService(
                    api);

            var commitService =
                new Win32DisplayTransactionCommitService(
                    api);

            TransactionService =
                new DisplayTransactionService(
                    SnapshotService,
                    configurationService,
                    topologyService,
                    commitService);
        }

        public IReadOnlyList<DisplayDevice> GetDisplays()
        {
            var displays =
                QueryService.GetDisplays();

            Assert.True(
                displays.Count >= 2,
                "This integration test requires at least two monitors.");

            return displays;
        }

        public DisplayDevice GetNonPrimaryDisplay()
        {
            return GetDisplays()
                .First(x => !x.IsPrimary);
        }

        public IReadOnlyList<ApplyDisplayConfigurationRequest>
            CreatePrimarySwitchRequests(
                DisplayDevice newPrimary)
        {
            return GetDisplays()
                .Select(display =>
                    new ApplyDisplayConfigurationRequest(
                        display.AdapterName,
                        display.Identity.Equals(newPrimary.Identity)))
                .ToList();
        }
    }
}
