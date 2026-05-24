using GameEngineChecker.Interfaces;
using Playnite.SDK.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GameEngineChecker.Services
{
	public class PcGamingWikiLinkProvider : IPcGamingWikiLinkProvider
	{
		private const string UrlBase = "https://www.pcgamingwiki.com/w/api.php?action=cargoquery&format=json&tables=Infobox_game&fields=Engines,_pageName=title&where=";
		private readonly Regex _steamLinkRegex = new Regex(@"store\.steampowered\.com/app/(?<appId>\d+)", RegexOptions.Compiled);
		private readonly Regex _wikipediaLinkRegex = new Regex(@"wikipedia\.org/wiki/(?<pageName>[^/]+)", RegexOptions.Compiled);

		public Task<Uri> GetLink(Game game, CancellationToken cancellationToken)
		{
			if (game.PluginId == Guid.Parse("CB91DFC9-B977-43BF-8E70-55F46E410FAB")) // Steam
			{
				return Task.FromResult(GetSteamGameLink(game.GameId));
			}

			if (game.PluginId == Guid.Parse("AEBE8B7C-6DC3-4A66-AF31-E7375C6B5E9E") // GOG
				|| game.PluginId == Guid.Parse("03689811-3F33-4DFB-A121-2EE168FB9A5C")) // GOG OSS
			{
				return Task.FromResult(GetGogGameLink(game.GameId));
			}

			if (game.Links == null)
			{
				return Task.FromResult<Uri>(null);
			}

			foreach (var link in game.Links)
			{
				var steamMatch = _steamLinkRegex.Match(link.Url);
				if (steamMatch.Success)
				{
					var gameId = steamMatch.Groups["appId"].Value;
					return Task.FromResult(GetSteamGameLink(gameId));
				}

				var wikipediaMatch = _wikipediaLinkRegex.Match(link.Url);
				if (wikipediaMatch.Success)
				{
					var pageName = wikipediaMatch.Groups["pageName"].Value;
					return Task.FromResult(GetWikipediaGameLink(pageName));
				}
			}

			return Task.FromResult<Uri>(null);
		}

		private static Uri GetSteamGameLink(string gameId)
		{
			return new Uri($@"{UrlBase}Steam_AppID HOLDS ""{gameId}""");
		}

		private static Uri GetGogGameLink(string gameId)
		{
			return new Uri($@"{UrlBase}GOGcom_ID HOLDS ""{gameId}""");
		}

		private static Uri GetWikipediaGameLink(string pageName)
		{
			var pcWikiPageName = pageName.Replace('_', ' ');
			return new Uri($@"{UrlBase}Wikipedia=""{pcWikiPageName}""");
		}
	}
}