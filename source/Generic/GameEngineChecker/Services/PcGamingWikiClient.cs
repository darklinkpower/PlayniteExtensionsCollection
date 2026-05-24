using GameEngineChecker.Interfaces;
using GameEngineChecker.Models.PcGamingWiki;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GameEngineChecker.Services
{
	public class PcGamingWikiClient : IPcGamingWikiClient, IDisposable
	{
		private readonly IPlayniteAPI _api;
		private readonly ILogger _logger = LogManager.GetLogger();
		private readonly HttpClient _httpClient;

		public PcGamingWikiClient(IPlayniteAPI api)
		{
			_api = api;
			_httpClient = new HttpClient();
		}

		public async Task<string> GetEngines(Uri link, Game game, CancellationToken cancellationToken)
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
				
				var allFoundEntriesEngines = parsedResponse?
					                             .CargoQuery?
					                             .Where(x => x.Title?.Engines != null)
					                             .Select(x => x.Title?.Engines)
					                             .ToList()
				                             ?? new List<string>();
				var engines = string.Join(",", allFoundEntriesEngines);
				if (string.IsNullOrEmpty(engines))
				{
					_logger.Info($"No engines found in response: {responseString}");
				}

				return engines;
			}
			catch (Exception ex)
			{
				if (!(ex is OperationCanceledException))
				{
					_logger.Error(ex, $"Error while getting engines via {link}");
					_api.Notifications.Add("game_engine_checker__pcgw_error_message",
						string.Format(
							ResourceProvider.GetString("LOCGame_Engine_Checker_PcgwDownloadErrorMessage"),
							game.Name,
							ex.Message),
						NotificationType.Error);
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