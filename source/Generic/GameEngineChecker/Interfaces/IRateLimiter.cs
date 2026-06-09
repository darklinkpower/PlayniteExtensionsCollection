using System.Threading;
using System.Threading.Tasks;

namespace GameEngineChecker.Interfaces
{
	public interface IRateLimiter
	{
		Task Limit(int batchSize, CancellationToken cancellationToken);
	}
}