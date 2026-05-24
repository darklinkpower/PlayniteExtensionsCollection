using DisplayHelper.Domain.Displays.Interfaces;
using DisplayHelper.Infrastructure.Win32.Services;
using FluentAssertions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WinApi.Flags;
using static WinApi.Structs;

namespace DisplayHelper.Tests.Tests.Infrastructure.Win32.Services
{
    public sealed class Win32DisplayQueryServiceTests
    {
        [Fact]
        public void GetDisplays_Should_Return_Empty_When_No_Displays_Exist()
        {
            var api =
                Substitute.For<IWin32DisplayApi>();

            var edidReader =
                Substitute.For<IMonitorIdentityProvider>();

            DISPLAY_DEVICE ignored = default;

            api.EnumDisplayDevices(
                    null,
                    0,
                    ref ignored,
                    0)
                .ReturnsForAnyArgs(false);

            var service =
                new Win32DisplayQueryService(
                    api,
                    edidReader);

            var result =
                service.GetDisplays();

            result.Should().BeEmpty();
        }

        [Fact]
        public void GetDisplays_Should_Ignore_Disconnected_Displays()
        {
            var api =
                Substitute.For<IWin32DisplayApi>();

            var monitorIdentityProvider =
                Substitute.For<IMonitorIdentityProvider>();

            api
                .WhenForAnyArgs(x =>
                    x.EnumDisplayDevices(
                        default,
                        default,
                        ref Arg.Any<DISPLAY_DEVICE>(),
                        default))
                .Do(call =>
                {
                    var device =
                        call.ArgAt<DISPLAY_DEVICE>(2);

                    device.DeviceName = @"\\.\DISPLAY1";

                    device.StateFlags =
                        0;

                    call[2] = device;
                });

            DISPLAY_DEVICE ignored = default;

            api.EnumDisplayDevices(
                    null,
                    0,
                    ref ignored,
                    0)
                .ReturnsForAnyArgs(true);

            api.EnumDisplayDevices(
                    null,
                    1,
                    ref ignored,
                    0)
                .ReturnsForAnyArgs(false);

            var service =
                new Win32DisplayQueryService(
                    api,
                    monitorIdentityProvider);

            var result =
                service.GetDisplays();

            result.Should().BeEmpty();
        }

        [Fact]
        public void GetDisplays_Should_Return_Primary_Display()
        {
            var api =
                Substitute.For<IWin32DisplayApi>();

            var monitorIdentityProvider =
                Substitute.For<IMonitorIdentityProvider>();

            SetupPrimaryDisplay(api);

            var service =
                new Win32DisplayQueryService(
                    api,
                    monitorIdentityProvider);

            var result =
                service.GetPrimaryDisplay();

            result.Should().NotBeNull();

            result.IsPrimary.Should().BeTrue();

            result.AdapterId.Should().Be(@"\\.\DISPLAY1");
        }

        [Fact]
        public void GetDisplay_Should_Return_Display_By_Id()
        {
            var api =
                Substitute.For<IWin32DisplayApi>();

            var monitorIdentityProvider =
                Substitute.For<IMonitorIdentityProvider>();

            SetupPrimaryDisplay(api);

            var service =
                new Win32DisplayQueryService(
                    api,
                    monitorIdentityProvider);

            var result =
                service.GetDisplayByAdapterId(@"\\.\DISPLAY1");

            result.Should().NotBeNull();

            result.AdapterName.Should().Be(@"\\.\DISPLAY1");
        }

        [Fact]
        public void GetSupportedModes_Should_Remove_Duplicates()
        {
            var api =
                Substitute.For<IWin32DisplayApi>();

            var monitorIdentityProvider =
                Substitute.For<IMonitorIdentityProvider>();

            SetupDuplicateModes(api);

            var service =
                new Win32DisplayQueryService(
                    api,
                    monitorIdentityProvider);

            var result =
                service.GetSupportedModes(
                    @"\\.\DISPLAY1");

            result.Should().HaveCount(1);

            result[0].Resolution.Width
                .Should().Be(1920);

            result[0].Resolution.Height
                .Should().Be(1080);
        }

        private static void SetupPrimaryDisplay2(IWin32DisplayApi api)
        {
            var devices = new List<DISPLAY_DEVICE>
            {
                new DISPLAY_DEVICE
                {
                    DeviceName = @"\\.\DISPLAY1",
                    DeviceString = "Test Monitor",
                    StateFlags =
                        DisplayDeviceStateFlags.AttachedToDesktop |
                        DisplayDeviceStateFlags.PrimaryDevice
                },
                new DISPLAY_DEVICE
                {
                    DeviceName = @"\\.\DISPLAY2",
                    DeviceString = "Secondary test monitor",
                    StateFlags =
                        DisplayDeviceStateFlags.AttachedToDesktop
                }
            };

            api
                .EnumDisplayDevices(
                    Arg.Any<string>(),
                    Arg.Any<uint>(),
                    ref Arg.Any<DISPLAY_DEVICE>(),
                    Arg.Any<uint>())
                .Returns(callInfo =>
                {
                    var index = callInfo.ArgAt<uint>(1);

                    if (index >= devices.Count)
                    {
                        return false;
                    }

                    var result = devices[(int)index];

                    callInfo[2] = result;

                    return true;
                });
        }

        private static void SetupPrimaryDisplay(
            IWin32DisplayApi api)
        {
            int callCount = 0;

            api
                .WhenForAnyArgs(x =>
                    x.EnumDisplayDevices(
                        default,
                        default,
                        ref Arg.Any<DISPLAY_DEVICE>(),
                        default))
                .Do(call =>
                {
                    if (callCount == 0)
                    {
                        var device =
                            call.ArgAt<DISPLAY_DEVICE>(2);

                        device.DeviceName =
                            @"\\.\DISPLAY1";

                        device.DeviceString =
                            "Test Monitor";

                        device.StateFlags =
                            DisplayDeviceStateFlags.AttachedToDesktop |
                            DisplayDeviceStateFlags.PrimaryDevice;

                        call[2] = device;
                    }

                    callCount++;
                });

            DISPLAY_DEVICE ignored = default;

            api.EnumDisplayDevices(
                    null,
                    0,
                    ref ignored,
                    0)
                .ReturnsForAnyArgs(true);

            api.EnumDisplayDevices(
                    null,
                    1,
                    ref ignored,
                    0)
                .ReturnsForAnyArgs(false);

            SetupCurrentMode(api);
        }

        private static void SetupCurrentMode(
            IWin32DisplayApi api)
        {
            api
                .WhenForAnyArgs(x =>
                    x.EnumDisplaySettings(
                        default,
                        default,
                        ref Arg.Any<DEVMODE>()))
                .Do(call =>
                {
                    var mode =
                        call.ArgAt<DEVMODE>(2);

                    mode.dmPelsWidth = 1920;
                    mode.dmPelsHeight = 1080;
                    mode.dmDisplayFrequency = 60;

                    call[2] = mode;
                });

            DEVMODE ignored = default;

            api.EnumDisplaySettings(
                    Arg.Any<string>(),
                    Arg.Any<int>(),
                    ref ignored)
                .ReturnsForAnyArgs(true);
        }

        private static void SetupDuplicateModes(
            IWin32DisplayApi api)
        {
            api
                .WhenForAnyArgs(x =>
                    x.EnumDisplaySettings(
                        default,
                        default,
                        ref Arg.Any<DEVMODE>()))
                .Do(call =>
                {
                    var mode =
                        call.ArgAt<DEVMODE>(2);

                    mode.dmPelsWidth = 1920;
                    mode.dmPelsHeight = 1080;
                    mode.dmDisplayFrequency = 60;

                    call[2] = mode;
                });

            DEVMODE ignored = default;

            api.EnumDisplaySettings(
                    Arg.Any<string>(),
                    Arg.Is<int>(x => x < 2),
                    ref ignored)
                .ReturnsForAnyArgs(true);

            api.EnumDisplaySettings(
                    Arg.Any<string>(),
                    2,
                    ref ignored)
                .ReturnsForAnyArgs(false);
        }
    }
}
