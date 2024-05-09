using System;
using FlowHttp.Requests;

namespace FlowHttp
{
    /// <summary>
    /// Provides a static class for configuring and initiating HTTP requests.
    /// </summary>
    internal static class HttpRequestFactory
    {
        private static readonly HttpClientFactory _httpClientFactory = new HttpClientFactory();

        /// <summary>
        /// Creates a new instance of an HTTP request pre-configured with default settings.
        /// Use this method to obtain a builder for configuring custom HTTP requests.
        /// </summary>
        /// <returns>An instance of FlowHttpRequest for configuring the settings of an HTTP request.</returns>
        public static FlowHttpRequest GetHttpRequest() => new FlowHttpRequest(_httpClientFactory);

        /// <summary>
        /// Creates a new instance of an HTTP request pre-configured with default settings and a specific URL.
        /// Use this method to obtain a builder for configuring custom HTTP requests with a specific URL.
        /// </summary>
        /// <param name="url">The URL to be used in the HTTP request.</param>
        /// <returns>An instance of FlowHttpRequest for configuring the settings of an HTTP request.</returns>
        public static FlowHttpRequest GetHttpRequest(string url) => new FlowHttpRequest(_httpClientFactory).WithUrl(url);

        /// <summary>
        /// Creates a new instance of an HTTP request pre-configured with default settings.
        /// Use this method to obtain a builder for configuring custom HTTP requests for downloading files.
        /// </summary>
        /// <returns>An instance of FlowHttpFileRequest for configuring the settings of an HTTP request for file download.</returns>
        public static FlowHttpFileRequest GetHttpFileRequest() => new FlowHttpFileRequest(_httpClientFactory);

        /// <summary>
        /// Creates a new instance of an HTTP request pre-configured with default settings and a specific URL.
        /// Use this method to obtain a builder for configuring custom HTTP requests for downloading files with a specific URL.
        /// </summary>
        /// <param name="url">The URL to be used in the HTTP request for downloading files.</param>
        /// <returns>An instance of FlowHttpFileRequest for configuring the settings of an HTTP request for file download.</returns>
        public static FlowHttpFileRequest GetHttpFileRequest(string url) => new FlowHttpFileRequest(_httpClientFactory).WithUrl(url);

        /// <summary>
        /// Creates a new instance of an HTTP request pre-configured with default settings and a specific URL.
        /// Use this method to obtain a builder for configuring custom HTTP requests for downloading files with a specific URL.
        /// </summary>
        /// <param name="url">The URL to be used in the HTTP request for downloading files.</param>
        /// <param name="downloadPath">The location to download the file to.</param>
        /// <returns>An instance of FlowHttpFileRequest for configuring the settings of an HTTP request for file download.</returns>
        public static FlowHttpFileRequest GetHttpFileRequest(string url, string downloadPath) => new FlowHttpFileRequest(_httpClientFactory)
            .WithUrl(url)
            .WithDownloadTo(downloadPath);
    }
}