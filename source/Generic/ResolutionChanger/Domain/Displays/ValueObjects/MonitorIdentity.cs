using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Domain.Displays.ValueObjects
{
    public sealed class MonitorIdentity : IEquatable<MonitorIdentity>
    {
        // GSM
        public string ManufacturerCode { get; }

        // 5C01
        public string ProductCode { get; }

        // 00998927
        public string SerialNumber { get; }

        // GSM5C01
        public string HardwareId { get; }

        // LG ULTRAGEAR

        public string FriendlyName { get; }

        // Generic PnP Monitor
        public string DeviceDescription { get; }

        // {4d36e96e-e325-11ce-bfc1-08002be10318}\0007
        public string DriverKey { get; }

        public MonitorIdentity(
            string manufacturerCode,
            string productCode,
            string serialNumber,
            string hardwareId,
            string friendlyName,
            string deviceDescription,
            string driverKey)
        {
            ManufacturerCode = manufacturerCode;
            ProductCode = productCode;
            SerialNumber = serialNumber;
            HardwareId = hardwareId;
            FriendlyName = friendlyName;
            DeviceDescription = deviceDescription;
            DriverKey = driverKey;
        }

        public static MonitorIdentity Unknown =>
            new MonitorIdentity(
                "UNKNOWN",
                "UNKNOWN",
                "UNKNOWN",
                "UNKNOWN",
                "UNKNOWN",
                "UNKNOWN",
                "UNKNOWN");

        public bool Equals(MonitorIdentity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(ManufacturerCode, other.ManufacturerCode) &&
                   string.Equals(ProductCode, other.ProductCode) &&
                   string.Equals(SerialNumber, other.SerialNumber) &&
                   string.Equals(HardwareId, other.HardwareId) &&
                   string.Equals(FriendlyName, other.FriendlyName) &&
                   string.Equals(DeviceDescription, other.DeviceDescription) &&
                   string.Equals(DriverKey, other.DriverKey);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MonitorIdentity);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                hash = (hash * 23) + (ManufacturerCode != null ? ManufacturerCode.GetHashCode() : 0);
                hash = (hash * 23) + (ProductCode != null ? ProductCode.GetHashCode() : 0);
                hash = (hash * 23) + (SerialNumber != null ? SerialNumber.GetHashCode() : 0);
                hash = (hash * 23) + (HardwareId != null ? HardwareId.GetHashCode() : 0);
                hash = (hash * 23) + (FriendlyName != null ? FriendlyName.GetHashCode() : 0);
                hash = (hash * 23) + (DeviceDescription != null ? DeviceDescription.GetHashCode() : 0);
                hash = (hash * 23) + (DriverKey != null ? DriverKey.GetHashCode() : 0);

                return hash;
            }
        }

        public static bool operator ==(
            MonitorIdentity left,
            MonitorIdentity right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(
            MonitorIdentity left,
            MonitorIdentity right)
        {
            return !Equals(left, right);
        }
    }
}
