using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThrottlerSharp
{
    public class RateLimiterBuilder
    {
        
        private int? _requestLimit;
        private TimeSpan? _timeWindow;
        private TimeSpan? _minTaskInterval;
        RateLimitMode _rateLimitMode = RateLimitMode.WaitForSlot;

        private RateLimiterBuilder()
        {

        }

        /// <summary>
        /// Creates a new instance of the <see cref="RateLimiterBuilder"/> class.
        /// </summary>
        /// <returns>A new instance of the <see cref="RateLimiterBuilder"/> class.</returns>
        public static RateLimiterBuilder Create() => new RateLimiterBuilder();

        /// <summary>
        /// Sets the request limit and time window for the rate limiter.
        /// </summary>
        /// <param name="requestLimit">The maximum number of requests allowed within the specified time window.</param>
        /// <param name="timeWindow">The time window for the request limit.</param>
        /// <returns>The <see cref="RateLimiterBuilder"/> instance.</returns>
        public RateLimiterBuilder WithRequestLimit(int requestLimit, TimeSpan timeWindow)
        {
            _requestLimit = requestLimit;
            _timeWindow = timeWindow;
            return this;
        }

        /// <summary>
        /// Sets the minimum interval between requests for the rate limiter.
        /// </summary>
        /// <param name="minTaskInterval">The minimum interval between requests.</param>
        /// <returns>The <see cref="RateLimiterBuilder"/> instance.</returns>
        public RateLimiterBuilder WithMinInterval(TimeSpan minTaskInterval)
        {
            _minTaskInterval = minTaskInterval;
            return this;
        }

        /// <summary>
        /// Sets the rate limiter to wait for a slot if the request limit is exceeded.
        /// </summary>
        /// <returns>The <see cref="RateLimiterBuilder"/> instance.</returns>
        public RateLimiterBuilder WithWaitForSlotMode()
        {
            _rateLimitMode = RateLimitMode.WaitForSlot;
            return this;
        }

        /// <summary>
        /// Sets the rate limiter to abort the request if the request limit is exceeded.
        /// </summary>
        /// <returns>The <see cref="RateLimiterBuilder"/> instance.</returns>
        public RateLimiterBuilder WithAbortMode()
        {
            _rateLimitMode = RateLimitMode.Abort;
            return this;
        }

        /// <summary>
        /// Builds a new instance of the <see cref="RateLimiter"/> class using the configured settings.
        /// </summary>
        /// <returns>A new instance of the <see cref="RateLimiter"/> class with the specified request limit, time window, minimum interval, and rate limit mode.</returns>
        public RateLimiter Build()
        {
            return new RateLimiter
            (
                _requestLimit,
                _timeWindow,
                _minTaskInterval,
                _rateLimitMode
            );
        }
        


    }



}