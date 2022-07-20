using SteamKit2;
using System;
using System.Threading.Tasks;

namespace GamesSizeCalculator.SteamSizeCalculation
{
    public interface ISteamApiClient : IDisposable
    {
        bool IsConnected { get; }
        bool IsLoggedIn { get; }

        Task<EResult> Connect();
        Task<KeyValue> GetProductInfo(uint id);
        Task<EResult> Login();
        void Logout();
    }
}