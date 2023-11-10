using Microsoft.Extensions.DependencyInjection;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebCommon
{
    /// <summary>
    /// Factory for creating and managing instances of HttpClient for HTTP requests.
    /// </summary>
    internal class HttpClientFactory
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private const string _clientForCookiesName = "ClientForCookiesUse";
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the HttpClientFactory class and configures HttpClient instances.
        /// </summary>
        internal HttpClientFactory()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddHttpClient();

            // By default, the HttpClients created via HttpClientFactory use
            // handlers with UseCookies set as true. To support adding
            // cookies via headers, this configuration disables cookies in the request handlers.
            serviceCollection.AddHttpClient(_clientForCookiesName).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler()
                {
                    UseCookies = false
                };
            });

            var serviceProvider = serviceCollection.BuildServiceProvider();
            _httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            _logger.Debug("Created service provider with IHttpClientFactory");
        }

        /// <summary>
        /// Gets an HttpClient instance for the given HttpRequestMessage.
        /// If the request contains the "Cookie" header, it uses a specialized HttpClient.
        /// </summary>
        /// <param name="httpRequestMessage">The HttpRequestMessage for which an HttpClient is requested.</param>
        /// <returns>An HttpClient instance for the specified request.</returns>
        public HttpClient GetClient(HttpRequestMessage httpRequestMessage)
        {
            if (httpRequestMessage.Headers.Contains("Cookie"))
            {
                return _httpClientFactory.CreateClient(_clientForCookiesName);
            }
            else
            {
                return _httpClientFactory.CreateClient(httpRequestMessage.RequestUri.Host);
            }
        }
    }
}