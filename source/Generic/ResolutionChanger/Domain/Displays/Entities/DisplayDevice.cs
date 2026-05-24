using DisplayHelper.Domain.Displays.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Domain.Displays.Entities
{
    public sealed class DisplayDevice : IEquatable<DisplayDevice>
    {
        // PCI\VEN_1002&DEV_73DF&SUBSYS_E4451DA2&REV_C5
        public string AdapterId { get; }

        // \\.\DISPLAY1
        public string AdapterName { get; }

        // AMD Radeon RX 6700 XT
        public string AdapterDeviceString { get; }
        public bool IsPrimary { get; }

        // MONITOR\GSM5C01\{4d36e96e-e325-11ce-bfc1-08002be10318}\0007
        public string MonitorDeviceId { get; }

        // \\.\DISPLAY1\Monitor0
        public string MonitorDeviceName { get; }

        // Generic PnP Monitor
        public string MonitorDeviceString { get; }

        public DisplayState CurrentState { get; }

        public IReadOnlyList<DisplayMode> SupportedModes { get; }

        public MonitorIdentity Identity { get; }

        public DisplayDevice(
            string adapterId,
            string adapterName,
            string adapterDeviceString,
            bool isPrimary,
            string monitorDeviceId,
            string monitorDeviceName,
            string monitorDeviceString,
            DisplayState currentMode,
            IReadOnlyList<DisplayMode> supportedModes,
            MonitorIdentity identifier)
        {
            AdapterId = adapterId;
            AdapterName = adapterName;
            AdapterDeviceString = adapterDeviceString;
            IsPrimary = isPrimary;
            MonitorDeviceId = monitorDeviceId;
            MonitorDeviceName = monitorDeviceName;
            MonitorDeviceString = monitorDeviceString;
            CurrentState = currentMode;
            SupportedModes = supportedModes;
            Identity = identifier;
        }

        public bool Equals(DisplayDevice other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(AdapterId, other.AdapterId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(MonitorDeviceId, other.MonitorDeviceId, StringComparison.OrdinalIgnoreCase)
                && Equals(Identity, other.Identity)
                && IsPrimary == other.IsPrimary
                && Equals(CurrentState, other.CurrentState)
                && SupportedModesEqual(SupportedModes, other.SupportedModes);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DisplayDevice);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                hash = (hash * 23) + (AdapterId != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(AdapterId) : 0);
                hash = (hash * 23) + (MonitorDeviceId != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(MonitorDeviceId) : 0);
                hash = (hash * 23) + (Identity != null ? Identity.GetHashCode() : 0);
                hash = (hash * 23) + IsPrimary.GetHashCode();
                hash = (hash * 23) + (CurrentState != null ? CurrentState.GetHashCode() : 0);
                hash = (hash * 23) + GetModesHash(SupportedModes);

                return hash;
            }
        }

        public static bool operator ==(DisplayDevice left, DisplayDevice right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DisplayDevice left, DisplayDevice right)
        {
            return !Equals(left, right);
        }

        private static bool SupportedModesEqual(
            IReadOnlyList<DisplayMode> a,
            IReadOnlyList<DisplayMode> b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null || a.Count != b.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Count; i++)
            {
                if (!Equals(a[i], b[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static int GetModesHash(IReadOnlyList<DisplayMode> modes)
        {
            if (modes == null)
            {
                return 0;
            }

            unchecked
            {
                int hash = 17;

                for (int i = 0; i < modes.Count; i++)
                {
                    hash = (hash * 23) + (modes[i] != null ? modes[i].GetHashCode() : 0);
                }

                return hash;
            }
        }
    }
}
