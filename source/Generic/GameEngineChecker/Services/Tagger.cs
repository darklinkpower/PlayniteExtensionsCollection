using GameEngineChecker.Interfaces;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GameEngineChecker.Services
{
	public class Tagger : ITagger
	{
		private const string TagPrefix = "[Engine]";

		private readonly IPlayniteAPI _api;
		private readonly SemaphoreSlim _semaphore;

		public Tagger(IPlayniteAPI api)
		{
			_api = api;
			_semaphore = new SemaphoreSlim(1, 1);
		}

		public void AddEngineTags(Game game, IReadOnlyCollection<string> engines, CancellationToken cancellationToken)
		{
			try
			{
				_semaphore.Wait(cancellationToken);
				var tagNames = engines.Select(x => $"{TagPrefix} {x}").ToList();
				var tags = _api.Database.Tags.Add(tagNames);

				var newTagsIdsForGame = tags
					.Select(x => x.Id)
					.Except(game.TagIds ?? new List<Guid>());

				if (game.TagIds == null)
				{
					game.TagIds = new List<Guid>();
				}

				game.TagIds.AddRange(newTagsIdsForGame);
				_api.Database.Games.Update(game);
			}
			finally
			{
				_semaphore.Release();
			}
		}
	}
}