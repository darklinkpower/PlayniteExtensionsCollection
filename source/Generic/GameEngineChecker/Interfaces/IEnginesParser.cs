using System.Collections.Generic;

namespace GameEngineChecker.Interfaces
{
	public interface IEnginesParser
	{
		IReadOnlyCollection<string> Parse(string engines);
	}
}