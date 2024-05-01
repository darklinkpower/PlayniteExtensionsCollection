using System;

namespace WebCommon
{
    /// <summary>
    /// Provides a static class for configuring and initiating HTTP download operations.
    /// </summary>
    internal static class HttpDownloader
    {
        private static readonly HttpClientFactory _httpClientFactory = new HttpClientFactory();

        /// <summary>
        /// Creates a new instance of an HTTP request builder pre-configured with default settings.
        /// Use this method to obtain a builder for configuring custom HTTP request clients.
        /// </summary>
        /// <returns>An instance of HttpRequestBuilder for configuring the settings of an HTTP request client.</returns>
        public static HttpRequestBuilder GetRequestBuilder() => new HttpRequestBuilder(_httpClientFactory);
    }
}