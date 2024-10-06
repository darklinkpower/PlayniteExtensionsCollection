using OpenCriticMetadata.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCriticMetadata.Domain.Interfaces
{
    public interface IOpenCriticService
    {
        Task<List<OpenCriticGameResult>> GetGameSearchResultsAsync(string searchTerm, CancellationToken cancelToken = default);
        Task<OpenCriticGameData> GetGameDataAsync(string gameId, CancellationToken cancelToken = default);
        Task<OpenCriticGameData> GetGameDataAsync(OpenCriticGameResult gameData, CancellationToken cancelToken = default);
    }
}
