using DisplayHelper.Domain.Displays.Entities;
using DisplayHelper.Domain.Displays.Interfaces;
using DisplayHelper.Domain.Displays.ValueObjects;
using DisplayHelper.Infrastructure.Win32.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Application.Displays.Services
{
    public sealed class DisplayResolverService : IDisplayResolver
    {
        private readonly IDisplayQueryService _queryService;

        public DisplayResolverService(IDisplayQueryService queryService)
        {
            _queryService = queryService;
        }

        public DisplayDevice Resolve(MonitorIdentity identity)
        {
            if (identity is null || identity == MonitorIdentity.Unknown)
            {
                throw new ArgumentException("Invalid monitor identity.");
            }

            var displays = _queryService.GetDisplays();

            // 1. Exact match (best case)
            var exact = displays.FirstOrDefault(d =>
                d.Identity.Equals(identity));

            if (exact != null)
            {
                return exact;
            }

            // 2. Fallback: match by serial/hardware fingerprint
            var fallback = displays.FirstOrDefault(d =>
                IsSameMonitor(identity, d.Identity));

            if (fallback != null)
            {
                return fallback;
            }

            throw new InvalidOperationException(
                $"Monitor not found for identity: {identity}");
        }

        private static bool IsSameMonitor(MonitorIdentity a, MonitorIdentity b)
        {
            // Strongest stable signal first
            if (!string.IsNullOrWhiteSpace(a.SerialNumber) &&
                a.SerialNumber == b.SerialNumber)
            {
                return true;
            }

            // Secondary match: driver key (OS-level binding)
            if (!string.IsNullOrWhiteSpace(a.DriverKey) &&
                a.DriverKey == b.DriverKey)
            {
                return true;
            }

            // Tertiary fallback: EDID hardware combo
            if (!string.IsNullOrWhiteSpace(a.HardwareId) &&
                a.HardwareId == b.HardwareId)
            {
                return true;
            }

            return false;
        }
    }
}
