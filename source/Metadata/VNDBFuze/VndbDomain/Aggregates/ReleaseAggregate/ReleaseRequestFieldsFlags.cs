using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Attributes;

namespace VNDBFuze.VndbDomain.Aggregates.ReleaseAggregate
{
    [Flags]
    public enum ReleaseRequestFieldsFlags
    {
        None = 0,
        [StringRepresentation(ReleaseConstants.Fields.Id)]
        Id = 1 << 0,
        [StringRepresentation(ReleaseConstants.Fields.Title)]
        Title = 1 << 1,
        [StringRepresentation(ReleaseConstants.Fields.AltTitle)]
        AltTitle = 1 << 2,
        [StringRepresentation(ReleaseConstants.Fields.LanguagesLang)]
        LanguagesLang = 1 << 3,
        [StringRepresentation(ReleaseConstants.Fields.LanguagesTitle)]
        LanguagesTitle = 1 << 4,
        [StringRepresentation(ReleaseConstants.Fields.LanguagesLatin)]
        LanguagesLatin = 1 << 5,
        [StringRepresentation(ReleaseConstants.Fields.LanguagesMtl)]
        LanguagesMtl = 1 << 6,
        [StringRepresentation(ReleaseConstants.Fields.LanguagesMain)]
        LanguagesMain = 1 << 7,
        [StringRepresentation(ReleaseConstants.Fields.Platforms)]
        Platforms = 1 << 8,
        [StringRepresentation(ReleaseConstants.Fields.MediaMedium)]
        MediaMedium = 1 << 9,
        [StringRepresentation(ReleaseConstants.Fields.MediaQty)]
        MediaQty = 1 << 10,
        [StringRepresentation(ReleaseConstants.Fields.VnsRType)]
        VnsRtype = 1 << 11,
        [StringRepresentation(ReleaseConstants.Fields.ProducersDeveloper)]
        ProducersDeveloper = 1 << 12,
        [StringRepresentation(ReleaseConstants.Fields.ProducersPublisher)]
        ProducersPublisher = 1 << 13,
        [StringRepresentation(ReleaseConstants.Fields.Released)]
        Released = 1 << 14,
        [StringRepresentation(ReleaseConstants.Fields.MinAge)]
        MinAge = 1 << 15,
        [StringRepresentation(ReleaseConstants.Fields.Patch)]
        Patch = 1 << 16,
        [StringRepresentation(ReleaseConstants.Fields.Freeware)]
        Freeware = 1 << 17,
        [StringRepresentation(ReleaseConstants.Fields.Uncensored)]
        Uncensored = 1 << 18,
        [StringRepresentation(ReleaseConstants.Fields.Official)]
        Official = 1 << 19,
        [StringRepresentation(ReleaseConstants.Fields.HasEro)]
        HasEro = 1 << 20,
        [StringRepresentation(ReleaseConstants.Fields.Resolution)]
        Resolution = 1 << 21,
        [StringRepresentation(ReleaseConstants.Fields.Engine)]
        Engine = 1 << 22,
        [StringRepresentation(ReleaseConstants.Fields.Voiced)]
        Voiced = 1 << 23,
        [StringRepresentation(ReleaseConstants.Fields.Notes)]
        Notes = 1 << 24,
        [StringRepresentation(ReleaseConstants.Fields.Gtin)]
        Gtin = 1 << 25,
        [StringRepresentation(ReleaseConstants.Fields.Catalog)]
        Catalog = 1 << 26,
        [StringRepresentation(ReleaseConstants.Fields.ExtLinksUrl)]
        ExtLinksUrl = 1 << 27,
        [StringRepresentation(ReleaseConstants.Fields.ExtLinksLabel)]
        ExtLinksLabel = 1 << 28,
        [StringRepresentation(ReleaseConstants.Fields.ExtLinksName)]
        ExtLinksName = 1 << 29,
        [StringRepresentation(ReleaseConstants.Fields.ExtLinksId)]
        ExtLinksId = 1 << 30
    }

    // Excluded fields: Vns.*, Producers.*.
}
