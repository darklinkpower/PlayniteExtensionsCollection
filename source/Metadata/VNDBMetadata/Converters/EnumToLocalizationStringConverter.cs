using Playnite.SDK;
using PluginsCommon.Converters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Aggregates.ImageAggregate;
using VNDBMetadata.VndbDomain.Common.Enums;

namespace VNDBMetadata.Converters
{
    public class EnumToLocalizationStringConverter : EnumToStringConverter
    {
        static EnumToLocalizationStringConverter()
        {
            AddStringMapDictionary<SpoilerLevelEnum>(new Dictionary<Enum, string>
            {
                { SpoilerLevelEnum.None, ResourceProvider.GetString("LOC_VndbConnect_SpoilerLevelNone") },
                { SpoilerLevelEnum.Minimum, ResourceProvider.GetString("LOC_VndbConnect_SpoilerLevelMinimum") },
                { SpoilerLevelEnum.Major, ResourceProvider.GetString("LOC_VndbConnect_SpoilerLevelMajor") }
            });

            AddStringMapDictionary<ImageSexualityLevelEnum>(new Dictionary<Enum, string>
            {
                { ImageSexualityLevelEnum.Safe, ResourceProvider.GetString("LOC_VndbConnect_SexualityLevelSafe") },
                { ImageSexualityLevelEnum.Suggestive, ResourceProvider.GetString("LOC_VndbConnect_SexualityLevelSuggestive") },
                { ImageSexualityLevelEnum.Explicit, ResourceProvider.GetString("LOC_VndbConnect_SexualityLevelExplicit") }
            });

            AddStringMapDictionary<ImageViolenceLevelEnum>(new Dictionary<Enum, string>
            {
                { ImageViolenceLevelEnum.Tame, ResourceProvider.GetString("LOC_VndbConnect_ViolenceLevelTame") },
                { ImageViolenceLevelEnum.Violent, ResourceProvider.GetString("LOC_VndbConnect_ViolenceLevelViolent") },
                { ImageViolenceLevelEnum.Brutal, ResourceProvider.GetString("LOC_VndbConnect_ViolenceLevelBrutal") }
            });
        }
    }
}