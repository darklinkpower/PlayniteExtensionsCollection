using GamesSizeCalculator.SteamSizeCalculation;
using System.IO;
using System.Threading.Tasks;

namespace GamesSizeCalculator.Tests
{
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
