using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace DisplayHelper.Native
{
    /// <summary>
    /// Resolves the real monitor name (e.g. "LG ULTRAGEAR") for a GDI display
    /// device name (e.g. "\\.\DISPLAY1") using the CCD display config API,
    /// the same source Windows Settings uses.
    /// </summary>
    public static class MonitorNames
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private const uint QDC_ONLY_ACTIVE_PATHS = 0x00000002;
        private const int ERROR_SUCCESS = 0;
        private const int ERROR_INSUFFICIENT_BUFFER = 122;
        private const int DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME = 1;
        private const int DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME = 2;

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DISPLAYCONFIG_PATH_SOURCE_INFO
        {
            public LUID adapterId;
            public uint id;
            public uint modeInfoIdx;
            public uint statusFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DISPLAYCONFIG_RATIONAL
        {
            public uint Numerator;
            public uint Denominator;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DISPLAYCONFIG_PATH_TARGET_INFO
        {
            public LUID adapterId;
            public uint id;
            public uint modeInfoIdx;
            public uint outputTechnology;
            public uint rotation;
            public uint scaling;
            public DISPLAYCONFIG_RATIONAL refreshRate;
            public uint scanLineOrdering;
            public int targetAvailable;
            public uint statusFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DISPLAYCONFIG_PATH_INFO
        {
            public DISPLAYCONFIG_PATH_SOURCE_INFO sourceInfo;
            public DISPLAYCONFIG_PATH_TARGET_INFO targetInfo;
            public uint flags;
        }

        // DISPLAYCONFIG_MODE_INFO is 64 bytes; contents are not needed here.
        [StructLayout(LayoutKind.Sequential, Size = 64)]
        private struct DISPLAYCONFIG_MODE_INFO
        {
            public uint infoType;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DISPLAYCONFIG_DEVICE_INFO_HEADER
        {
            public int type;
            public int size;
            public LUID adapterId;
            public uint id;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DISPLAYCONFIG_SOURCE_DEVICE_NAME
        {
            public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string viewGdiDeviceName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DISPLAYCONFIG_TARGET_DEVICE_NAME
        {
            public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
            public uint flags;
            public uint outputTechnology;
            public ushort edidManufactureId;
            public ushort edidProductCodeId;
            public uint connectorInstance;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string monitorFriendlyDeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string monitorDevicePath;
        }

        [DllImport("user32.dll")]
        private static extern int GetDisplayConfigBufferSizes(uint flags, out uint numPathArrayElements, out uint numModeInfoArrayElements);

        [DllImport("user32.dll")]
        private static extern int QueryDisplayConfig(
            uint flags,
            ref uint numPathArrayElements,
            [Out] DISPLAYCONFIG_PATH_INFO[] pathArray,
            ref uint numModeInfoArrayElements,
            [Out] DISPLAYCONFIG_MODE_INFO[] modeInfoArray,
            IntPtr currentTopologyId);

        [DllImport("user32.dll")]
        private static extern int DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_SOURCE_DEVICE_NAME requestPacket);

        [DllImport("user32.dll")]
        private static extern int DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_TARGET_DEVICE_NAME requestPacket);

        /// <summary>
        /// Custom names set by the user, keyed by monitor device path
        /// (stable per monitor, unlike DISPLAY numbers). Set by the plugin from its settings.
        /// </summary>
        public static Dictionary<string, string> CustomNames { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public sealed class MonitorInfo
        {
            // \\.\DISPLAY1
            public string GdiName { get; set; }
            // Name reported by the monitor (EDID), may be empty for some panels
            public string FriendlyName { get; set; }
            // \\?\DISPLAY#HPN3698#5&...#{e6f07b5f-...}, stable id used for custom names
            public string DevicePath { get; set; }
            public uint AdapterLow { get; set; }
            public int AdapterHigh { get; set; }
            public uint TargetId { get; set; }
        }

        /// <summary>
        /// Numbers monitors the way Windows Settings (Display > Identify) does:
        /// ordered by graphics adapter, then by the adapter's output (target) id.
        /// Returns GDI name ("\\.\DISPLAY1") -> number (1-based).
        /// </summary>
        public static Dictionary<string, int> GetWindowsNumbers(List<MonitorInfo> monitors = null)
        {
            monitors = monitors ?? GetMonitors();
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var ordered = monitors
                .GroupBy(x => x.DevicePath, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First())
                .OrderBy(x => x.AdapterHigh)
                .ThenBy(x => x.AdapterLow)
                .ThenBy(x => x.TargetId)
                .ToList();
            for (var i = 0; i < ordered.Count; i++)
            {
                if (!result.ContainsKey(ordered[i].GdiName))
                {
                    result[ordered[i].GdiName] = i + 1;
                }
            }

            return result;
        }

        /// <summary>"Display 3" (Windows number), or "DISPLAY1" if unknown.</summary>
        public static string GetDisplayNumberText(string gdiName, Dictionary<string, int> windowsNumbers)
        {
            if (gdiName != null && windowsNumbers != null && windowsNumbers.TryGetValue(gdiName, out var number))
            {
                return $"Display {number}";
            }

            return (gdiName ?? string.Empty).Replace(@"\\.\", string.Empty);
        }

        /// <summary>
        /// Lists active monitors with their real names and device paths. Never throws.
        /// </summary>
        public static List<MonitorInfo> GetMonitors()
        {
            var result = new List<MonitorInfo>();
            try
            {
                DISPLAYCONFIG_PATH_INFO[] paths;
                DISPLAYCONFIG_MODE_INFO[] modes;
                uint pathCount, modeCount;
                int status;
                var attempts = 0;
                do
                {
                    status = GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, out pathCount, out modeCount);
                    if (status != ERROR_SUCCESS)
                    {
                        return result;
                    }

                    paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
                    modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
                    status = QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
                    attempts++;
                } while (status == ERROR_INSUFFICIENT_BUFFER && attempts < 5);

                if (status != ERROR_SUCCESS)
                {
                    return result;
                }

                for (var i = 0; i < pathCount; i++)
                {
                    var path = paths[i];

                    var source = new DISPLAYCONFIG_SOURCE_DEVICE_NAME();
                    source.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME;
                    source.header.size = Marshal.SizeOf(typeof(DISPLAYCONFIG_SOURCE_DEVICE_NAME));
                    source.header.adapterId = path.sourceInfo.adapterId;
                    source.header.id = path.sourceInfo.id;
                    if (DisplayConfigGetDeviceInfo(ref source) != ERROR_SUCCESS ||
                        string.IsNullOrEmpty(source.viewGdiDeviceName))
                    {
                        continue;
                    }

                    var target = new DISPLAYCONFIG_TARGET_DEVICE_NAME();
                    target.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME;
                    target.header.size = Marshal.SizeOf(typeof(DISPLAYCONFIG_TARGET_DEVICE_NAME));
                    target.header.adapterId = path.targetInfo.adapterId;
                    target.header.id = path.targetInfo.id;
                    if (DisplayConfigGetDeviceInfo(ref target) != ERROR_SUCCESS)
                    {
                        continue;
                    }

                    result.Add(new MonitorInfo
                    {
                        GdiName = source.viewGdiDeviceName,
                        FriendlyName = target.monitorFriendlyDeviceName?.Trim() ?? string.Empty,
                        DevicePath = string.IsNullOrEmpty(target.monitorDevicePath) ? source.viewGdiDeviceName : target.monitorDevicePath,
                        AdapterLow = path.targetInfo.adapterId.LowPart,
                        AdapterHigh = path.targetInfo.adapterId.HighPart,
                        TargetId = path.targetInfo.id
                    });
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Error while getting monitors");
            }

            return result;
        }

        /// <summary>
        /// Returns a map of GDI device name ("\\.\DISPLAY1") to its full label:
        /// "Custom name (Real name, Display 1)" when a custom name is set,
        /// otherwise "Real name (Display 1)", using the Windows Settings number. Monitors with no name are left out.
        /// Never throws; returns an empty map on failure.
        /// </summary>
        public static Dictionary<string, string> GetMonitorFriendlyNames()
        {
            var customByGdi = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var realByGdi = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var customNames = CustomNames;
            var monitors = GetMonitors();
            var windowsNumbers = GetWindowsNumbers(monitors);
            foreach (var monitor in monitors)
            {
                string custom = null;
                if (customNames != null &&
                    customNames.TryGetValue(monitor.DevicePath, out var value) &&
                    !string.IsNullOrWhiteSpace(value))
                {
                    custom = value.Trim();
                }

                AddDistinct(customByGdi, monitor.GdiName, custom);
                AddDistinct(realByGdi, monitor.GdiName, monitor.FriendlyName);
            }

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var gdi in realByGdi.Keys)
            {
                // Cloned displays share one source; join their names.
                var custom = string.Join(" + ", customByGdi[gdi]);
                var real = string.Join(" + ", realByGdi[gdi]);
                var shortName = GetDisplayNumberText(gdi, windowsNumbers);
                if (custom.Length > 0)
                {
                    result[gdi] = real.Length > 0 && !string.Equals(custom, real, StringComparison.OrdinalIgnoreCase)
                        ? $"{custom} ({real}, {shortName})"
                        : $"{custom} ({shortName})";
                }
                else if (real.Length > 0)
                {
                    result[gdi] = $"{real} ({shortName})";
                }
            }

            return result;
        }

        private static void AddDistinct(Dictionary<string, List<string>> map, string key, string value)
        {
            if (!map.TryGetValue(key, out var list))
            {
                list = new List<string>();
                map[key] = list;
            }

            if (!string.IsNullOrEmpty(value) && !list.Contains(value))
            {
                list.Add(value);
            }
        }
        /// <summary>
        /// Builds a readable label such as "Left Gaming Monitor (LG ULTRAGEAR, Display 1)" or "LG ULTRAGEAR (Display 1)".
        /// Falls back to "DISPLAY1 (adapter name)" when no monitor name is available.
        /// </summary>
        public static string GetLabel(string gdiDeviceName, string adapterDeviceString, Dictionary<string, string> friendlyNames = null)
        {
            if (friendlyNames is null)
            {
                friendlyNames = GetMonitorFriendlyNames();
            }

            var shortName = (gdiDeviceName ?? string.Empty).Replace(@"\\.\", string.Empty);
            if (gdiDeviceName != null && friendlyNames.TryGetValue(gdiDeviceName, out var label))
            {
                return label;
            }

            return string.IsNullOrEmpty(adapterDeviceString)
                ? shortName
                : $"{shortName} ({adapterDeviceString})";
        }
    }
}
