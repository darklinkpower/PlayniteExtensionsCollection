using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameEngineChecker.Interfaces;
using GameEngineChecker.Services;
using Xunit;

namespace GameEngineChecker.Tests.Services
{
	public class GameEngineCheckerServiceTests
	{
		private readonly Fixture _fixture;
		private readonly GameEngineCheckerService _sut;
		private readonly IGamesFilter _gamesFilter;
		private readonly IPcGamingWikiLinkProvider _pcGamingWikiLinkProvider;
		private readonly IPcGamingWikiClient _pcGamingWikiClient;
		private readonly IEnginesParser _enginesParser;
		private readonly ITagger _tagger;

		public GameEngineCheckerServiceTests()
		{
			_fixture = new Fixture();
			_fixture.Customize(new AutoFakeItEasyCustomization());

			_gamesFilter = _fixture.Freeze<IGamesFilter>();
			_pcGamingWikiLinkProvider = _fixture.Freeze<IPcGamingWikiLinkProvider>();
			_pcGamingWikiClient = _fixture.Freeze<IPcGamingWikiClient>();
			_enginesParser = _fixture.Freeze<IEnginesParser>();
			_tagger = _fixture.Freeze<ITagger>();
			_sut = _fixture.Create<GameEngineCheckerService>();
		}

		[Fact]
		public async Task AddGameEngineTags_AddsEngineTags_WhenTagsShouldBeAdded()
		{
			// Arrange
			var game = _fixture.Create<Game>();
			var link = _fixture.Create<Uri>();
			var engines = "Engine:Unity";
			var parsedEngines = new List<string> { "Unity" };
			SetupSuccessfulRun(game, link, engines, parsedEngines);

			// Act
			await _sut.AddGameEngineTags(new List<Game> { game }, x => { }, CancellationToken.None);

			// Assert
			A.CallTo(() => _tagger.AddEngineTags(game, parsedEngines, CancellationToken.None)).MustHaveHappenedOnceExactly();
		}

		[Fact]
		public async Task AddGameEngineTags_DoesNotGenerateLink_WhenTagsShouldNotBeAdded()
		{
			// Arrange
			var game = _fixture.Create<Game>();
			A.CallTo(() => _gamesFilter.ShouldTheGameBeProcessed(game)).Returns(false);

			// Act
			await _sut.AddGameEngineTags(new List<Game> { game }, x => { }, CancellationToken.None);

			// Assert
			A.CallTo(() => _pcGamingWikiLinkProvider.GetLink(A<Game>._, CancellationToken.None)).MustNotHaveHappened();
		}

		[Fact]
		public async Task AddGameEngineTags_DoesNotCallPcGamingWiki_WhenLinkCouldNotBeGenerated()
		{
			// Arrange
			var game = _fixture.Create<Game>();
			A.CallTo(() => _gamesFilter.ShouldTheGameBeProcessed(game)).Returns(true);
			A.CallTo(() => _pcGamingWikiLinkProvider.GetLink(game, A<CancellationToken>._)).Returns<Uri>(null);

			// Act
			await _sut.AddGameEngineTags(new List<Game> { game }, x => { }, CancellationToken.None);

			// Assert
			A.CallTo(() => _pcGamingWikiClient.GetEngines(A<Uri>._, A<Game>._, A<CancellationToken>._)).MustNotHaveHappened();
		}

		private void SetupSuccessfulRun(Game game, Uri link, string engines, List<string> parsedEngines)
		{
			A.CallTo(() => _gamesFilter.ShouldTheGameBeProcessed(game)).Returns(true);
			A.CallTo(() => _pcGamingWikiLinkProvider.GetLink(game, A<CancellationToken>._)).Returns(link);
			A.CallTo(() => _pcGamingWikiClient.GetEngines(link, A<Game>._, A<CancellationToken>._)).Returns(engines);
			A.CallTo(() => _enginesParser.Parse(engines)).Returns(parsedEngines);
		}
	}
}