using System;
using System.Collections.Generic;
using System.Linq;
using GameEngineChecker.Interfaces;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace GameEngineChecker.Services
{
	public class GamesFilter : IGamesFilter
	{
		private readonly HashSet<Guid> _engineTagIds;

		public GamesFilter(IPlayniteAPI api)
		{
			_engineTagIds = api.Database.Tags.Where(x => x.Name.StartsWith("[Engine]")).Select(x => x.Id).ToHashSet();
		}

		public bool ShouldTheGameBeProcessed(Game game)
		{
			return game.TagIds?.All(x => !_engineTagIds.Contains(x)) ?? true;
		}
	}
}