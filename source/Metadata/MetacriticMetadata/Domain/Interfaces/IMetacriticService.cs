using MetacriticMetadata.Domain.Entities;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MetacriticMetadata.Domain.Interfaces
{
    public interface IMetacriticService
    {
        Task<List<MetacriticSearchResult>> GetGameSearchResultsAsync(Game game, string apiKey, CancellationToken cancelToken = default);
        Task<List<MetacriticSearchResult>> GetGameSearchResultsAsync(string gameName, string apiKey, CancellationToken cancelToken = default);
    }
}