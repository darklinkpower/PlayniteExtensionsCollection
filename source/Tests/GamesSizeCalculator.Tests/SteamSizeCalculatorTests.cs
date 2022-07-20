using GamesSizeCalculator.Steam;
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
            Assert.NotNull(completeSize);
            Assert.Equal(expectedBaseSize + expectedDLCsize, completeSize);
            Assert.NotNull(baseSize);
            Assert.Equal(expectedBaseSize, baseSize);
        }

        [Fact]
        public async Task Watch_Dogs()
        {
            long expectedBaseSize =
                1790331721L +
                109784184L + //WW
                12807162011L +
                //asia
                102629169L +
                88449227L +
                1786744869L;

            long expectedDLCsize =
                28037350L +
                30841083L +
                1147L +
                3718567700L +
                525L;

            var calc = new SteamSizeCalculator(new FakeSteamApiClient());

            var completeSize = await calc.GetInstallSizeAsync(243470);
            Assert.NotNull(completeSize);
            Assert.Equal(expectedBaseSize + expectedDLCsize, completeSize);

            var baseSize = await calc.GetInstallSizeAsync(243470, false, false);
            Assert.NotNull(baseSize);
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
            Assert.NotNull(completeSize);
            Assert.Equal(normal + optional + worldwide, completeSize.Value);

            var minimalSize = await calc.GetInstallSizeAsync(48190, false, false);
            Assert.NotNull(minimalSize);
            Assert.Equal(normal + worldwide, minimalSize.Value);
        }

        [Fact]
        public async Task TheDoor()
        {
            var calc = new SteamSizeCalculator(new FakeSteamApiClient());

            var completeSize = await calc.GetInstallSizeAsync(1360440);
            Assert.Null(completeSize);

            var minimalSize = await calc.GetInstallSizeAsync(1360440, false, false);
            Assert.Null(minimalSize);
        }

        //[Theory]
        //[InlineData(243470u)]
        //[InlineData(48190u)]
        //[InlineData(1360440u)]
        public async Task Serialize(uint appId)
        {
            SteamApiClient client = new SteamApiClient();
            var productInfo = await client.GetProductInfo(appId);
            File.WriteAllText($@"D:\code\{appId}.json", Newtonsoft.Json.JsonConvert.SerializeObject(productInfo));
        }
    }

    internal class FakeSteamApiClient : ISteamApiClient
    {
        public FakeSteamApiClient()
        {
        }

        public bool IsConnected { get; set; }

        public bool IsLoggedIn { get; set; }
        public string FilePath { get; }

        public async Task<SteamKit2.EResult> Connect()
        {
            IsConnected = true;
            return SteamKit2.EResult.OK;
        }

        public void Dispose()
        {
            Logout();
        }

        public async Task<SteamKit2.KeyValue> GetProductInfo(uint id)
        {
            await Login();
            string fileContent = File.ReadAllText($"./Data/{id}.json");
            var output = Newtonsoft.Json.JsonConvert.DeserializeObject<SteamKit2.KeyValue>(fileContent);
            return output;
        }

        public async Task<SteamKit2.EResult> Login()
        {
            IsConnected = true;
            IsLoggedIn = true;
            return SteamKit2.EResult.OK;
        }

        public void Logout()
        {
            IsConnected = false;
            IsLoggedIn = false;
        }
    }
}
