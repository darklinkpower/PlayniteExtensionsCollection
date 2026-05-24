using DisplayHelper.Domain.Displays.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Infrastructure.Win32.Parsing
{
    public static class EdidParser
    {
        public static MonitorIdentity Parse(
            byte[] edid,
            string hardwareId,
            string friendlyName,
            string deviceDescription,
            string driverKey)
        {
            var manufacturerCode =
                DecodeManufacturerId(edid);

            var productCode =
                BitConverter.ToUInt16(edid, 10)
                    .ToString("X4");

            var serialNumber =
                BitConverter.ToUInt32(edid, 12)
                    .ToString("X8");

            return new MonitorIdentity(
                manufacturerCode,
                productCode,
                serialNumber,
                hardwareId,
                friendlyName,
                deviceDescription,
                driverKey);
        }

        private static string DecodeManufacturerId(
            byte[] edid)
        {
            ushort raw =
                (ushort)((edid[8] << 8) | edid[9]);

            char first =
                (char)(((raw >> 10) & 0x1F) + 64);

            char second =
                (char)(((raw >> 5) & 0x1F) + 64);

            char third =
                (char)((raw & 0x1F) + 64);

            return new string(
                new[] { first, second, third });
        }
    }
}
