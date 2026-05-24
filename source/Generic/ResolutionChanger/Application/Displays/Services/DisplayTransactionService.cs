using DisplayHelper.Application.Displays.DTOs;
using DisplayHelper.Application.Mapping;
using DisplayHelper.Domain.Displays.Entities;
using DisplayHelper.Domain.Displays.Interfaces;
using DisplayHelper.Domain.Displays.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Application.Displays.Services
{
    public sealed class DisplayTransactionService : IDisplayTransactionService
    {
        private readonly IDisplaySnapshotService _snapshotService;
        private readonly IDisplayConfigurationService _configurationService;
        private readonly IDisplayTopologyService _topologyService;
        private readonly IDisplayTransactionCommitService _commitService;

        public DisplayTransactionService(
            IDisplaySnapshotService snapshotService,
            IDisplayConfigurationService configurationService,
            IDisplayTopologyService topologyService,
            IDisplayTransactionCommitService commitService)
        {
            _snapshotService = snapshotService;
            _configurationService = configurationService;
            _topologyService = topologyService;
            _commitService = commitService;
        }

        public DisplayTransactionResult Apply(
            IReadOnlyList<ApplyDisplayConfigurationRequest> requests)
        {
            var snapshot =
                _snapshotService.Capture();

            try
            {
                return ExecuteTransaction(
                    requests,
                    snapshot,
                    rollbackOnFailure: true);
            }
            catch (Exception ex)
            {
                return Rollback(
                    snapshot,
                    $"Unhandled exception: {ex}");
            }
        }

        private DisplayTransactionResult ExecuteTransaction(
            IReadOnlyList<ApplyDisplayConfigurationRequest> requests,
            DisplaySnapshot snapshot,
            bool rollbackOnFailure)
        {
            // Only apply changes to displays that currently exist.
            var availableDisplayIds =
                snapshot.Configurations
                    .Select(x => x.DisplayId)
                    .Distinct()
                    .ToHashSet();

            var filteredRequests =
                requests
                    .Where(x => availableDisplayIds.Contains(x.DisplayId))
                    .ToList();

            if (filteredRequests.Count == 0)
            {
                return DisplayTransactionResult.NoChangesApplied();
            }

            var normalized =
                RebaseDisplayPositionsForPrimaryChange(
                    filteredRequests,
                    snapshot);

            var primaryTargets =
                normalized
                    .Where(x => x.SetAsPrimary)
                    .ToList();

            if (primaryTargets.Count > 1)
            {
                return DisplayTransactionResult.InvalidConfiguration(
                    "Only one display can be primary.");
            }

            var primary =
                primaryTargets.SingleOrDefault();

            // STEP 1: Apply primary first, otherwise CHANGEDISPLAYSETTINGSEX may return FAILURE
            if (primary != null)
            {
                var result =
                    _configurationService.ApplyConfiguration(
                        primary,
                        DisplayApplyMode.Transactional);

                if (!result.Success)
                {
                    return rollbackOnFailure
                        ? Rollback(
                            snapshot,
                            $"Failed applying primary '{primary.DisplayId}': {result.Error}")
                        : DisplayTransactionResult.FailedUnrecovered(result.Error);
                }
            }

            // STEP 2: Apply NON-primary displays
            foreach (var request in normalized.Where(x => !x.SetAsPrimary))
            {
                var result =
                    _configurationService.ApplyConfiguration(
                        request,
                        DisplayApplyMode.Transactional);

                if (!result.Success)
                {
                    return rollbackOnFailure
                        ? Rollback(
                            snapshot,
                            $"Failed applying '{request.DisplayId}': {result.Error}")
                        : DisplayTransactionResult.FailedUnrecovered(result.Error);
                }
            }

            // STEP 3: Commit transaction
            var commit = _commitService.Commit();
            if (!commit.Success)
            {
                return rollbackOnFailure
                    ? Rollback(
                        snapshot,
                        $"Failed committing transaction: {commit.Error}")
                    : DisplayTransactionResult.FailedUnrecovered(commit.Error);
            }

            return DisplayTransactionResult.Succeeded();
        }

        private DisplayTransactionResult Rollback(
            DisplaySnapshot snapshot,
            string error)
        {
            try
            {
                var topologyResult =
                    _topologyService.SetTopology(snapshot.Topology);

                if (!topologyResult.Success)
                {
                    return DisplayTransactionResult.FailedUnrecovered(
                        $"{error}\nRollback failed restoring topology.");
                }

                var rollbackRequests =
                    snapshot.Configurations
                        .Select(Map)
                        .ToList();

                var rollbackResult =
                    ExecuteTransaction(
                        rollbackRequests,
                        snapshot,
                        rollbackOnFailure: false);

                return rollbackResult.Success
                    ? DisplayTransactionResult.FailedRecovered(error)
                    : DisplayTransactionResult.FailedUnrecovered(
                        $"{error}\nRollback failed restoring configuration.");
            }
            catch (Exception ex)
            {
                return DisplayTransactionResult.FailedUnrecovered(
                    $"{error}\nRollback exception: {ex}");
            }
        }

        private static ApplyDisplayConfigurationRequest Map(
            DisplayConfiguration configuration)
        {
            return new ApplyDisplayConfigurationRequest(
                configuration.DisplayId,
                configuration.SetAsPrimary,
                configuration.State.Mode.Resolution,
                configuration.State.Mode.RefreshRate);
        }

        /// <summary>
        /// <para>Rebases all display coordinates so the target primary display becomes the new virtual desktop origin at (0,0).</para>
        ///
        /// <para>Win32 display layouts are expressed in a shared virtual desktop space whose coordinate origin is always anchored to the primary monitor.</para>
        ///
        /// <para>When changing the primary display, all other monitor coordinates must be translated relative to the new primary; otherwise the resulting topology may become invalid or monitors may shift unexpectedly.</para>
        ///
        /// <para>This method preserves the relative arrangement between monitors while transforming the layout into the coordinate space required by the future primary display.</para>
        /// </summary>
        private static IReadOnlyList<ApplyDisplayConfigurationRequest>
            RebaseDisplayPositionsForPrimaryChange(
            IReadOnlyList<ApplyDisplayConfigurationRequest> configurations,
            DisplaySnapshot snapshot)
        {
            var list =
                configurations.ToList();

            var primary =
                list.SingleOrDefault(x => x.SetAsPrimary);

            // No primary change requested.
            // Preserve coordinates exactly as provided.
            if (primary is null)
            {
                return list;
            }

            var matchingPrimarySnapshotConfig =
                snapshot.Configurations
                    .SingleOrDefault(x => x.DisplayId == primary.DisplayId);

            var offsetX =
                matchingPrimarySnapshotConfig?.State.Position.X ?? 0;

            var offsetY =
                matchingPrimarySnapshotConfig?.State.Position.Y ?? 0;

            var normalized =
                new List<ApplyDisplayConfigurationRequest>();

            foreach (var configuration in list)
            {
                var matchingSnapshotConfig =
                    snapshot.Configurations
                        .SingleOrDefault(x => x.DisplayId == configuration.DisplayId);

                var oldState =
                    matchingSnapshotConfig?.State;

                var oldPosition =
                    oldState?.Position ?? new DisplayPosition(0, 0);

                // Rebase all displays relative to new primary.
                var newPosition =
                    new DisplayPosition(
                        oldPosition.X - offsetX,
                        oldPosition.Y - offsetY);

                normalized.Add(
                    new ApplyDisplayConfigurationRequest(
                        configuration.DisplayId,
                        configuration.SetAsPrimary,
                        configuration.Resolution,
                        configuration.RefreshRate,
                        newPosition));
            }

            return normalized;
        }


    }
}
