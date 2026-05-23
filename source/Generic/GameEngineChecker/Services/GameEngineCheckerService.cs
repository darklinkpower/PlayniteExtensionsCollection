using GameEngineChecker.Interfaces;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GameEngineChecker.Services
{
	public class GameEngineCheckerService
	{
		private readonly ILogger _logger = LogManager.GetLogger();
		private readonly IPlayniteAPI _api;
		private readonly IGamesFilter _filter;
		private readonly IRateLimiter _rateLimiter;
		private readonly IPcGamingWikiLinkProvider _linkProvider;
		private readonly IPcGamingWikiClient _client;
		private readonly IEnginesParser _enginesParser;
		private readonly ITagger _tagger;

		public GameEngineCheckerService(
			IPlayniteAPI api,
			IGamesFilter filter,
			IRateLimiter rateLimiter,
			IPcGamingWikiLinkProvider linkProvider,
			IPcGamingWikiClient client,
			IEnginesParser enginesParser,
			ITagger tagger)
		{
			_api = api;
			_filter = filter;
			_rateLimiter = rateLimiter;
			_linkProvider = linkProvider;
			_client = client;
			_enginesParser = enginesParser;
			_tagger = tagger;
		}

		public async Task<int> AddGameEngineTags(
			IReadOnlyList<Game> games,
			Action<float> reportProgress,
			CancellationToken cancellationToken)
		{
			var addedCount = 0;
			try
			{
				using (var _ = _api.Database.BufferedUpdate())
				{
					for (var i = 0; i < games.Count; i++)
					{
						var game = games[i];
						if (cancellationToken.IsCancellationRequested)
						{
							return addedCount;
						}

						if (!_filter.ShouldTheGameBeProcessed(game))
						{
							continue;
						}

						var link = await _linkProvider.GetLink(game, cancellationToken);
						if (link == null)
						{
							_logger.Info($"Could not create PC Gaming Wiki link for game {game.Id} - {game.Name}.");
							continue;
						}

						await _rateLimiter.Limit(games.Count, cancellationToken);
						var engines = await _client.GetEngines(link, game, cancellationToken);
						if (engines == null)
						{
							_logger.Info($"No engines found for game {game.Id} - {game.Name}.");
							continue;
						}

						var parsedEngines = _enginesParser.Parse(engines);
						_tagger.AddEngineTags(game, parsedEngines, cancellationToken);

						addedCount++;
						reportProgress.Invoke(i * 100f / games.Count);
					}
				}

				return addedCount;
			}
			catch (OperationCanceledException)
			{
				return addedCount;
			}
		}
	}
}