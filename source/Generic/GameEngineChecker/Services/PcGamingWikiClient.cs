using GameEngineChecker.Models.PcGamingWiki;
using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GameEngineChecker.Interfaces;

namespace GameEngineChecker.Services
{
	public class PcGamingWikiClient : IPcGamingWikiClient, IDisposable
	{
		private readonly ILogger _logger = LogManager.GetLogger();
		private readonly HttpClient _httpClient;

		public PcGamingWikiClient()
		{
			_httpClient = new HttpClient();
		}

		public async Task<string> GetEngines(Uri link, CancellationToken cancellationToken)
		{
			try
			{
				var request = new HttpRequestMessage(HttpMethod.Get, link);
				request.Headers.TryAddWithoutValidation("User-Agent", "Playnite.GameEngineChecker Extension 3.0 (https://github.com/SparrowBrain/)");

				var response = await _httpClient.SendAsync(request, cancellationToken);
				var responseString = await response.Content.ReadAsStringAsync();
				_logger.Debug($"Response from PC Gaming Wiki: Status: {response.StatusCode}; Body {responseString}");

				response.EnsureSuccessStatusCode();
				var parsedResponse = ParseResponse(responseString);
				var engines = parsedResponse?.CargoQuery?.FirstOrDefault()?.Title?.Engines;
				if (engines == null)
				{
					_logger.Warn($"No engines found in response: {responseString}");
				}

				return engines;
			}
			catch (Exception ex)
			{
				if (!(ex is OperationCanceledException))
				{
					_logger.Error(ex, $"Error while getting engines via {link}");
				}

				return null;
			}
		}

		public void Dispose()
		{
			_httpClient.Dispose();
		}

		private PcGamingWikiEngineResponse ParseResponse(string responseContent)
		{
			var importResponse = JsonConvert.DeserializeObject<PcGamingWikiEngineResponse>(responseContent);
			return importResponse;
		}
	}
}