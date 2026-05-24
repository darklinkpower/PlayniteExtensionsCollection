using DisplayHelper.Domain.Displays.ValueObjects;
using DisplayHelper.Infrastructure.Win32.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Infrastructure.Win32.Services
{
    public sealed class Win32MonitorIdentityProvider : IMonitorIdentityProvider
    {
        private readonly IRegistryService _registryService;

        public Win32MonitorIdentityProvider(
            IRegistryService registryService)
        {
            _registryService = registryService;
        }

        public MonitorIdentity Get(
            string monitorDeviceId)
        {
            if (string.IsNullOrWhiteSpace(
                monitorDeviceId))
            {
                return MonitorIdentity.Unknown;
            }

            // MONITOR\GSM5C01\{GUID}\0007

            var parts =
                monitorDeviceId.Split('\\');

            if (parts.Length < 4)
            {
                return MonitorIdentity.Unknown;
            }

            var hardwareId =
                parts[1];

            var driverKey =
                $"{parts[2]}\\{parts[3]}";

            var basePath =
                $@"SYSTEM\CurrentControlSet\Enum\DISPLAY\{hardwareId}";

            var subKeys =
                _registryService.GetSubKeyNames(
                    basePath);

            foreach (var subKey in subKeys)
            {
                var registryPath =
                    $"{basePath}\\{subKey}";

                var registryDriverKey =
                    _registryService.GetValue(
                        registryPath,
                        "Driver") as string;

                if (!string.Equals(
                        registryDriverKey,
                        driverKey,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var friendlyName =
                    NormalizeIndirectRegistryString(
                        _registryService.GetValue(
                            registryPath,
                            "FriendlyName") as string);

                var deviceDescription =
                    NormalizeIndirectRegistryString(
                        _registryService.GetValue(
                            registryPath,
                            "DeviceDesc") as string);

                var edidPath =
                    $"{registryPath}\\Device Parameters";

                var edid =
                    _registryService.GetBinaryValue(
                        edidPath,
                        "EDID");

                if (edid is null ||
                    edid.Length < 128)
                {
                    return MonitorIdentity.Unknown;
                }

                return EdidParser.Parse(
                    edid,
                    hardwareId,
                    friendlyName,
                    deviceDescription,
                    driverKey);
            }

            return MonitorIdentity.Unknown;
        }

        private static string NormalizeIndirectRegistryString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "UNKNOWN";
            }

            if (value.StartsWith("@"))
            {
                var segments =
                    value.Split(new[]{';'}, StringSplitOptions.RemoveEmptyEntries);

                if (segments.Length > 0)
                {
                    var lastSegment =
                        segments[segments.Length - 1]
                            .Trim();

                    if (lastSegment.StartsWith("(") &&
                        lastSegment.EndsWith(")") &&
                        lastSegment.Length > 2)
                    {
                        lastSegment =
                            lastSegment.Substring(
                                1,
                                lastSegment.Length - 2);
                    }

                    if (!string.IsNullOrWhiteSpace(
                            lastSegment))
                    {
                        return lastSegment;
                    }
                }
            }

            return value.Trim();
        }

    }
}
