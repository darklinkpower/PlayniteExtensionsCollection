using Playnite.SDK.Models;

namespace GameEngineChecker.Interfaces
{
	public interface IGamesFilter
	{
		bool ShouldTheGameBeProcessed(Game game);
	}
}