using DisplayHelper.Domain.Displays.ValueObjects;
using DisplayHelper.Infrastructure.Win32.Services;
using FluentAssertions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Tests.Tests.Infrastructure.Win32.Services
{
    public sealed class Win32MonitorIdentityProviderTests
    {
        [Fact]
        public void Get_Should_Return_Unknown_When_DeviceId_Is_Null()
        {
            var registry =
                Substitute.For<IRegistryService>();

            var provider =
                new Win32MonitorIdentityProvider(
                    registry);

            var result =
                provider.Get(null);

            result.Should()
                .Be(MonitorIdentity.Unknown);
        }

        [Fact]
        public void Get_Should_Return_Unknown_When_DeviceId_Is_Invalid()
        {
            var registry =
                Substitute.For<IRegistryService>();

            var provider =
                new Win32MonitorIdentityProvider(
                    registry);

            var result =
                provider.Get("INVALID");

            result.Should()
                .Be(MonitorIdentity.Unknown);
        }

        [Fact]
        public void Get_Should_Return_Unknown_When_No_Subkeys_Exist()
        {
            var registry =
                Substitute.For<IRegistryService>();

            registry.GetSubKeyNames(
                    Arg.Any<string>())
                .Returns(Array.Empty<string>());

            var provider =
                new Win32MonitorIdentityProvider(
                    registry);

            var result =
                provider.Get(
                    @"MONITOR\GSM5C01\{GUID}\0007");

            result.Should()
                .Be(MonitorIdentity.Unknown);
        }

        [Fact]
        public void Get_Should_Return_Unknown_When_DriverKey_Does_Not_Match()
        {
            var registry =
                Substitute.For<IRegistryService>();

            registry.GetSubKeyNames(
                    Arg.Any<string>())
                .Returns(new[]
                {
                "7&d6c3718&0&UID256"
                });

            registry.GetValue(
                    Arg.Any<string>(),
                    "Driver")
                .Returns("{GUID}\\9999");

            var provider =
                new Win32MonitorIdentityProvider(
                    registry);

            var result =
                provider.Get(
                    @"MONITOR\GSM5C01\{GUID}\0007");

            result.Should()
                .Be(MonitorIdentity.Unknown);
        }

        [Fact]
        public void Get_Should_Return_Unknown_When_Edid_Is_Missing()
        {
            var registry =
                Substitute.For<IRegistryService>();

            registry.GetSubKeyNames(
                    Arg.Any<string>())
                .Returns(new[]
                {
                "7&d6c3718&0&UID256"
                });

            registry.GetValue(
                    Arg.Any<string>(),
                    "Driver")
                .Returns("{GUID}\\0007");

            registry.GetBinaryValue(
                    Arg.Any<string>(),
                    "EDID")
                .Returns((byte[])null);

            var provider =
                new Win32MonitorIdentityProvider(
                    registry);

            var result =
                provider.Get(
                    @"MONITOR\GSM5C01\{GUID}\0007");

            result.Should()
                .Be(MonitorIdentity.Unknown);
        }

        [Fact]
        public void Get_Should_Return_Parsed_Identity()
        {
            var registry =
                Substitute.For<IRegistryService>();

            registry.GetSubKeyNames(
                    Arg.Any<string>())
                .Returns(new[]
                {
                "7&d6c3718&0&UID256"
                });

            registry.GetValue(
                    Arg.Any<string>(),
                    "Driver")
                .Returns("{GUID}\\0007");

            registry.GetValue(
                    Arg.Any<string>(),
                    "FriendlyName")
                .Returns("LG ULTRAGEAR");

            registry.GetValue(
                    Arg.Any<string>(),
                    "DeviceDesc")
                .Returns("Generic PnP Monitor");

            registry.GetBinaryValue(
                    Arg.Any<string>(),
                    "EDID")
                .Returns(CreateFakeEdid());

            var provider =
                new Win32MonitorIdentityProvider(
                    registry);

            var result =
                provider.Get(
                    @"MONITOR\GSM5C01\{GUID}\0007");

            result.ManufacturerCode
                .Should()
                .Be("DEL");

            result.ProductCode
                .Should()
                .Be("1234");

            result.SerialNumber
                .Should()
                .Be("78563412");

            result.HardwareId
                .Should()
                .Be("GSM5C01");

            result.FriendlyName
                .Should()
                .Be("LG ULTRAGEAR");

            result.DeviceDescription
                .Should()
                .Be("Generic PnP Monitor");

            result.DriverKey
                .Should()
                .Be("{GUID}\\0007");
        }

        private static byte[] CreateFakeEdid()
        {
            var edid = new byte[128];

            ushort manufacturer =
                EncodeManufacturer("DEL");

            edid[8] = (byte)(manufacturer >> 8);
            edid[9] = (byte)(manufacturer & 0xFF);

            edid[10] = 0x34;
            edid[11] = 0x12;

            edid[12] = 0x12;
            edid[13] = 0x34;
            edid[14] = 0x56;
            edid[15] = 0x78;

            return edid;
        }

        private static ushort EncodeManufacturer(
            string manufacturer)
        {
            return (ushort)(
                manufacturer[0] - 64 << 10 |
                manufacturer[1] - 64 << 5 |
                manufacturer[2] - 64);
        }
    }
}
