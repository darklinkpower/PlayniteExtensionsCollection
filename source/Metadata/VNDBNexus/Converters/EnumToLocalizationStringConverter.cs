using Playnite.SDK;
using PluginsCommon.Converters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.ImageAggregate;
using VndbApiDomain.SharedKernel;
using VNDBNexus.Enums;

namespace VNDBNexus.Converters
{
    public class EnumToLocalizationStringConverter : EnumToStringConverter
    {
        static EnumToLocalizationStringConverter()
        {
            AddStringMapDictionary<SpoilerLevelEnum>(new Dictionary<Enum, string>
            {
                { SpoilerLevelEnum.None, ResourceProvider.GetString("LOC_VndbNexus_SpoilerLevelNone") },
                { SpoilerLevelEnum.Minimum, ResourceProvider.GetString("LOC_VndbNexus_SpoilerLevelMinimum") },
                { SpoilerLevelEnum.Major, ResourceProvider.GetString("LOC_VndbNexus_SpoilerLevelMajor") }
            });

            AddStringMapDictionary<ImageSexualityLevelEnum>(new Dictionary<Enum, string>
            {
                { ImageSexualityLevelEnum.Safe, ResourceProvider.GetString("LOC_VndbNexus_SexualityLevelSafe") },
                { ImageSexualityLevelEnum.Suggestive, ResourceProvider.GetString("LOC_VndbNexus_SexualityLevelSuggestive") },
                { ImageSexualityLevelEnum.Explicit, ResourceProvider.GetString("LOC_VndbNexus_SexualityLevelExplicit") }
            });

            AddStringMapDictionary<ImageViolenceLevelEnum>(new Dictionary<Enum, string>
            {
                { ImageViolenceLevelEnum.Tame, ResourceProvider.GetString("LOC_VndbNexus_ViolenceLevelTame") },
                { ImageViolenceLevelEnum.Violent, ResourceProvider.GetString("LOC_VndbNexus_ViolenceLevelViolent") },
                { ImageViolenceLevelEnum.Brutal, ResourceProvider.GetString("LOC_VndbNexus_ViolenceLevelBrutal") }
            });

            AddStringMapDictionary<TagsDisplayOptionEnum>(new Dictionary<Enum, string>
            {
                { TagsDisplayOptionEnum.All, ResourceProvider.GetString("LOC_VndbNexus_TagsDisplayOptionEnumAll") },
                { TagsDisplayOptionEnum.Summary, ResourceProvider.GetString("LOC_VndbNexus_TagsDisplayOptionSummary") }
            });
        }


    }


}