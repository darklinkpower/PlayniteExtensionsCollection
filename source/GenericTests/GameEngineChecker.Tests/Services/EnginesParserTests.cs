using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using GameEngineChecker.Services;
using Xunit;

namespace GameEngineChecker.Tests.Services
{
	public class EnginesParserTests
	{
		private readonly EnginesParser _sut;

		public EnginesParserTests()
		{
			var fixture = new Fixture();
			fixture.Customize(new AutoFakeItEasyCustomization());

			_sut = fixture.Create<EnginesParser>();
		}

		[Fact]
		public void Parse_ReturnsSingleEngine_WhenSingleEngine()
		{
			// Arrange
			var engines = "Engine:Unity";

			// Act
			var parsedEngines = _sut.Parse(engines);

			// Assert
			var engine = Assert.Single(parsedEngines);
			Assert.Equal("Unity", engine);
		}

		[Fact]
		public void Parse_ReturnsTwoEngines_WhenTwoDifferentEngines()
		{
			// Arrange
			var engines = "Engine:Unreal Engine 5,Engine:Gamebryo (TES Engine)";

			// Act
			var parsedEngines = _sut.Parse(engines);

			// Assert
			Assert.Equal(2, parsedEngines.Count);
			Assert.Contains("Unreal Engine 5", parsedEngines);
			Assert.Contains("Gamebryo (TES Engine)", parsedEngines);
		}

		[Fact]
		public void Parse_ReturnsSingleEngine_WhenSeveralIdenticalEngines()
		{
			// Arrange
			var engines = "Engine:Unity,Engine:Unity,Engine:Unity";

			// Act
			var parsedEngines = _sut.Parse(engines);

			// Assert
			var engine = Assert.Single(parsedEngines);
			Assert.Equal("Unity", engine);
		}
	}
}