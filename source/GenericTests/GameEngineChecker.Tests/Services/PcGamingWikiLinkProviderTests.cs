using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using Playnite.SDK.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using GameEngineChecker.Services;
using Xunit;

namespace GameEngineChecker.Tests.Services
{
	public class PcGamingWikiLinkProviderTests
	{
		private readonly Fixture _fixture;
		private readonly PcGamingWikiLinkProvider _sut;

		public PcGamingWikiLinkProviderTests()
		{
			_fixture = new Fixture();
			_fixture.Customize(new AutoFakeItEasyCustomization());

			_sut = _fixture.Create<PcGamingWikiLinkProvider>();
		}

		[Fact]
		public async Task GetLink_UseGameId_WhenGameIsSteamGame()
		{
			// Arrange
			var game = _fixture.Create<Game>();
			game.PluginId = Guid.Parse("CB91DFC9-B977-43BF-8E70-55F46E410FAB");

			// Act
			var result = await _sut.GetLink(game, CancellationToken.None);

			// Assert
			Assert.Equal(new Uri($@"https://www.pcgamingwiki.com/w/api.php?action=cargoquery&format=json&tables=Infobox_game&fields=Engines,_pageName=title&where=Steam_AppID HOLDS ""{game.GameId}"""), result);
		}

		[Theory]
		[InlineData("AEBE8B7C-6DC3-4A66-AF31-E7375C6B5E9E")] // GOG
		[InlineData("03689811-3F33-4DFB-A121-2EE168FB9A5C")] // GOG OSS
		public async Task GetLink_UseGameId_WhenGameIsGogGame(string pluginId)
		{
			// Arrange
			var game = _fixture.Create<Game>();
			game.PluginId = Guid.Parse(pluginId);

			// Act
			var result = await _sut.GetLink(game, CancellationToken.None);

			// Assert
			Assert.Equal(new Uri($@"https://www.pcgamingwiki.com/w/api.php?action=cargoquery&format=json&tables=Infobox_game&fields=Engines,_pageName=title&where=GOGcom_ID HOLDS ""{game.GameId}"""), result);
		}

		[Theory]
		[InlineData("Steam", "https://store.steampowered.com/app/3634520/Samson")]
		[InlineData("LINK!", "store.steampowered.com/app/3634520")]
		public async Task GetLink_UseSteamLink_WhenGameIsNonSteamNonGogGameAndHasSteamLink(string name, string url)
		{
			// Arrange
			var game = _fixture.Create<Game>();
			game.Links.Add(new Link(name, url));

			// Act
			var result = await _sut.GetLink(game, CancellationToken.None);

			// Assert
			Assert.Equal(new Uri($@"https://www.pcgamingwiki.com/w/api.php?action=cargoquery&format=json&tables=Infobox_game&fields=Engines,_pageName=title&where=Steam_AppID HOLDS ""3634520"""), result);
		}

		[Theory]
		[InlineData("Wikipedia", "https://en.wikipedia.org/wiki/Need_for_Speed_III:_Hot_Pursuit")]
		[InlineData("LINK!", "wikipedia.org/wiki/Need_for_Speed_III:_Hot_Pursuit")]
		public async Task GetLink_UseWikipediaLink_WhenGameIsNonSteamNonGogGameAndHasWikipediaLink(string name, string url)
		{
			// Arrange
			var game = _fixture.Create<Game>();
			game.Links.Add(new Link(name, url));

			// Act
			var result = await _sut.GetLink(game, CancellationToken.None);

			// Assert
			Assert.Equal(new Uri($@"https://www.pcgamingwiki.com/w/api.php?action=cargoquery&format=json&tables=Infobox_game&fields=Engines,_pageName=title&where=Wikipedia=""Need for Speed III: Hot Pursuit"""), result);
		}

		[Fact]
		public async Task GetLink_ReturnNull_WhenGameIsNonSteamNonGogAndHasNoLinks()
		{
			// Arrange
			var game = _fixture.Create<Game>();
			game.Links = null;

			// Act
			var result = await _sut.GetLink(game, CancellationToken.None);

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public async Task GetLink_ReturnNull_WhenGameLinkNotGenerated()
		{
			// Arrange
			var game = _fixture.Create<Game>();

			// Act
			var result = await _sut.GetLink(game, CancellationToken.None);

			// Assert
			Assert.Null(result);
		}
	}
}