using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Attributes;
using VNDBFuze.VndbDomain.Common.Constants;

namespace VNDBFuze.VndbDomain.Common.Enums
{
    public enum LanguageEnum
    {
        [StringRepresentation(null)]
        Unknown,

        [StringRepresentation(QueryEnums.Language.Arabic)]
        Arabic,

        [StringRepresentation(QueryEnums.Language.Basque)]
        Basque,

        [StringRepresentation(QueryEnums.Language.Bulgarian)]
        Bulgarian,

        [StringRepresentation(QueryEnums.Language.Catalan)]
        Catalan,

        [StringRepresentation(QueryEnums.Language.Cherokee)]
        Cherokee,

        [StringRepresentation(QueryEnums.Language.Chinese)]
        Chinese,

        [StringRepresentation(QueryEnums.Language.ChineseSimplified)]
        ChineseSimplified,

        [StringRepresentation(QueryEnums.Language.ChineseTraditional)]
        ChineseTraditional,

        [StringRepresentation(QueryEnums.Language.Croatian)]
        Croatian,

        [StringRepresentation(QueryEnums.Language.Czech)]
        Czech,

        [StringRepresentation(QueryEnums.Language.Danish)]
        Danish,

        [StringRepresentation(QueryEnums.Language.Dutch)]
        Dutch,

        [StringRepresentation(QueryEnums.Language.English)]
        English,

        [StringRepresentation(QueryEnums.Language.Esperanto)]
        Esperanto,

        [StringRepresentation(QueryEnums.Language.Finnish)]
        Finnish,

        [StringRepresentation(QueryEnums.Language.French)]
        French,

        [StringRepresentation(QueryEnums.Language.German)]
        German,

        [StringRepresentation(QueryEnums.Language.Greek)]
        Greek,

        [StringRepresentation(QueryEnums.Language.Hebrew)]
        Hebrew,

        [StringRepresentation(QueryEnums.Language.Hindi)]
        Hindi,

        [StringRepresentation(QueryEnums.Language.Hungarian)]
        Hungarian,

        [StringRepresentation(QueryEnums.Language.Irish)]
        Irish,

        [StringRepresentation(QueryEnums.Language.Indonesian)]
        Indonesian,

        [StringRepresentation(QueryEnums.Language.Italian)]
        Italian,

        [StringRepresentation(QueryEnums.Language.Inuktitut)]
        Inuktitut,

        [StringRepresentation(QueryEnums.Language.Japanese)]
        Japanese,

        [StringRepresentation(QueryEnums.Language.Korean)]
        Korean,

        [StringRepresentation(QueryEnums.Language.Latin)]
        Latin,

        [StringRepresentation(QueryEnums.Language.Latvian)]
        Latvian,

        [StringRepresentation(QueryEnums.Language.Lithuanian)]
        Lithuanian,

        [StringRepresentation(QueryEnums.Language.Macedonian)]
        Macedonian,

        [StringRepresentation(QueryEnums.Language.Malay)]
        Malay,

        [StringRepresentation(QueryEnums.Language.Norwegian)]
        Norwegian,

        [StringRepresentation(QueryEnums.Language.Persian)]
        Persian,

        [StringRepresentation(QueryEnums.Language.Polish)]
        Polish,

        [StringRepresentation(QueryEnums.Language.PortugueseBrazil)]
        PortugueseBrazil,

        [StringRepresentation(QueryEnums.Language.PortuguesePortugal)]
        PortuguesePortugal,

        [StringRepresentation(QueryEnums.Language.Romanian)]
        Romanian,

        [StringRepresentation(QueryEnums.Language.Russian)]
        Russian,

        [StringRepresentation(QueryEnums.Language.ScottishGaelic)]
        ScottishGaelic,

        [StringRepresentation(QueryEnums.Language.Serbian)]
        Serbian,

        [StringRepresentation(QueryEnums.Language.Slovak)]
        Slovak,

        [StringRepresentation(QueryEnums.Language.Slovene)]
        Slovene,

        [StringRepresentation(QueryEnums.Language.Spanish)]
        Spanish,

        [StringRepresentation(QueryEnums.Language.Swedish)]
        Swedish,

        [StringRepresentation(QueryEnums.Language.Tagalog)]
        Tagalog,

        [StringRepresentation(QueryEnums.Language.Thai)]
        Thai,

        [StringRepresentation(QueryEnums.Language.Turkish)]
        Turkish,

        [StringRepresentation(QueryEnums.Language.Ukrainian)]
        Ukrainian,

        [StringRepresentation(QueryEnums.Language.Urdu)]
        Urdu,

        [StringRepresentation(QueryEnums.Language.Vietnamese)]
        Vietnamese
    }

}