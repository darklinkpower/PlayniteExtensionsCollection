using DisplayHelper.Domain.Displays.ValueObjects;
using DisplayHelper.Infrastructure.Win32.Native;
using DisplayHelper.Infrastructure.Win32.Services;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WinApi.Flags;
using static WinApi.Structs;

namespace DisplayHelper.IntegrationTests.Win32.Services
{
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public sealed class Win32MonitorIdentityProviderIntegrationTests
    {
        private static readonly Win32DisplayApi Win32DisplayApi = new Win32DisplayApi();

        [Fact]
        public void Get_Should_Return_Identity_For_Real_Monitor()
        {
            var monitorDeviceId =
                GetFirstMonitorDeviceId();

            monitorDeviceId.Should()
                .NotBeNullOrWhiteSpace();

            var provider =
                new Win32MonitorIdentityProvider(
                    new RegistryService());

            var result =
                provider.Get(
                    monitorDeviceId);

            result.Should()
                .NotBe(MonitorIdentity.Unknown);

            result.FriendlyName
                .Should()
                .NotBeNullOrWhiteSpace();

            result.ManufacturerCode
                .Should()
                .NotBeNullOrWhiteSpace();

            result.ProductCode
                .Should()
                .NotBeNullOrWhiteSpace();

            result.SerialNumber
                .Should()
                .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Get_Should_Return_FriendlyName()
        {
            var monitorDeviceId =
                GetFirstMonitorDeviceId();

            var provider =
                new Win32MonitorIdentityProvider(
                    new RegistryService());

            var result =
                provider.Get(
                    monitorDeviceId);

            result.FriendlyName
                .Should()
                .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Get_Should_Return_HardwareId()
        {
            var monitorDeviceId =
                GetFirstMonitorDeviceId();

            var provider =
                new Win32MonitorIdentityProvider(
                    new RegistryService());

            var result =
                provider.Get(
                    monitorDeviceId);

            result.HardwareId
                .Should()
                .NotBeNullOrWhiteSpace();
        }

        private static string GetFirstMonitorDeviceId()
        {
            DISPLAY_DEVICE adapter =
                new DISPLAY_DEVICE();

            adapter.cb =
                System.Runtime.InteropServices.Marshal
                    .SizeOf(adapter);

            for (uint i = 0;
                 Win32DisplayApi.EnumDisplayDevices(
                     null,
                     i,
                     ref adapter,
                     0);
                 i++)
            {
                if (!adapter.StateFlags.HasFlag(
                        DisplayDeviceStateFlags.AttachedToDesktop))
                {
                    continue;
                }

                DISPLAY_DEVICE monitor =
                    new DISPLAY_DEVICE();

                monitor.cb =
                    System.Runtime.InteropServices.Marshal
                        .SizeOf(monitor);

                if (Win32DisplayApi.EnumDisplayDevices(
                    adapter.DeviceName,
                    0,
                    ref monitor,
                    0))
                {
                    return monitor.DeviceID;
                }
            }

            return null;
        }
    }
}
