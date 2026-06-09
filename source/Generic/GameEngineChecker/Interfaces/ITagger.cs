using System.Collections.Generic;
using System.Threading;
using Playnite.SDK.Models;

namespace GameEngineChecker.Interfaces
{
	public interface ITagger
	{
		void AddEngineTags(Game game, IReadOnlyCollection<string> engines, CancellationToken cancellationToken);
	}
}