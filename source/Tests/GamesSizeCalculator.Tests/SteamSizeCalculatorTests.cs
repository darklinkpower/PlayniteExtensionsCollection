using GamesSizeCalculator.SteamSizeCalculation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GamesSizeCalculator.Tests
{
    public class SteamSizeCalculatorTests
    {
        [Fact]
        public async Task MasterChiefCollection()
        {
            long expectedBaseSize = 14715108044L;

            long expectedDLCsize =
                9880606811L +
                5364014009L +
                31248440761L +
                21063599446L +
                8582825401L +
                9088632434L +
                9475667570L +
                8546806807L +
                25369702824L +
                21123601548L +
                10099205546L +
                8229782954L +
                1467914176L +
                933914112L +
                564810752L +
                1899134368L +
                1542698112L +
                3724939335L;

            var calc = new SteamSizeCalculator(new FakeSteamApiClient());
            var completeSize = await calc.GetInstallSizeAsync(976730);
            var baseSize = await calc.GetInstallSizeAsync(976730, false, false);
            Assert.Equal(expectedBaseSize + expectedDLCsize, completeSize);
            Assert.Equal(expectedBaseSize, baseSize);
        }

        [Fact]
        public async Task Watch_Dogs()
        {
            long expectedBaseSize =
                1790331721L + //Watch Dogs English
                102629169L + //Watch_Dogs Binaries ASIA 
                88449227L + //Watch_Dogs Support ASIA
                1786744869L + //Watch_Dogs English ASIA
                12807162011L; //Watch Dogs Common

            long expectedDLCsize =
                28037350L + //Watch_Dogs - DLC 0 (293055) Depot
                30841083L + //Watch_Dogs - DLC 1 (293057) Depot
                1147L + //Watch_Dogs - DLC 2 (293059) Depot
                3718567700L + //Watch_Dogs - DLC 3 (293061) Depot
                525L; //Watch_Dogs Season Pass Uplay Activation (293054) Depot

            var calc = new SteamSizeCalculator(new FakeSteamApiClient());

            var completeSize = await calc.GetInstallSizeAsync(243470);
            Assert.Equal(expectedBaseSize + expectedDLCsize, completeSize);

            var baseSize = await calc.GetInstallSizeAsync(243470, false, false);
            Assert.Equal(expectedBaseSize, baseSize);
        }

        [Fact]
        public async Task AssassinsCreedBrotherhood()
        {
            long normal = 9529023303L;
            long optional = 1954919585L;
            long worldwide = 7993844118L;

            var calc = new SteamSizeCalculator(new FakeSteamApiClient());

            var completeSize = await calc.GetInstallSizeAsync(48190);
            Assert.Equal(normal + optional + worldwide, completeSize.Value);

            var minimalSize = await calc.GetInstallSizeAsync(48190, false, false);
            Assert.Equal(normal + worldwide, minimalSize.Value);
        }

        [Fact]
        public async Task MissingDepotsReturnNull()
        {
            var calc = new SteamSizeCalculator(new FakeSteamApiClient());

            //The Door (unreleased)
            Assert.Null(await calc.GetInstallSizeAsync(1360440));

            //Castle Crashers
            Assert.Null(await calc.GetInstallSizeAsync(204360));

            //Two Point Hospital
            Assert.Null(await calc.GetInstallSizeAsync(535930));

            //PC Building Simulator
            Assert.Null(await calc.GetInstallSizeAsync(621060));
        }

        [Fact]
        public async Task Metro2033()
        {
            //For some reason, every depot for Metro 2033 is marked optional
            long expectedSize = 5028692647L + 2884284795L;

            var calc = new SteamSizeCalculator(new FakeSteamApiClient());

            var completeSize = await calc.GetInstallSizeAsync(43110);
            Assert.Equal(expectedSize, completeSize);

            var minimalSize = await calc.GetInstallSizeAsync(43110, false, false);
            Assert.Equal(expectedSize, minimalSize);
        }

        //[Theory]
        //[InlineData(243470u)]
        //[InlineData(48190u)]
        //[InlineData(1360440u)]
        //[InlineData(204360u)] //Castle Crashers
        //[InlineData(621060u)] //PC Building Simulator
        //[InlineData(43110u)] //Metro 2033
        //[InlineData(535930u)] //Two Point Hospital
        public async Task Serialize(uint appId)
        {
            SteamApiClient client = new SteamApiClient();
            var productInfo = await client.GetProductInfo(appId);
            File.WriteAllText($@"D:\code\{appId}.json", Newtonsoft.Json.JsonConvert.SerializeObject(productInfo));
        }
    }
}