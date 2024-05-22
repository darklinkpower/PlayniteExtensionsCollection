using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Attributes;
using VNDBFuze.VndbDomain.Common.Constants;

namespace VNDBFuze.VndbDomain.Common.Enums
{
    public enum MediumEnum
    {
        [StringRepresentation(null)]
        Unknown,

        [StringRepresentation(QueryEnums.Medium.CD)]
        CD,

        [StringRepresentation(QueryEnums.Medium.DVD)]
        DVD,

        [StringRepresentation(QueryEnums.Medium.GDROM)]
        GDROM,

        [StringRepresentation(QueryEnums.Medium.BluRayDisc)]
        BluRayDisc,

        [StringRepresentation(QueryEnums.Medium.Floppy)]
        Floppy,

        [StringRepresentation(QueryEnums.Medium.CassetteTape)]
        CassetteTape,

        [StringRepresentation(QueryEnums.Medium.Cartridge)]
        Cartridge,

        [StringRepresentation(QueryEnums.Medium.MemoryCard)]
        MemoryCard,

        [StringRepresentation(QueryEnums.Medium.UMD)]
        UMD,

        [StringRepresentation(QueryEnums.Medium.NintendoOpticalDisc)]
        NintendoOpticalDisc,

        [StringRepresentation(QueryEnums.Medium.InternetDownload)]
        InternetDownload,

        [StringRepresentation(QueryEnums.Medium.Other)]
        Other
    }

}