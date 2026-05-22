using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameEngineChecker.Interfaces
{
	public interface IPcGamingWikiClient
	{
		Task<string> GetEngines(Uri link, CancellationToken cancellationToken);
	}
}