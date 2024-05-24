using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;

namespace VndbApiDomain.SharedKernel
{
    public enum PlatformEnum
    {
        [StringRepresentation(QueryEnums.Platform.Windows)]
        Windows,

        [StringRepresentation(QueryEnums.Platform.Linux)]
        Linux,

        [StringRepresentation(QueryEnums.Platform.MacOs)]
        MacOs,

        [StringRepresentation(QueryEnums.Platform.Website)]
        Website,

        [StringRepresentation(QueryEnums.Platform.ThreeDO)]
        ThreeDO,

        [StringRepresentation(QueryEnums.Platform.AppleIProduct)]
        AppleIProduct,

        [StringRepresentation(QueryEnums.Platform.Android)]
        Android,

        [StringRepresentation(QueryEnums.Platform.BluRayPlayer)]
        BluRayPlayer,

        [StringRepresentation(QueryEnums.Platform.DOS)]
        DOS,

        [StringRepresentation(QueryEnums.Platform.DVDPlayer)]
        DVDPlayer,

        [StringRepresentation(QueryEnums.Platform.Dreamcast)]
        Dreamcast,

        [StringRepresentation(QueryEnums.Platform.Famicom)]
        Famicom,

        [StringRepresentation(QueryEnums.Platform.SuperFamicom)]
        SuperFamicom,

        [StringRepresentation(QueryEnums.Platform.FM7)]
        FM7,

        [StringRepresentation(QueryEnums.Platform.FM8)]
        FM8,

        [StringRepresentation(QueryEnums.Platform.FMTowns)]
        FMTowns,

        [StringRepresentation(QueryEnums.Platform.GameBoyAdvance)]
        GameBoyAdvance,

        [StringRepresentation(QueryEnums.Platform.GameBoyColor)]
        GameBoyColor,

        [StringRepresentation(QueryEnums.Platform.MSX)]
        MSX,

        [StringRepresentation(QueryEnums.Platform.NintendoDS)]
        NintendoDS,

        [StringRepresentation(QueryEnums.Platform.NintendoSwitch)]
        NintendoSwitch,

        [StringRepresentation(QueryEnums.Platform.NintendoWii)]
        NintendoWii,

        [StringRepresentation(QueryEnums.Platform.NintendoWiiU)]
        NintendoWiiU,

        [StringRepresentation(QueryEnums.Platform.Nintendo3DS)]
        Nintendo3DS,

        [StringRepresentation(QueryEnums.Platform.PC88)]
        PC88,

        [StringRepresentation(QueryEnums.Platform.PC98)]
        PC98,

        [StringRepresentation(QueryEnums.Platform.PCEngine)]
        PCEngine,

        [StringRepresentation(QueryEnums.Platform.PCFX)]
        PCFX,

        [StringRepresentation(QueryEnums.Platform.PlayStationPortable)]
        PlayStationPortable,

        [StringRepresentation(QueryEnums.Platform.PlayStation1)]
        PlayStation1,

        [StringRepresentation(QueryEnums.Platform.PlayStation2)]
        PlayStation2,

        [StringRepresentation(QueryEnums.Platform.PlayStation3)]
        PlayStation3,

        [StringRepresentation(QueryEnums.Platform.PlayStation4)]
        PlayStation4,

        [StringRepresentation(QueryEnums.Platform.PlayStation5)]
        PlayStation5,

        [StringRepresentation(QueryEnums.Platform.PlayStationVita)]
        PlayStationVita,

        [StringRepresentation(QueryEnums.Platform.SegaMegaDrive)]
        SegaMegaDrive,

        [StringRepresentation(QueryEnums.Platform.SegaMegaCD)]
        SegaMegaCD,

        [StringRepresentation(QueryEnums.Platform.SegaSaturn)]
        SegaSaturn,

        [StringRepresentation(QueryEnums.Platform.VNDS)]
        VNDS,

        [StringRepresentation(QueryEnums.Platform.SharpX1)]
        SharpX1,

        [StringRepresentation(QueryEnums.Platform.SharpX68000)]
        SharpX68000,

        [StringRepresentation(QueryEnums.Platform.Xbox)]
        Xbox,

        [StringRepresentation(QueryEnums.Platform.Xbox360)]
        Xbox360,

        [StringRepresentation(QueryEnums.Platform.XboxOne)]
        XboxOne,

        [StringRepresentation(QueryEnums.Platform.XboxX_S)]
        XboxX_S,

        [StringRepresentation(QueryEnums.Platform.OtherMobile)]
        OtherMobile,

        [StringRepresentation(QueryEnums.Platform.Other)]
        Other
    }
}