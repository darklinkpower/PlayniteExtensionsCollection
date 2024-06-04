using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThrottlerSharp
{
    /// <summary>
    /// Provides functionality to limit the rate of asynchronous operations, ensuring that
    /// the number of operations does not exceed a specified limit within a given time window,
    /// and optionally enforcing a minimum interval between consecutive operations.
    /// </summary>
    /// <remarks>
    /// The <see cref="RateLimiter"/> class is designed to help manage the rate of API requests
    /// or other operations that have rate limiting constraints. It supports two modes:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <see cref="RateLimitMode.WaitForSlot"/>: When the rate limit is reached, the operations will be delayed
    /// until a slot is available.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <see cref="RateLimitMode.Abort"/>: When the rate limit is reached, the operations will be aborted and will return
    /// the default value of the result type.
    /// </description>
    /// </item>
    /// </list>
    /// The <see cref="RateLimiter"/> can be configured using fluent methods to set the request limit, time window,
    /// minimum interval between requests, and the rate limiting mode with the <see cref="RateLimiterBuilder"/> builder.
    /// </remarks>
    public class RateLimiter
    {
        private int? _requestLimit;
        private TimeSpan? _timeWindow;
        private TimeSpan? _minTaskInterval;
        private RateLimitMode _rateLimitMode;
        private Queue<DateTime> _requestTimestamps = new Queue<DateTime>();
        private readonly ILogger _logger = LogManager.GetLogger();

        public RateLimiter(int? requestLimit, TimeSpan? timeWindow, TimeSpan? minTaskInterval, RateLimitMode rateLimitMode)
        {
            _requestLimit = requestLimit;
            _timeWindow = timeWindow;
            _minTaskInterval = minTaskInterval;
            _rateLimitMode = rateLimitMode;
        }

        /// <summary>
        /// Perform a task asynchronously with rate limiting.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the task.</typeparam>
        /// <param name="function">The asynchronous function representing the task to perform.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>An <see cref="OperationResult{TResult}"/> object indicating whether the task was performed successfully and containing the result of the task, 
        /// or default if the rate limit was exceeded and the mode is Abort.</returns>
        public async Task<OperationResult<TResult>> ExecuteAsync<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken = default)
        {            
            var now = DateTime.UtcNow;
            TimeSpan delayForNextRequest;
            DateTime futureTimestamp;
            lock (_requestTimestamps)
            {
                CleanupOldRequests(now);
                if (_requestLimit.HasValue && _requestTimestamps.Count >= _requestLimit.Value)
                {
                    if (_rateLimitMode == RateLimitMode.Abort)
                    {
                        _logger.Warn("Rate limit exceeded and mode set to Abort.");
                        return new OperationResult<TResult>(false, default);
                    }

                    var oldestRequestTime = _requestTimestamps.Peek();
                    var timeUntilSlotFreed = _timeWindow.Value - (now - oldestRequestTime);
                    var nextRequestInterval = GetDelayForNextRequest(now);
                    if (timeUntilSlotFreed > nextRequestInterval)
                    {
                        delayForNextRequest = timeUntilSlotFreed;
                    }
                    else
                    {
                        delayForNextRequest = nextRequestInterval;
                    }
                }
                else
                {
                    delayForNextRequest = GetDelayForNextRequest(now);
                }

                futureTimestamp = now + delayForNextRequest;
                _requestTimestamps.Enqueue(futureTimestamp);
            }

            if (delayForNextRequest > TimeSpan.Zero)
            {
                try
                {
                    _logger.Info($"Waiting for a slot to be freed. Delaying task for {delayForNextRequest.TotalMilliseconds}ms.");
                    await Task.Delay(delayForNextRequest, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.Info("Task was canceled.");

                    // Remove the reserved spot if the task is canceled
                    lock (_requestTimestamps)
                    {
                        if (_requestTimestamps.Contains(futureTimestamp))
                        {
                            _requestTimestamps = new Queue<DateTime>(_requestTimestamps.Where(t => t != futureTimestamp));
                        }
                    }

                    return new OperationResult<TResult>(false, default);
                }
            }

            var result = await function();
            return new OperationResult<TResult>(true, result);
        }

        private void CleanupOldRequests(DateTime now)
        {
            while (_requestTimestamps.Count > 0 && (now - _requestTimestamps.Peek()) > _timeWindow)
            {
                _requestTimestamps.Dequeue();
            }
        }

        private TimeSpan GetDelayForNextRequest(DateTime now)
        {
            if (_requestTimestamps.Count == 0 || !_minTaskInterval.HasValue)
            {
                return TimeSpan.Zero;
            }

            var lastRequestTime = _requestTimestamps.Last();
            var timeSinceLastRequest = now.Subtract(lastRequestTime);
            if (_minTaskInterval.Value > timeSinceLastRequest)
            {
                return _minTaskInterval.Value - timeSinceLastRequest;
            }

            var intervalMilliseconds = _minTaskInterval.Value.TotalMilliseconds;
            var intervalLastRequestMilliseconds = (_minTaskInterval.Value - timeSinceLastRequest).TotalMilliseconds;

            return TimeSpan.Zero;
        }
    }
}