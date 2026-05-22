using System;
using System.Threading;
using System.Threading.Tasks;
using GameEngineChecker.Interfaces;

namespace GameEngineChecker.Services
{
	public class RateLimiter : IRateLimiter
	{
		private readonly TimeSpan _rateLimitWindow;
		private readonly int _maxRequestsPerWindow;
		private readonly SemaphoreSlim _semaphore;
		private DateTime _firstExecutionInWindow = DateTime.MinValue;
		private int _totalExecutionInWindow;

		public RateLimiter(TimeSpan rateLimitWindow, int maxRequestsPerWindow)
		{
			_rateLimitWindow = rateLimitWindow;
			_maxRequestsPerWindow = maxRequestsPerWindow;
			_semaphore = new SemaphoreSlim(1, 1);
		}

		public async Task Limit(int batchSize, CancellationToken cancellationToken)
		{
			try
			{
				await _semaphore.WaitAsync(cancellationToken);

				if (batchSize > _maxRequestsPerWindow)
				{
					var delayMilliseconds = _rateLimitWindow.TotalMilliseconds / _maxRequestsPerWindow;
					await Task.Delay(TimeSpan.FromMilliseconds(delayMilliseconds), cancellationToken);
				}

				if (DateTime.UtcNow > ExecutionWindowEnd)
				{
					ResetWindow(DateTime.UtcNow);
				}

				if (_totalExecutionInWindow >= _maxRequestsPerWindow)
				{
					var timeLeftInWindow = ExecutionWindowEnd - DateTime.UtcNow;
					await Task.Delay(timeLeftInWindow, cancellationToken);

					ResetWindow(DateTime.UtcNow);
				}

				_totalExecutionInWindow++;
			}
			finally
			{
				_semaphore.Release();
			}
		}

		private DateTime ExecutionWindowEnd => _firstExecutionInWindow + _rateLimitWindow;

		private void ResetWindow(DateTime utcNow)
		{
			_firstExecutionInWindow = utcNow;
			_totalExecutionInWindow = 0;
		}
	}
}