using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;

namespace VndbApiDomain.SharedKernel
{
    public enum ExtLinksEnum
    {
        /// <summary>
        /// Nintendo (HK)
        /// Url format: https://store.nintendo.com.hk/%d
        /// </summary>
        [StringRepresentation(ExtLinks.Release.NintendoHK)]
        NintendoHK,

        /// <summary>
        /// Nutaku
        /// Url format: https://www.nutaku.net/games/%s/
        /// </summary>
        [StringRepresentation(ExtLinks.Release.Nutaku)]
        Nutaku,

        /// <summary>
        /// ErogameScape
        /// Url format: https://erogamescape.dyndns.org/~ap2/ero/toukei_kaiseki/game.php?game=%d
        /// </summary>
        [StringRepresentation(ExtLinks.Release.ErogameScape)]
        ErogameScape,

        /// <summary>
        /// Melonbooks.com
        /// Url format: https://www.melonbooks.com/index.php?main_page=product_info&products_id=IT%010d
        /// </summary>
        [StringRepresentation(ExtLinks.Release.Melonbooks)]
        Melonbooks,

        /// <summary>
        /// PlayStation Store (JP)
        /// Url format: https://store.playstation.com/ja-jp/product/%s
        /// </summary>
        [StringRepresentation(ExtLinks.Release.PlayStationJP)]
        PlayStationJp,

        /// <summary>
        /// JAST USA
        /// Url format: https://jastusa.com/games/%s/vndb
        /// </summary>
        [StringRepresentation(ExtLinks.Release.JastUsa)]
        JastUsa,

        // Rest of the enum members follow the same pattern
        [StringRepresentation(ExtLinks.Release.Getchu)]
        Getchu,

        [StringRepresentation(ExtLinks.Release.NintendoJP)]
        NintendoJP,

        [StringRepresentation(ExtLinks.Release.PlayStationEU)]
        PlayStationEU,

        [StringRepresentation(ExtLinks.Release.Itch)]
        Itch,

        [StringRepresentation(ExtLinks.Release.PatreonPost)]
        PatreonPost,

        [StringRepresentation(ExtLinks.Release.Toranoana)]
        Toranoana,

        [StringRepresentation(ExtLinks.Release.BOOTH)]
        BOOTH,

        [StringRepresentation(ExtLinks.Release.SubscribeStar)]
        SubscribeStar,

        [StringRepresentation(ExtLinks.Release.NovelGame)]
        NovelGame,

        [StringRepresentation(ExtLinks.Release.Freem)]
        Freem,

        [StringRepresentation(ExtLinks.Release.FreegameMugen)]
        FreegameMugen,

        [StringRepresentation(ExtLinks.Release.MelonbooksJP)]
        MelonbooksJP,

        [StringRepresentation(ExtLinks.Release.GooglePlay)]
        GooglePlay,

        [StringRepresentation(ExtLinks.Release.Digiket)]
        Digiket,

        [StringRepresentation(ExtLinks.Release.DMM)]
        DMM,

        [StringRepresentation(ExtLinks.Release.AppStore)]
        AppStore,

        [StringRepresentation(ExtLinks.Release.GameJolt)]
        GameJolt,

        [StringRepresentation(ExtLinks.Release.GetchuDL)]
        GetchuDL,

        [StringRepresentation(ExtLinks.Release.Fakku)]
        Fakku,

        [StringRepresentation(ExtLinks.Release.MangaGamer)]
        MangaGamer,

        [StringRepresentation(ExtLinks.Release.GOG)]
        GOG,

        [StringRepresentation(ExtLinks.Release.AnimateGames)]
        AnimateGames,

        [StringRepresentation(ExtLinks.Release.Patreon)]
        Patreon,

        [StringRepresentation(ExtLinks.Release.Denpasoft)]
        Denpasoft,

        [StringRepresentation(ExtLinks.Release.DLSite)]
        DLSite,

        [StringRepresentation(ExtLinks.Release.PlayStationHK)]
        PlayStationHK,

        [StringRepresentation(ExtLinks.Release.PlayStationNA)]
        PlayStationNA,

        [StringRepresentation(ExtLinks.Release.Gyutto)]
        Gyutto,

        [StringRepresentation(ExtLinks.Release.Nintendo)]
        Nintendo,

        [StringRepresentation(ExtLinks.Release.JList)]
        JList,

        [StringRepresentation(ExtLinks.Release.Steam)]
        Steam
    }

}