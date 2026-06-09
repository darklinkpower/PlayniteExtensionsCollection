using System;
using System.Collections.Generic;
using System.Linq;
using GameEngineChecker.Interfaces;

namespace GameEngineChecker.Services
{
	public class EnginesParser : IEnginesParser
	{
		public IReadOnlyCollection<string> Parse(string engines)
		{
			return engines
				.Split(new[] { "Engine:", ",Engine:" }, StringSplitOptions.RemoveEmptyEntries)
				.Distinct()
				.ToList();
		}
	}
}