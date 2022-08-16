using Playnite.SDK.Models;
using System.Threading.Tasks;

namespace GamesSizeCalculator
{
    public interface IOnlineSizeCalculator
    {
        string ServiceName { get; }
        Task<ulong?> GetInstallSizeAsync(Game game);
        bool IsPreferredInstallSizeCalculator(Game game);
    }
}