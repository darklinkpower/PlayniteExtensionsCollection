using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowHttp;
using System.Windows.Threading;
using System.Threading;
using RateLimiter;
using ComposableAsync;
using System.Text.RegularExpressions;
using FlowHttp.Results;
using FlowHttp.Requests;
using FlowHttp.Constants;

namespace XboxMetadata.Services
{
    public class XboxWebService
    {
        private const string quickSearchTemplate = @"https://www.microsoft.com/msstoreapiprod/api/autosuggest?market={0}&sources=Microsoft-Terms%2CIris-Products%2CxSearch-Products&filter=%2BClientType%3AStoreWeb&counts={1}%2C1%2C5&query={2}";
        private const int quickSearchNumberOfResults = 5;

        private const string searchResultsUrlTemplate = @"https://emerald.xboxservices.com/xboxcomfd/search/{0}?locale={1}";
        private const string searchGameTypeParameter = "games";

        private readonly XboxMetadataSettingsViewModel _settingsViewModel;
        private readonly TimeLimiter _requestLimiter = TimeLimiter.GetFromMaxCountByInterval(1, TimeSpan.FromMilliseconds(500));

        public XboxWebService(XboxMetadataSettingsViewModel settingsViewModel)
        {
            _settingsViewModel = settingsViewModel;
        }

        public async Task<HttpContentResult<string>> ExecuteRequestAsync(FlowHttpRequest request)
        {
            await _requestLimiter;
            return request.DownloadString();
        }

        public HttpContentResult<string> ExecuteRequest(FlowHttpRequest request)
        {
            return Task.Run(async () => await ExecuteRequestAsync(request)).Result;
        }

        public List<Suggest> GetQuickGameSearchResults(string searchTerm)
        {
            return GetQuickSearchResults(searchTerm).Where(x => x.Source == "Game").ToList();
        }

        public List<Suggest> GetQuickSearchResults(string searchTerm)
        {
            var results = new List<Suggest>();
            var requestUrl = string.Format(quickSearchTemplate,
                _settingsViewModel.Settings.MarketLanguagePreference.GetStringValue(), quickSearchNumberOfResults, searchTerm.EscapeDataString());
            var request = HttpRequestFactory.GetFlowHttpRequest()
                .WithUrl(requestUrl);
            var result = ExecuteRequest(request);
            if (!result.IsSuccess)
            {
                return results;
            }

            var response = Serialization.FromJson<QuickSearchResult>(result.Content);
            foreach (var resultSet in response.ResultSets)
            {
                foreach (var item in resultSet.Suggests)
                {
                    item.Url = string.Concat("https:", item.Url);
                    item.ImageUrl = string.Concat("https:", item.ImageUrl);
                    results.Add(item);
                }
            }

            return results;
        }

        public List<ProductSummary> GetGameSearchResults(string searchTerm)
        {
            return GetSearchResults(searchTerm, searchGameTypeParameter);
        }

        private List<ProductSummary> GetSearchResults(string searchTerm, string searchType)
        {
            var requestUrl = string.Format(searchResultsUrlTemplate, searchType, _settingsViewModel.Settings.MarketLanguagePreference.GetStringValue());
            var headers = new Dictionary<string, string>
            {
                { "ms-cv", "abc/37.16" },
                { "x-ms-api-version", "1.1" }
            };

            var channelBodyValue = Regex.Replace(searchTerm.Replace(' ', '-'), @"[^a-zA-Z0-9\s]", string.Empty).ToUpper();
            var requestBody = new SearchRequestBody
            {
                Query = searchTerm,
                Filters = "e30=",
                ReturnFilters = false,
                ChannelKeyToBeUsedInResponse = $"SEARCH_GAMES_SEARCHQUERY={channelBodyValue}_"
            };

            var jsonBody = Serialization.ToJson(requestBody);
            var request = HttpRequestFactory.GetFlowHttpRequest()
                .WithUrl(requestUrl)
                .WithHeaders(headers)
                .WithPostHttpMethod()
                .WithContent(jsonBody, HttpContentTypes.Json);

            var results = new List<ProductSummary>();
            var result = ExecuteRequest(request);
            if (!result.IsSuccess)
            {
                return results;
            }

            var response = Serialization.FromJson<SearchResponse>(result.Content);
            results.AddRange(response.ProductSummaries);
            return results;
        }

    }
}