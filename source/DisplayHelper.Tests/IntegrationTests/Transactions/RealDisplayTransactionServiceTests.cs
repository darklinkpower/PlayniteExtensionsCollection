using DisplayHelper.Application.Displays.DTOs;
using DisplayHelper.Application.Displays.Services;
using DisplayHelper.Domain.Displays.Entities;
using DisplayHelper.Domain.Displays.Interfaces;
using DisplayHelper.Domain.Displays.ValueObjects;
using DisplayHelper.Infrastructure.Win32.Native;
using DisplayHelper.Infrastructure.Win32.Services;
using DisplayHelper.Tests.Fixtures;
using DisplayHelper.Tests.IntegrationTests.Win32.Services.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Tests.IntegrationTests.Transactions
{
    public sealed class RealDisplayTransactionServiceTests
    {
        [Fact]
        public void Apply_Should_Switch_Primary_Display()
        {
            var context =
                new DisplayIntegrationTestContext();

            using (var environment =
                new DisplayTestEnvironment(
                    context.SnapshotService,
                    context.TransactionService))
            {
                var newPrimary =
                    context.GetNonPrimaryDisplay();

                var requests =
                    context.CreatePrimarySwitchRequests(
                        newPrimary);

                var result =
                    context.TransactionService.Apply(requests);

                Assert.True(
                    result.Success,
                    result.Error);

                var updatedPrimary =
                    context.QueryService
                        .GetDisplays()
                        .Single(x => x.IsPrimary);

                Assert.Equal(
                    newPrimary.Identity,
                    updatedPrimary.Identity);
            }
        }

        [Fact]
        public void Apply_Should_Normalize_Display_Positions_When_Primary_Changes()
        {
            var context =
                new DisplayIntegrationTestContext();

            using (var environment =
                new DisplayTestEnvironment(
                    context.SnapshotService,
                    context.TransactionService))
            {
                var newPrimary =
                    context.GetNonPrimaryDisplay();

                var requests =
                    context.CreatePrimarySwitchRequests(
                        newPrimary);

                var result =
                    context.TransactionService.Apply(requests);

                Assert.True(
                    result.Success,
                    result.Error);

                var updatedPrimary =
                    context.QueryService
                        .GetDisplays()
                        .Single(x => x.IsPrimary);

                Assert.Equal(
                    0,
                    updatedPrimary.CurrentState.Position.X);

                Assert.Equal(
                    0,
                    updatedPrimary.CurrentState.Position.Y);
            }
        }

        [Fact]
        public void Apply_Should_Change_RefreshRate_To_60Hz_For_All_Displays()
        {
            // ARRANGE
            var context =
                new DisplayIntegrationTestContext();

            using (var environment =
                new DisplayTestEnvironment(
                    context.SnapshotService,
                    context.TransactionService))
            {
                var displays =
                    context.GetDisplays();

                var targetRefreshRate = new RefreshRate(60);
                var requests =
                    displays
                        .Select(display =>
                            new ApplyDisplayConfigurationRequest(
                                display.AdapterName,
                                false,
                                null,
                                targetRefreshRate,
                                null))
                        .ToList();

                // ACT
                var result =
                    context.TransactionService.Apply(requests);

                // ASSERT
                Assert.True(
                    result.Success,
                    result.Error);

                var updatedDisplays =
                    context.QueryService.GetDisplays();

                foreach (var display in updatedDisplays)
                {
                    Assert.Equal(
                        targetRefreshRate.Value,
                        display.CurrentState.Mode.RefreshRate.Value);
                }
            }
        }

        [Fact]
        public void Apply_Should_Change_Resolution_To_1280x720_For_All_Displays()
        {
            // ARRANGE
            var context =
                new DisplayIntegrationTestContext();

            using (var environment =
                new DisplayTestEnvironment(
                    context.SnapshotService,
                    context.TransactionService))
            {
                var displays =
                    context.GetDisplays();

                var targetResolution =
                    new Resolution(1280, 720);

                var requests =
                    displays
                        .Select(display =>
                            new ApplyDisplayConfigurationRequest(
                                display.AdapterName,
                                false,
                                targetResolution,
                                null,
                                null))
                        .ToList();

                // ACT
                var result =
                    context.TransactionService.Apply(requests);

                // ASSERT
                Assert.True(
                    result.Success,
                    result.Error);

                var updatedDisplays =
                    context.QueryService.GetDisplays();

                foreach (var display in updatedDisplays)
                {
                    Assert.Equal(
                        targetResolution.Width,
                        display.CurrentState.Mode.Resolution.Width);

                    Assert.Equal(
                        targetResolution.Height,
                        display.CurrentState.Mode.Resolution.Height);
                }
            }
        }

        [Fact]
        public void Apply_Should_Change_Primary_Resolution_And_RefreshRate_Together()
        {
            // ARRANGE
            var context =
                new DisplayIntegrationTestContext();

            using (var environment =
                new DisplayTestEnvironment(
                    context.SnapshotService,
                    context.TransactionService))
            {
                var displays =
                    context.GetDisplays();

                var newPrimary =
                    context.GetNonPrimaryDisplay();

                var resolution =
                    new Resolution(1280, 720);

                var refreshRate =
                    new RefreshRate(60);

                var requests =
                    displays
                        .Select(display =>
                            new ApplyDisplayConfigurationRequest(
                                display.AdapterName,
                                display.Identity.Equals(newPrimary.Identity),
                                resolution,
                                refreshRate,
                                null))
                        .ToList();

                // ACT
                var result =
                    context.TransactionService.Apply(requests);

                // ASSERT
                Assert.True(
                    result.Success,
                    result.Error);

                var updatedDisplays =
                    context.QueryService.GetDisplays();

                var updatedPrimary =
                    updatedDisplays.Single(x => x.IsPrimary);

                // Verify primary changed
                Assert.Equal(
                    newPrimary.Identity,
                    updatedPrimary.Identity);

                // Verify primary rebased to (0,0)
                Assert.Equal(
                    0,
                    updatedPrimary.CurrentState.Position.X);

                Assert.Equal(
                    0,
                    updatedPrimary.CurrentState.Position.Y);

                // Verify resolution + refresh rate
                foreach (var display in updatedDisplays)
                {
                    Assert.Equal(
                        1280,
                        display.CurrentState.Mode.Resolution.Width);

                    Assert.Equal(
                        720,
                        display.CurrentState.Mode.Resolution.Height);

                    Assert.Equal(
                        60,
                        display.CurrentState.Mode.RefreshRate.Value);
                }
            }
        }



    }
}
