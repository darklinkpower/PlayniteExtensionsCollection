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
        Task<List<OpenCriticGameResult>> GetGameSearchResultsAsync(string apiKey, string searchTerm, CancellationToken cancelToken = default);
        Task<OpenCriticGameData> GetGameDataAsync(string apiKey, string gameId, CancellationToken cancelToken = default);
        Task<OpenCriticGameData> GetGameDataAsync(string apiKey, OpenCriticGameResult gameData, CancellationToken cancelToken = default);
    }
}
