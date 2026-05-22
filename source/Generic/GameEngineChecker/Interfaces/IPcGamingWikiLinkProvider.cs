using System;
using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK.Models;

namespace GameEngineChecker.Interfaces
{
	public interface IPcGamingWikiLinkProvider
	{
		Task<Uri> GetLink(Game game, CancellationToken cancellationToken);
	}
}