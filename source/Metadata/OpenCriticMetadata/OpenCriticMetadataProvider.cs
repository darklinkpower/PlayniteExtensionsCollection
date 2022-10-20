using OpenCriticMetadata.Models;
using OpenCriticMetadata.Services;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCommon;

namespace OpenCriticMetadata
{
    public class OpenCriticMetadataProvider : OnDemandMetadataProvider
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly MetadataRequestOptions options;
        private readonly OpenCriticMetadata plugin;
        private readonly OpenCriticService openCriticService;

        public override List<MetadataField> AvailableFields { get; } = new List<MetadataField>
        {
            MetadataField.CriticScore
        };

        public OpenCriticMetadataProvider(MetadataRequestOptions options, OpenCriticMetadata plugin, OpenCriticService openCriticService)
        {
            this.options = options;
            this.plugin = plugin;
            this.openCriticService = openCriticService;
        }

        public override int? GetCriticScore(GetMetadataFieldArgs args)
        {
            if (options.IsBackgroundDownload)
            {
                var gameResults = OpenCriticService.GetGameSearchResults(options.GameData.Name);
                if (!gameResults.HasItems())
                {
                    return base.GetCriticScore(args);
                }

                var resultMatch = gameResults.FirstOrDefault(x => x.Name.GetMatchModifiedName() == options.GameData.Name.GetMatchModifiedName());
                if (resultMatch != null)
                {
                    var data = openCriticService.GetGameData(resultMatch);
                    if (data != null)
                    {
                        return GetCriticScore(data);
                    }
                }
            }
            else
            {
                var selectedData = plugin.PlayniteApi.Dialogs.ChooseItemWithSearch(
                    new List<GenericItemOption>(),
                    (a) => GetOpencriticSearchOptions(a),
                    options.GameData.Name,
                    "Select game");

                if (selectedData != null)
                {
                    var data = openCriticService.GetGameData(selectedData.Description);
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
            return OpenCriticService.GetGameSearchResults(gameName)
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