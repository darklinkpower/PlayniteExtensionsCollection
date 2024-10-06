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

        public override List<MetadataField> AvailableFields { get; } = new List<MetadataField>
        {
            MetadataField.CriticScore
        };

        public OpenCriticMetadataProvider(MetadataRequestOptions options, OpenCriticMetadata plugin, IOpenCriticService openCriticService)
        {
            _options = options;
            _plugin = plugin;
            _openCriticService = openCriticService;
        }

        public override int? GetCriticScore(GetMetadataFieldArgs args)
        {
            if (_options.IsBackgroundDownload)
            {
                var gameResults = Task.Run(
                    () => _openCriticService.GetGameSearchResultsAsync(_options.GameData.Name, args.CancelToken))
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
                        () => _openCriticService.GetGameDataAsync(resultMatch, args.CancelToken))
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
                        () => _openCriticService.GetGameDataAsync(selectedData.Description))
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
            return Task.Run(() => _openCriticService.GetGameSearchResultsAsync(gameName))
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