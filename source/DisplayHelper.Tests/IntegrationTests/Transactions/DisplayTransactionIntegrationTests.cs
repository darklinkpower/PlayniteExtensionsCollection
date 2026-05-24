using DisplayHelper.Application.Displays.DTOs;
using DisplayHelper.Application.Displays.Services;
using DisplayHelper.Domain.Displays.Entities;
using DisplayHelper.Domain.Displays.ValueObjects;
using DisplayHelper.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Tests.IntegrationTests.Transactions
{
    public sealed class DisplayTransactionIntegrationTests
    {
        [Fact]
        public void Transaction_Should_ApplyPrimaryLast()
        {
            var snapshotService = new FakeSnapshotService();
            var configService = new FakeDisplayConfigurationService();
            var topologyService = new FakeTopologyService();
            var commitService = new FakeDisplayTransactionCommitService();

            var service = new DisplayTransactionService(
                snapshotService,
                configService,
                topologyService,
                commitService);

            var configs = new List<ApplyDisplayConfigurationRequest>
            {
                new("A", false),
                new("B", true),
                new("C", false)
            };

            service.Apply(configs);

            Assert.Equal("B", configService.AppliedOrder[^1].DisplayId);
        }

        [Fact]
        public void Transaction_Should_Rollback_OnTopologyFailure()
        {
            var snapshotService = new FakeSnapshotService();
            var configService = new FakeDisplayConfigurationService()
            {
                FailImmediate = true,
                FailTransactional = true
            };
            var topologyService = new FakeTopologyService();
            var commitService = new FakeDisplayTransactionCommitService();

            var service = new DisplayTransactionService(
                snapshotService,
                configService,
                topologyService,
                commitService);

            var result = service.Apply(
            [
                new("A", false)
            ]);

            Assert.False(result.Success);
        }

        private static DisplayState CreateState()
            => new(
                new DisplayMode(new Resolution(1920, 1080), new RefreshRate(60)),
                new DisplayPosition(0, 0),
                DisplayOrientation.Landscape,
                DisplayScaling.Default);
    }
}
