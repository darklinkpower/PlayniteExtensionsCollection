using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.Fields
{
    [Flags]
    public enum ReleaseFields
    {
        None = 0,
        Id = 1 << 0,
        Title = 1 << 1,
        AltTitle = 1 << 2,
        Languages = 1 << 3,
        LanguagesLang = 1 << 4,
        LanguagesTitle = 1 << 5,
        LanguagesLatin = 1 << 6,
        LanguagesMtl = 1 << 7,
        LanguagesMain = 1 << 8,
        Platforms = 1 << 9,
        Media = 1 << 10,
        MediaMedium = 1 << 11,
        MediaQty = 1 << 12,
        Vns = 1 << 13,
        VnsRtype = 1 << 14,
        Producers = 1 << 15,
        ProducersDeveloper = 1 << 16,
        ProducersPublisher = 1 << 17,
        Released = 1 << 18,
        MinAge = 1 << 19,
        Patch = 1 << 20,
        Freeware = 1 << 21,
        Uncensored = 1 << 22,
        Official = 1 << 23,
        HasEro = 1 << 24,
        Resolution = 1 << 25,
        Engine = 1 << 26,
        Voiced = 1 << 27,
        Notes = 1 << 28,
        Gtin = 1 << 29,
        Catalog = 1 << 30,
        ExtLinks = 1 << 31,
        ExtLinksUrl = 1 << 32,
        ExtLinksLabel = 1 << 33,
        ExtLinksName = 1 << 34,
        ExtLinksId = 1 << 35,
    }

    // Excluded fields: Vns.*, Producers.*.
}
