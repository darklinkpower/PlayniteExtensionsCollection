using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameEngineChecker.Services;
using Xunit;

namespace GameEngineChecker.Tests.Services
{
	public class TaggerTests
	{
		private readonly Fixture _fixture;
		private readonly IPlayniteAPI _api;
		private readonly Tagger _sut;
		private readonly TestableItemCollection<Tag> _tags;

		public TaggerTests()
		{
			_fixture = new Fixture();
			_fixture.Customize(new AutoFakeItEasyCustomization());

			_api = _fixture.Freeze<IPlayniteAPI>();
			_sut = _fixture.Create<Tagger>();

			_tags = new TestableItemCollection<Tag>(new List<Tag>());

			A.CallTo(() => _api.Database.Tags).Returns(_tags);
		}

		[Fact]
		public void AddEngineTags_AddsNewTagAndUpdatesTheGame_WhenTagDoesNotExist()
		{
			// Arrange
			var game = _fixture.Create<Game>();
			var engines = new List<string> { "Unity" };

			// Act
			_sut.AddEngineTags(game, engines, CancellationToken.None);

			// Assert
			var tag = Assert.Single(_api.Database.Tags);
			Assert.Equal("[Engine] Unity", tag.Name);
			Assert.Contains(tag.Id, game.TagIds);
		}

		[Fact]
		public void AddEngineTags_AddsNewTagAndUpdatesTheGame_WhenGameHasNoTags()
		{
			// Arrange
			var game = _fixture.Create<Game>();
			var engines = new List<string> { "Unity" };
			game.TagIds = null;

			// Act
			_sut.AddEngineTags(game, engines, CancellationToken.None);

			// Assert
			var tag = Assert.Single(_api.Database.Tags);
			Assert.Equal("[Engine] Unity", tag.Name);
			Assert.Contains(tag.Id, game.TagIds);
		}

		[Fact]
		public void AddEngineTags_DoesNotAddTheTagToTheGame_WhenGameHasTheTag()
		{
			// Arrange
			var game = _fixture.Create<Game>();
			var engines = new List<string> { "Unity" };
			var tagOnGame = _tags.Add("[Engine] Unity");
			game.TagIds.Add(tagOnGame.Id);

			// Act
			_sut.AddEngineTags(game, engines, CancellationToken.None);

			// Assert
			Assert.Single(game.TagIds.Where(x => x == tagOnGame.Id));
		}

		[Fact]
		public void AddEngineTags_PreventsRaceCondition_WhenProcessingInParallel()
		{
			// Arrange
			var game = _fixture.Create<Game>();
			var engines = new List<string> { "Unity" };

			// Act
			Parallel.For(0, 2, x => _sut.AddEngineTags(game, engines, CancellationToken.None));

			// Assert
			var tag = Assert.Single(_api.Database.Tags);
			Assert.Equal("[Engine] Unity", tag.Name);
			Assert.Contains(tag.Id, game.TagIds);
		}
	}
}