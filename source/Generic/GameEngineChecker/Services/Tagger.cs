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

		private readonly ILogger _logger = LogManager.GetLogger();
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
				var tags = _api.Database.Tags.Add(tagNames).ToList();

				var newTagsIdsForGame = tags
					.Select(x => x.Id)
					.Except(game.TagIds ?? new List<Guid>())
					.ToList();

				if (newTagsIdsForGame.Count == 0)
				{
					return;
				}

				if (game.TagIds == null)
				{
					game.TagIds = new List<Guid>();
				}

				game.TagIds.AddRange(newTagsIdsForGame);
				_api.Database.Games.Update(game);

				var addedTagNames = newTagsIdsForGame
					.Select(id => tags.FirstOrDefault(x => x.Id == id))
					.Where(x => x != null)
					.Select(x => x.Name);

				_logger.Info($"Added game engine(s) {string.Join(", ", addedTagNames)} to {game.Name}");
			}
			finally
			{
				_semaphore.Release();
			}
		}
	}
}