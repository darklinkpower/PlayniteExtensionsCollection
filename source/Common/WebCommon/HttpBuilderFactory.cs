using System;
using WebCommon.Builders;

namespace WebCommon
{
    /// <summary>
    /// Provides a static class for configuring and initiating HTTP download operations.
    /// </summary>
    internal static class HttpBuilderFactory
    {
        private static readonly HttpClientFactory _httpClientFactory = new HttpClientFactory();

        /// <summary>
        /// Creates a new instance of an HTTP request builder pre-configured with default settings.
        /// Use this method to obtain a builder for configuring custom HTTP request clients.
        /// </summary>
        /// <returns>An instance of DownloadStringClientBuilder for configuring the settings of a DownloadStringClient.</returns>
        public static StringClientBuilder GetStringClientBuilder() => new StringClientBuilder(_httpClientFactory);

        /// <summary>
        /// Creates a new instance of an HTTP request builder pre-configured with default settings.
        /// Use this method to obtain a builder for configuring custom HTTP request clients.
        /// </summary>
        /// <returns>An instance of DownloadFileClientBuilder for configuring the settings of a DownloadFileClient.</returns>
        public static FileClientBuilder GetFileClientBuilder() => new FileClientBuilder(_httpClientFactory);
    }
}