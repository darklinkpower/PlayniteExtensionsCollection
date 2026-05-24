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

namespace DisplayHelper.Tests.Tests.Application.Services
{
    public sealed class DisplayTransactionServiceTests
    {
        [Fact]
        public void Apply_Should_OrderPrimaryLast()
        {
            var snapshotService = new FakeSnapshotService();
            var configService = new FakeDisplayConfigurationService();
            var topologyService = new FakeTopologyService();
            var fakeCommitService = new FakeDisplayTransactionCommitService();

            var service = new DisplayTransactionService(
                snapshotService,
                configService,
                topologyService,
                fakeCommitService);

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
        public void Apply_Should_ReturnFailedRecovered_WhenRollbackSucceeds()
        {
            var snapshotService = new FakeSnapshotService();

            var configService = new FakeDisplayConfigurationService
            {
                FailTransactional = true,
                FailImmediate = false
            };

            var topologyService = new FakeTopologyService();
            var fakeCommitService = new FakeDisplayTransactionCommitService();

            var service = new DisplayTransactionService(
                snapshotService,
                configService,
                topologyService,
                fakeCommitService);

            var result = service.Apply(new List<ApplyDisplayConfigurationRequest>
            {
                new("A", false)
            });

            Assert.Equal(TransactionState.FailedRecovered, result.State);
            Assert.NotEmpty(configService.AppliedImmediate);
        }

        [Fact]
        public void Apply_Should_Rollback_OnException()
        {
            var snapshotService = new FakeSnapshotService();
            var configService = new FakeThrowingConfigurationService();
            var topologyService = new FakeTopologyService();
            var fakeCommitService = new FakeDisplayTransactionCommitService();

            var service = new DisplayTransactionService(
                snapshotService,
                configService,
                topologyService,
                fakeCommitService);

            var result = service.Apply(new List<ApplyDisplayConfigurationRequest>
            {
                new("A", false)
            });

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
