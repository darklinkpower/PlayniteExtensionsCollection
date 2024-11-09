
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

namespace MetacriticMetadata
{
    public class MetacriticMetadataProvider : OnDemandMetadataProvider
    {
        private readonly MetadataRequestOptions _options;
        private readonly IPlayniteAPI _playniteApi;
        private readonly IMetacriticService _metacriticService;
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
            MetacriticMetadataSettingsViewModel settings)
        {
            _options = options;
            _playniteApi = playniteApi;
            _metacriticService = metacriticService;
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
                    (a) => Task.Run(() => GetMetacriticSearchOptions(a)).GetAwaiter().GetResult(),
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
                    x.Url + "\n\n" + x.Description))
                .ToList();
        }

        private static string GenerateItemOptionName(MetacriticSearchResult result)
        {
            var name = $"{result.Name} ({string.Join(", ", result.Platforms)} - {result.ReleaseDate}";
            if (result.MetacriticScore.HasValue)
            {
                name += $" - {result.MetacriticScore.Value}";
            }

            name += ")";
            return name;
        }

        private async Task<List<GenericItemOption>> GetMetacriticSearchOptions(string gameName)
        {
            if (!_firstSearchMade) //First search makes use of game platform
            {
                _currentResults = await _metacriticService.GetGameSearchResultsAsync(
                    _options.GameData,
                    _settingsViewModel.Settings.ApiKey);
                _firstSearchMade = true;
            }
            else
            {
                _currentResults = await _metacriticService.GetGameSearchResultsAsync(
                    gameName,
                    _settingsViewModel.Settings.ApiKey);
            }

            return GetItemOptionListFromResults(_currentResults);
        }
    }
}