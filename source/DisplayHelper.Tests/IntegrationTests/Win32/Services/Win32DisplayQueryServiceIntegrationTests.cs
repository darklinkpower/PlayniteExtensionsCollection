using DisplayHelper.Domain.Displays.ValueObjects;
using DisplayHelper.Infrastructure.Win32.Native;
using DisplayHelper.Infrastructure.Win32.Services;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.IntegrationTests.Win32.Services
{
    [Trait("Category", "Integration")]
    [Trait("Category", "WindowsOnly")]
    public sealed class Win32DisplayQueryServiceIntegrationTests
    {
        private readonly Win32DisplayQueryService _service;

        public Win32DisplayQueryServiceIntegrationTests()
        {
            var displayApi =
                new Win32DisplayApi();

            var registryService =
                new RegistryService();

            var identityProvider =
                new Win32MonitorIdentityProvider(
                    registryService);

            _service =
                new Win32DisplayQueryService(
                    displayApi,
                    identityProvider);
        }

        [Fact]
        public void GetDisplays_Should_Return_At_Least_One_Display()
        {
            var result =
                _service.GetDisplays();

            result.Should()
                .NotBeEmpty();
        }

        [Fact]
        public void GetDisplays_Should_Return_Only_Unique_Display_Ids()
        {
            var result =
                _service.GetDisplays();

            result.Select(x => x.MonitorDeviceId)
                .Should()
                .OnlyHaveUniqueItems();
        }

        [Fact]
        public void GetDisplays_Should_Return_Display_Names()
        {
            var result =
                _service.GetDisplays();

            result.Should()
                .OnlyContain(x =>
                    !string.IsNullOrWhiteSpace(
                        x.AdapterName));
        }

        [Fact]
        public void GetDisplays_Should_Return_Current_Mode()
        {
            var result =
                _service.GetDisplays();

            result.Should()
                .OnlyContain(x =>
                    x.CurrentState != null &&
                    x.CurrentState.Mode != null);
        }

        [Fact]
        public void GetDisplays_Should_Return_Resolution()
        {
            var result =
                _service.GetDisplays();

            result.Should()
                .OnlyContain(x =>
                    x.CurrentState.Mode.Resolution.Width > 0 &&
                    x.CurrentState.Mode.Resolution.Height > 0);
        }

        [Fact]
        public void GetDisplays_Should_Return_RefreshRate()
        {
            var result =
                _service.GetDisplays();

            result.Should()
                .OnlyContain(x =>
                    x.CurrentState.Mode.RefreshRate.Value > 0);
        }

        [Fact]
        public void GetDisplays_Should_Return_Display_Position()
        {
            var result =
                _service.GetDisplays();

            result.Should()
                .OnlyContain(x =>
                    x.CurrentState.Position != null);
        }

        [Fact]
        public void GetDisplays_Should_Return_SupportedModes()
        {
            var result =
                _service.GetDisplays();

            result.Should()
                .OnlyContain(x =>
                    x.SupportedModes != null &&
                    x.SupportedModes.Count > 0);
        }

        [Fact]
        public void GetDisplays_Should_Return_MonitorIdentity()
        {
            var result =
                _service.GetDisplays();

            result.Should()
                .OnlyContain(x =>
                    x.Identity != null);
        }

        [Fact]
        public void GetDisplays_Should_Return_At_Most_One_Primary_Display()
        {
            var result =
                _service.GetDisplays();

            result.Count(x => x.IsPrimary)
                .Should()
                .BeLessThanOrEqualTo(1);
        }

        [Fact]
        public void GetPrimaryDisplay_Should_Return_Primary_Display()
        {
            var result =
                _service.GetPrimaryDisplay();

            result.Should()
                .NotBeNull();

            result.IsPrimary
                .Should()
                .BeTrue();
        }

        [Fact]
        public void GetPrimaryDisplay_Should_Exist_In_Display_List()
        {
            var displays =
                _service.GetDisplays();

            var primary =
                _service.GetPrimaryDisplay();

            displays.Should()
                .Contain(x => x.AdapterId == primary.AdapterId);
        }

        [Fact]
        public void GetDisplay_Should_Return_Display_By_Id()
        {
            var display =
                _service.GetDisplays()
                    .First();

            var result =
                _service.GetDisplayByAdapterId(
                    display.AdapterId);

            result.Should()
                .NotBeNull();

            result.AdapterId.Should()
                .Be(display.AdapterId);
        }

        [Fact]
        public void GetDisplay_Should_Return_Null_For_Invalid_Id()
        {
            var result =
                _service.GetDisplayByAdapterId(
                    "INVALID_DISPLAY_ID");

            result.Should()
                .BeNull();
        }

        [Fact]
        public void GetSupportedModes_Should_Return_Modes()
        {
            var display =
                _service.GetDisplays()
                    .First();

            var result =
                _service.GetSupportedModes(
                    display.AdapterName);

            result.Should()
                .NotBeEmpty();
        }

        [Fact]
        public void GetSupportedModes_Should_Not_Return_Duplicates()
        {
            var display =
                _service.GetDisplays()
                    .First();

            var result =
                _service.GetSupportedModes(
                    display.AdapterName);

            result.Should()
                .OnlyHaveUniqueItems();
        }

        [Fact]
        public void GetSupportedModes_Should_Be_Ordered()
        {
            var display =
                _service.GetDisplays()
                    .First();

            var result =
                _service.GetSupportedModes(
                    display.AdapterName);

            var ordered =
                result
                    .OrderByDescending(x => x.Resolution.Width)
                    .ThenByDescending(x => x.Resolution.Height)
                    .ThenByDescending(x => x.RefreshRate.Value)
                    .ToList();

            result.Should()
                .Equal(ordered);
        }

        [Fact]
        public void GetSupportedModes_Should_Return_Valid_Values()
        {
            var display =
                _service.GetDisplays()
                    .First();

            var result =
                _service.GetSupportedModes(
                    display.AdapterName);

            result.Should()
                .OnlyContain(x =>
                    x.Resolution.Width > 0 &&
                    x.Resolution.Height > 0 &&
                    x.RefreshRate.Value > 0);
        }

        [Fact]
        public void GetDisplays_Should_Return_Valid_Display_Ids()
        {
            var result =
                _service.GetDisplays();

            result.Should()
                .OnlyContain(x =>
                    !string.IsNullOrWhiteSpace(
                        x.AdapterId));
        }

        [Fact]
        public void GetDisplays_Should_Return_Valid_Display_Names()
        {
            var result =
                _service.GetDisplays();

            result.Should()
                .OnlyContain(x =>
                    !string.IsNullOrWhiteSpace(
                        x.AdapterName));
        }

        [Fact]
        public void GetDisplays_Should_Return_Identity_FriendlyName_When_Available()
        {
            var result =
                _service.GetDisplays();

            result.Should()
                .Contain(x =>
                    x.Identity != null &&
                    x.Identity != MonitorIdentity.Unknown);
        }
    }
}
