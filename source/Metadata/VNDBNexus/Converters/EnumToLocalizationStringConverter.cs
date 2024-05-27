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
using VndbApiDomain.VisualNovelAggregate;
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

            AddStringMapDictionary<VnRelationTypeEnum>(new Dictionary<Enum, string>
            {
                { VnRelationTypeEnum.AlternativeVersion, ResourceProvider.GetString("LOC_VndbNexus_RelationTypeAlternativeVersion") },
                { VnRelationTypeEnum.SharesCharacters, ResourceProvider.GetString("LOC_VndbNexus_RelationTypeSharesCharacters") },
                { VnRelationTypeEnum.Fandisc, ResourceProvider.GetString("LOC_VndbNexus_RelationTypeFandisc") },
                { VnRelationTypeEnum.OriginalGame, ResourceProvider.GetString("LOC_VndbNexus_RelationTypeOriginalGame") },
                { VnRelationTypeEnum.ParentStory, ResourceProvider.GetString("LOC_VndbNexus_RelationTypeParentStory") },
                { VnRelationTypeEnum.Prequel, ResourceProvider.GetString("LOC_VndbNexus_RelationTypePrequel") },
                { VnRelationTypeEnum.Sequel, ResourceProvider.GetString("LOC_VndbNexus_RelationTypeSequel") },
                { VnRelationTypeEnum.SameSeries, ResourceProvider.GetString("LOC_VndbNexus_RelationTypeSameSeries") },
                { VnRelationTypeEnum.SameSetting, ResourceProvider.GetString("LOC_VndbNexus_RelationTypeSameSetting") },
                { VnRelationTypeEnum.SideStory, ResourceProvider.GetString("LOC_VndbNexus_RelationTypeSideStory") }
            });

            AddStringMapDictionary<VnLengthEnum>(new Dictionary<Enum, string>
            {
                { VnLengthEnum.VeryShort, ResourceProvider.GetString("LOC_VndbNexus_LengthVeryShort") },
                { VnLengthEnum.Short, ResourceProvider.GetString("LOC_VndbNexus_LengthShort") },
                { VnLengthEnum.Medium, ResourceProvider.GetString("LOC_VndbNexus_LengthMedium") },
                { VnLengthEnum.Long, ResourceProvider.GetString("LOC_VndbNexus_LengthLong") },
                { VnLengthEnum.VeryLong, ResourceProvider.GetString("LOC_VndbNexus_LengthVeryLong") }
            });

            AddStringMapDictionary<LanguageEnum>(new Dictionary<Enum, string>
            {
                { LanguageEnum.Unknown, ResourceProvider.GetString("LOC_VndbNexus_LanguageUnknown") },
                { LanguageEnum.Arabic, ResourceProvider.GetString("LOC_VndbNexus_LanguageArabic") },
                { LanguageEnum.Basque, ResourceProvider.GetString("LOC_VndbNexus_LanguageBasque") },
                { LanguageEnum.Bulgarian, ResourceProvider.GetString("LOC_VndbNexus_LanguageBulgarian") },
                { LanguageEnum.Catalan, ResourceProvider.GetString("LOC_VndbNexus_LanguageCatalan") },
                { LanguageEnum.Cherokee, ResourceProvider.GetString("LOC_VndbNexus_LanguageCherokee") },
                { LanguageEnum.Chinese, ResourceProvider.GetString("LOC_VndbNexus_LanguageChinese") },
                { LanguageEnum.ChineseSimplified, ResourceProvider.GetString("LOC_VndbNexus_LanguageChineseSimplified") },
                { LanguageEnum.ChineseTraditional, ResourceProvider.GetString("LOC_VndbNexus_LanguageChineseTraditional") },
                { LanguageEnum.Croatian, ResourceProvider.GetString("LOC_VndbNexus_LanguageCroatian") },
                { LanguageEnum.Czech, ResourceProvider.GetString("LOC_VndbNexus_LanguageCzech") },
                { LanguageEnum.Danish, ResourceProvider.GetString("LOC_VndbNexus_LanguageDanish") },
                { LanguageEnum.Dutch, ResourceProvider.GetString("LOC_VndbNexus_LanguageDutch") },
                { LanguageEnum.English, ResourceProvider.GetString("LOC_VndbNexus_LanguageEnglish") },
                { LanguageEnum.Esperanto, ResourceProvider.GetString("LOC_VndbNexus_LanguageEsperanto") },
                { LanguageEnum.Finnish, ResourceProvider.GetString("LOC_VndbNexus_LanguageFinnish") },
                { LanguageEnum.French, ResourceProvider.GetString("LOC_VndbNexus_LanguageFrench") },
                { LanguageEnum.German, ResourceProvider.GetString("LOC_VndbNexus_LanguageGerman") },
                { LanguageEnum.Greek, ResourceProvider.GetString("LOC_VndbNexus_LanguageGreek") },
                { LanguageEnum.Hebrew, ResourceProvider.GetString("LOC_VndbNexus_LanguageHebrew") },
                { LanguageEnum.Hindi, ResourceProvider.GetString("LOC_VndbNexus_LanguageHindi") },
                { LanguageEnum.Hungarian, ResourceProvider.GetString("LOC_VndbNexus_LanguageHungarian") },
                { LanguageEnum.Irish, ResourceProvider.GetString("LOC_VndbNexus_LanguageIrish") },
                { LanguageEnum.Indonesian, ResourceProvider.GetString("LOC_VndbNexus_LanguageIndonesian") },
                { LanguageEnum.Italian, ResourceProvider.GetString("LOC_VndbNexus_LanguageItalian") },
                { LanguageEnum.Inuktitut, ResourceProvider.GetString("LOC_VndbNexus_LanguageInuktitut") },
                { LanguageEnum.Japanese, ResourceProvider.GetString("LOC_VndbNexus_LanguageJapanese") },
                { LanguageEnum.Korean, ResourceProvider.GetString("LOC_VndbNexus_LanguageKorean") },
                { LanguageEnum.Latin, ResourceProvider.GetString("LOC_VndbNexus_LanguageLatin") },
                { LanguageEnum.Latvian, ResourceProvider.GetString("LOC_VndbNexus_LanguageLatvian") },
                { LanguageEnum.Lithuanian, ResourceProvider.GetString("LOC_VndbNexus_LanguageLithuanian") },
                { LanguageEnum.Macedonian, ResourceProvider.GetString("LOC_VndbNexus_LanguageMacedonian") },
                { LanguageEnum.Malay, ResourceProvider.GetString("LOC_VndbNexus_LanguageMalay") },
                { LanguageEnum.Norwegian, ResourceProvider.GetString("LOC_VndbNexus_LanguageNorwegian") },
                { LanguageEnum.Persian, ResourceProvider.GetString("LOC_VndbNexus_LanguagePersian") },
                { LanguageEnum.Polish, ResourceProvider.GetString("LOC_VndbNexus_LanguagePolish") },
                { LanguageEnum.PortugueseBrazil, ResourceProvider.GetString("LOC_VndbNexus_LanguagePortugueseBrazil") },
                { LanguageEnum.PortuguesePortugal, ResourceProvider.GetString("LOC_VndbNexus_LanguagePortuguesePortugal") },
                { LanguageEnum.Romanian, ResourceProvider.GetString("LOC_VndbNexus_LanguageRomanian") },
                { LanguageEnum.Russian, ResourceProvider.GetString("LOC_VndbNexus_LanguageRussian") },
                { LanguageEnum.ScottishGaelic, ResourceProvider.GetString("LOC_VndbNexus_LanguageScottishGaelic") },
                { LanguageEnum.Serbian, ResourceProvider.GetString("LOC_VndbNexus_LanguageSerbian") },
                { LanguageEnum.Slovak, ResourceProvider.GetString("LOC_VndbNexus_LanguageSlovak") },
                { LanguageEnum.Slovene, ResourceProvider.GetString("LOC_VndbNexus_LanguageSlovene") },
                { LanguageEnum.Spanish, ResourceProvider.GetString("LOC_VndbNexus_LanguageSpanish") },
                { LanguageEnum.Swedish, ResourceProvider.GetString("LOC_VndbNexus_LanguageSwedish") },
                { LanguageEnum.Tagalog, ResourceProvider.GetString("LOC_VndbNexus_LanguageTagalog") },
                { LanguageEnum.Thai, ResourceProvider.GetString("LOC_VndbNexus_LanguageThai") },
                { LanguageEnum.Turkish, ResourceProvider.GetString("LOC_VndbNexus_LanguageTurkish") },
                { LanguageEnum.Ukrainian, ResourceProvider.GetString("LOC_VndbNexus_LanguageUkrainian") },
                { LanguageEnum.Urdu, ResourceProvider.GetString("LOC_VndbNexus_LanguageUrdu") },
                { LanguageEnum.Vietnamese, ResourceProvider.GetString("LOC_VndbNexus_LanguageVietnamese") }
            });
        }


    }


}