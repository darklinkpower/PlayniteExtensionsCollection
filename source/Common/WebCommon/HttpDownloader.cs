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
        /// Initializes a new HTTP download operation with default settings.
        /// Use this method to configure and initiate a download operation.
        /// </summary>
        /// <returns>An instance of the HttpRequestBuilder for configuring the download operation.</returns>
        public static HttpRequestBuilder GetRequestBuilder() => new HttpRequestBuilder(_httpClientFactory);
    }
}