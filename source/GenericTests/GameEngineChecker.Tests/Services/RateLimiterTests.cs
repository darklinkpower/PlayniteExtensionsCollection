using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GameEngineChecker.Services;
using Xunit;

namespace GameEngineChecker.Tests.Services
{
	public class RateLimiterTests
	{
		[Fact]
		public async Task Limit_Bursts_WhenBatchIsSmallerThanLimitPerWindow()
		{
			// Arrange
			var expected = 19;
			var actual = 0;
			var cts = new CancellationTokenSource(200);

			// Act
			_ = await Record.ExceptionAsync(() => Task.Run(async () =>
			{
				var sut = new RateLimiter(TimeSpan.FromMilliseconds(250), expected);
				while (true)
				{
					await sut.Limit(expected, cts.Token);
					actual++;
				}
			}, cts.Token));

			// Assert

			Assert.Equal(expected, actual);
		}

		[Fact]
		public async Task Limit_Steady_WhenBatchIsHigherThanLimitPerWindow()
		{
			// Arrange
			var batchSize = 4;
			var maxRequestsPerWindow = 2;
			var windowMilliseconds = 50;

			var cts = new CancellationTokenSource(500);

			// Act
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			await Task.Run(async () =>
			{
				var sut = new RateLimiter(TimeSpan.FromMilliseconds(windowMilliseconds), maxRequestsPerWindow);
				for (var i = 0; i < batchSize; i++)
				{
					await sut.Limit(batchSize, cts.Token);
				}
			}, cts.Token);
			stopwatch.Stop();

			// Assert
			Assert.True(stopwatch.ElapsedMilliseconds > windowMilliseconds * batchSize / maxRequestsPerWindow);
		}
	}
}