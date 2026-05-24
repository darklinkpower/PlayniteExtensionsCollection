using Playnite.SDK;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCriticMetadata.Domain.Entities;
using OpenCriticMetadata.Domain.Interfaces;

namespace OpenCriticMetadata
{
    public class OpenCriticMetadataProvider : OnDemandMetadataProvider
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private readonly MetadataRequestOptions _options;
        private readonly OpenCriticMetadata _plugin;
        private readonly IOpenCriticService _openCriticService;
        private readonly OpenCriticMetadataSettingsViewModel _settings;

        public override List<MetadataField> AvailableFields { get; } = new List<MetadataField>
        {
            MetadataField.CriticScore
        };

        public OpenCriticMetadataProvider(MetadataRequestOptions options, OpenCriticMetadata plugin, IOpenCriticService openCriticService, OpenCriticMetadataSettingsViewModel settings)
        {
            _options = options;
            _plugin = plugin;
            _openCriticService = openCriticService;
            _settings = settings;
        }

        public override int? GetCriticScore(GetMetadataFieldArgs args)
        {
            if (_options.IsBackgroundDownload)
            {
                var gameResults = Task.Run(
                    () => _openCriticService.GetGameSearchResultsAsync(_settings.Settings.ApiKey, _options.GameData.Name, args.CancelToken))
                    .GetAwaiter().GetResult();
                if (!gameResults.HasItems())
                {
                    return base.GetCriticScore(args);
                }

                var normalizedGameName = _options.GameData.Name.Satinize();
                var resultMatch = gameResults.FirstOrDefault(x => x.Name.Satinize() == normalizedGameName);
                if (resultMatch != null)
                {
                    var data = Task.Run(
                        () => _openCriticService.GetGameDataAsync(_settings.Settings.ApiKey, resultMatch, args.CancelToken))
                        .GetAwaiter().GetResult();
                    if (data != null)
                    {
                        return GetCriticScore(data);
                    }
                }
            }
            else
            {
                var selectedData = _plugin.PlayniteApi.Dialogs.ChooseItemWithSearch(
                    new List<GenericItemOption>(),
                    (a) => GetOpencriticSearchOptions(a),
                    _options.GameData.Name,
                    "Select game");

                if (selectedData != null)
                {
                    var data = Task.Run(
                        () => _openCriticService.GetGameDataAsync(_settings.Settings.ApiKey, selectedData.Description))
                        .GetAwaiter().GetResult();
                    if (data != null)
                    {
                        return GetCriticScore(data);
                    }
                }
            }
            
            return base.GetCriticScore(args);
        }

        private List<GenericItemOption> GetOpencriticSearchOptions(string gameName)
        {
            return Task.Run(() => _openCriticService.GetGameSearchResultsAsync(_settings.Settings.ApiKey, gameName))
                .GetAwaiter().GetResult()
                .Select(x => new GenericItemOption(x.Name, x.Id.ToString()))
                .ToList();
        }

        private int? GetCriticScore(OpenCriticGameData data)
        {
            if (data.TopCriticScore > 0)
            {
                return Convert.ToInt32(data.TopCriticScore);
            }
            else if (data.MedianScore > 0)
            {
                return Convert.ToInt32(data.MedianScore);
            }
            else
            {
                return null;
            }
        }
    }
}