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
using VNDBFuze.Enums;

namespace VNDBFuze.Converters
{
    public class EnumToLocalizationStringConverter : EnumToStringConverter
    {
        static EnumToLocalizationStringConverter()
        {
            AddStringMapDictionary<SpoilerLevelEnum>(new Dictionary<Enum, string>
            {
                { SpoilerLevelEnum.None, ResourceProvider.GetString("LOC_VndbFuze_SpoilerLevelNone") },
                { SpoilerLevelEnum.Minimum, ResourceProvider.GetString("LOC_VndbFuze_SpoilerLevelMinimum") },
                { SpoilerLevelEnum.Major, ResourceProvider.GetString("LOC_VndbFuze_SpoilerLevelMajor") }
            });

            AddStringMapDictionary<ImageSexualityLevelEnum>(new Dictionary<Enum, string>
            {
                { ImageSexualityLevelEnum.Safe, ResourceProvider.GetString("LOC_VndbFuze_SexualityLevelSafe") },
                { ImageSexualityLevelEnum.Suggestive, ResourceProvider.GetString("LOC_VndbFuze_SexualityLevelSuggestive") },
                { ImageSexualityLevelEnum.Explicit, ResourceProvider.GetString("LOC_VndbFuze_SexualityLevelExplicit") }
            });

            AddStringMapDictionary<ImageViolenceLevelEnum>(new Dictionary<Enum, string>
            {
                { ImageViolenceLevelEnum.Tame, ResourceProvider.GetString("LOC_VndbFuze_ViolenceLevelTame") },
                { ImageViolenceLevelEnum.Violent, ResourceProvider.GetString("LOC_VndbFuze_ViolenceLevelViolent") },
                { ImageViolenceLevelEnum.Brutal, ResourceProvider.GetString("LOC_VndbFuze_ViolenceLevelBrutal") }
            });

            AddStringMapDictionary<TagsDisplayOptionEnum>(new Dictionary<Enum, string>
            {
                { TagsDisplayOptionEnum.All, ResourceProvider.GetString("LOC_VndbFuze_TagsDisplayOptionEnumAll") },
                { TagsDisplayOptionEnum.Summary, ResourceProvider.GetString("LOC_VndbFuze_TagsDisplayOptionSummary") }
            });
        }


    }


}