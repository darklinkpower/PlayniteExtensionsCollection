
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetacriticMetadata.Domain.Entities;
using MetacriticMetadata.Domain.Interfaces;
using System.Threading;

namespace MetacriticMetadata
{
    public class MetacriticMetadataProvider : OnDemandMetadataProvider
    {
        private readonly MetadataRequestOptions _options;
        private readonly IPlayniteAPI _playniteApi;
        private readonly IMetacriticService _metacriticService;
        private readonly ILogger _logger;
        private readonly MetacriticMetadataSettingsViewModel _settingsViewModel;
        private List<MetacriticSearchResult> _currentResults = new List<MetacriticSearchResult>();
        private bool _firstSearchMade = false;

        public override List<MetadataField> AvailableFields { get; } = new List<MetadataField>
        {
            MetadataField.CriticScore
        };

        public MetacriticMetadataProvider(
            MetadataRequestOptions options,
            IMetacriticService metacriticService,
            IPlayniteAPI playniteApi,
            ILogger logger,
            MetacriticMetadataSettingsViewModel settings)
        {
            _options = options;
            _playniteApi = playniteApi;
            _metacriticService = metacriticService;
            _logger = logger;
            _settingsViewModel = settings;
        }

        public override int? GetCriticScore(GetMetadataFieldArgs args)
        {
            if (_options.IsBackgroundDownload)
            {
                var gameResults = Task.Run(() => _metacriticService.GetGameSearchResultsAsync(
                    _options.GameData,
                    _settingsViewModel.Settings.ApiKey,
                    args.CancelToken))
                    .GetAwaiter().GetResult();
                if (!gameResults.HasItems())
                {
                    return base.GetCriticScore(args);
                }

                var normalizedGameName = _options.GameData.Name.Satinize();
                var resultMatch = gameResults.FirstOrDefault(x => x.Name.Satinize() == normalizedGameName);
                if (resultMatch is null || resultMatch.MetacriticScore is null)
                {
                    return base.GetCriticScore(args);
                }

                return resultMatch.MetacriticScore.GetValueOrDefault();
            }
            else
            {
                var selectedData = _playniteApi.Dialogs.ChooseItemWithSearch(
                    null,
                    (a) => GetMetacriticSearchOptions(a, args.CancelToken),
                    _options.GameData.Name,
                    "Select game");

                if (selectedData != null)
                {
                    // The original item is retrieved from the search. This is because GenericItems don't allow setting arbitrary data
                    var selectedResult = _currentResults?.FirstOrDefault(x => selectedData.Description.StartsWith(x.Url));
                    if (selectedResult?.MetacriticScore.HasValue == true)
                    {
                        return selectedResult.MetacriticScore;
                    }
                }
            }

            return base.GetCriticScore(args);
        }

        private static List<GenericItemOption> GetItemOptionListFromResults(List<MetacriticSearchResult> list)
        {
            return list
                .Select(x => new GenericItemOption(
                    GenerateItemOptionName(x),
                    $"{x.Url}\n\n{x.Description}"))
                .ToList();
        }

        private static string GenerateItemOptionName(MetacriticSearchResult result)
        {
            var platforms = string.Join(", ", result.Platforms);
            var releaseDate = result.ReleaseDate;
            var metacriticScore = result.MetacriticScore.HasValue ? $" - {result.MetacriticScore.Value}" : string.Empty;

            return $"{result.Name} ({platforms} - {releaseDate}{metacriticScore})";
        }

        private List<GenericItemOption> GetMetacriticSearchOptions(string gameName, CancellationToken cancelToken)
        {
            try
            {
                if (_firstSearchMade)
                {
                    var results = GetMetacriticSearchOptionsAsync(gameName, true, cancelToken).GetAwaiter().GetResult();
                    _currentResults = results;
                    _firstSearchMade = true;
                    return GetItemOptionListFromResults(_currentResults);
                }
                else
                {
                    var results = GetMetacriticSearchOptionsAsync(gameName, false, cancelToken).GetAwaiter().GetResult();
                    _currentResults = results;
                    return GetItemOptionListFromResults(_currentResults);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get Metacritic search options.");
                throw;
            }
        }

        private async Task<List<MetacriticSearchResult>> GetMetacriticSearchOptionsAsync(string gameName, bool useGameData, CancellationToken cancelToken)
        {
            if (useGameData)
            {
                return await _metacriticService.GetGameSearchResultsAsync(
                    _options.GameData,
                    _settingsViewModel.Settings.ApiKey,
                    cancelToken);
            }
            else
            {
                return await _metacriticService.GetGameSearchResultsAsync(
                    gameName,
                    _settingsViewModel.Settings.ApiKey,
                    cancelToken);
            }
        }

    }
}