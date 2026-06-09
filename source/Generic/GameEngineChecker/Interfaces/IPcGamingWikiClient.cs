using System;
using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK.Models;

namespace GameEngineChecker.Interfaces
{
	public interface IPcGamingWikiClient
	{
		Task<string> GetEngines(Uri link, Game game, CancellationToken cancellationToken);
	}
}