using System.Threading;
using System.Threading.Tasks;
using ReviewViewer.Domain;
using ReviewViewer.Infrastructure;

namespace ReviewViewer.Application
{
    public interface ISteamReviewsProvider
    {
        /// <summary>
        /// Fetches Steam reviews for the given app with specified query options and cursor for pagination.
        /// </summary>
        /// <param name="appId">Steam application ID</param>
        /// <param name="options">Query options for filtering and sorting reviews</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// /// <param name="cursor">Cursor token for pagination; default "*" for first page</param>
        /// <returns>Deserialized reviews response DTO or null if failed</returns>
        Task<ReviewsResponseDto> GetReviewsAsync(int appId, QueryOptions options, CancellationToken cancellationToken, string cursor = "*");
    }
}