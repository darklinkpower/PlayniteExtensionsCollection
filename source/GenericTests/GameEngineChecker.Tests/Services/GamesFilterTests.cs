using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using GameEngineChecker.Services;
using Xunit;

namespace GameEngineChecker.Tests.Services
{
	public class GamesFilterTests
	{
		private readonly Fixture _fixture;
		private readonly GamesFilter _sut;
		private readonly Tag _engineTag;

		public GamesFilterTests()
		{
			_fixture = new Fixture();
			_fixture.Customize(new AutoFakeItEasyCustomization());

			var api = _fixture.Freeze<IPlayniteAPI>();

			var tags = new TestableItemCollection<Tag>(new List<Tag>());

			_engineTag = _fixture.Create<Tag>();
			_engineTag.Name = "[Engine] Unity";
			tags.Add(_engineTag);

			A.CallTo(() => api.Database.Tags).Returns(tags);

			_sut = _fixture.Create<GamesFilter>();
		}

		[Fact]
		public void ShouldTheGameBeProcessed_ReturnsTrue_WhenGameHasNoEngineTags()
		{
			// Arrange
			var game = _fixture.Create<Game>();

			// Act
			var result = _sut.ShouldTheGameBeProcessed(game);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void ShouldTheGameBeProcessed_ReturnsTrue_WhenGameHasNoTags()
		{
			// Arrange
			var game = _fixture.Create<Game>();
			game.TagIds = null;

			// Act
			var result = _sut.ShouldTheGameBeProcessed(game);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void ShouldTheGameBeProcessed_ReturnsFalse_WhenGameHasAnyEngineTag()
		{
			// Arrange
			var game = _fixture.Create<Game>();
			game.TagIds.Add(_engineTag.Id);

			// Act
			var result = _sut.ShouldTheGameBeProcessed(game);

			// Assert
			Assert.False(result);
		}
	}
}