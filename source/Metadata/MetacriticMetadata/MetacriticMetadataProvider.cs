using MetacriticMetadata.Services;
using MetacriticMetadata.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetacriticMetadata
{
    public class MetacriticMetadataProvider : OnDemandMetadataProvider
    {
        private readonly MetadataRequestOptions options;
        private readonly MetacriticMetadata plugin;
        private readonly MetacriticService metacriticService;
        private List<MetacriticSearchResult> currentResults = new List<MetacriticSearchResult>();
        private bool firstSearchMade = false;

        public override List<MetadataField> AvailableFields { get; } = new List<MetadataField>
        {
            MetadataField.CriticScore
        };

        public MetacriticMetadataProvider(MetadataRequestOptions options, MetacriticMetadata plugin, MetacriticService metacriticService)
        {
            this.options = options;
            this.plugin = plugin;
            this.metacriticService = metacriticService;
        }

        public override int? GetCriticScore(GetMetadataFieldArgs args)
        {
            if (options.IsBackgroundDownload)
            {
                var gameResults = metacriticService.GetGameSearchResults(options.GameData, args.CancelToken);
                if (!gameResults.HasItems())
                {
                    return base.GetCriticScore(args);
                }

                var normalizedGameName = options.GameData.Name.Satinize();
                var resultMatch = gameResults.FirstOrDefault(x => x.Name.Satinize() == normalizedGameName);
                if (resultMatch is null || resultMatch.MetacriticScore is null)
                {
                    return base.GetCriticScore(args);
                }

                return resultMatch.MetacriticScore.GetValueOrDefault();
            }
            else
            {
                var selectedData = plugin.PlayniteApi.Dialogs.ChooseItemWithSearch(
                    null,
                    (a) => GetOpencriticSearchOptions(a),
                    options.GameData.Name,
                    "Select game");

                if (selectedData != null)
                {
                    // The original item is retrieved from the search. This is because GenericItems don't allow setting arbitrary data
                    var selectedResult = currentResults?.FirstOrDefault(x => selectedData.Description.StartsWith(x.Url));
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

        private List<GenericItemOption> GetOpencriticSearchOptions(string gameName)
        {
            if (!firstSearchMade) //First search makes use of game platform
            {
                currentResults = metacriticService.GetGameSearchResults(options.GameData);
                firstSearchMade = true;
            }
            else
            {
                currentResults = metacriticService.GetGameSearchResults(gameName);
            }

            return GetItemOptionListFromResults(currentResults);
        }
    }
}