using GameEngineChecker.Services;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GameEngineChecker
{
	public class GameEngineChecker : GenericPlugin
	{
		private const string ExtensionName = "Game Engine Checker";
		private const int PcGamingWikiMaxRequestsPerWindow = 30;
		private static readonly TimeSpan PcGamingWikiRateLimitWindow = TimeSpan.FromSeconds(60);
		private static readonly ILogger Logger = LogManager.GetLogger();
		private readonly Tagger _tagger;
		private readonly RateLimiter _rateLimiter;

		public override Guid Id { get; } = Guid.Parse("7a21243e-c7cc-4ca7-85bd-f6f96f22e9db");

		public GameEngineChecker(IPlayniteAPI api) : base(api)
		{
			Properties = new GenericPluginProperties
			{
				HasSettings = false
			};
			_tagger = new Tagger(PlayniteApi);
			_rateLimiter = new RateLimiter(PcGamingWikiRateLimitWindow, PcGamingWikiMaxRequestsPerWindow);
		}

		public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
		{
			yield return new MainMenuItem()
			{
				MenuSection = $"@{ExtensionName}",
				Description = ResourceProvider.GetString("LOCGame_Engine_Checker_MenuItemAddTagSelectedGamesDescription"),
				Action = x =>
				{
					try
					{
						Task.Run(() => AddTagsToGames(PlayniteApi.MainView.SelectedGames.ToList()));
					}
					catch (Exception ex)
					{
						Logger.Error(ex, "Failure running add task. Should not happen.");
					}
				}
			};
		}

		private async Task AddTagsToGames(IReadOnlyCollection<Game> games)
		{
			try
			{
				var gamesFilter = new GamesFilter(PlayniteApi);
				var pcGamingWikiLinkProvider = new PcGamingWikiLinkProvider();
				var pcGamingWikiClient = new PcGamingWikiClient(PlayniteApi);
				var enginesParser = new EnginesParser();

				var gameEngineCheckerService = new GameEngineCheckerService(
					PlayniteApi,
					gamesFilter,
					_rateLimiter,
					pcGamingWikiLinkProvider,
					pcGamingWikiClient,
					enginesParser,
					_tagger);

				var addedCount = await gameEngineCheckerService.AddGameEngineTags(games, CancellationToken.None);
				PlayniteApi.Notifications.Add(
					"game_engine_checker__added_count",
					string.Format(ResourceProvider.GetString("LOCGame_Engine_Checker_ResultsMessage"), addedCount),
					NotificationType.Info);
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "Failure while adding engines to games.");
			}
		}
	}
}