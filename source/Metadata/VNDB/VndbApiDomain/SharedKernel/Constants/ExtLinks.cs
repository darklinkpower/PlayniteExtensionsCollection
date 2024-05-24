using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VndbApiDomain.SharedKernel
{
    public static class ExtLinks
    {
        public static class Release
        {
            /// <summary>
            /// Nintendo (HK)
            /// Url format: https://store.nintendo.com.hk/%d
            /// </summary>
            public const string NintendoHK = "nintendo_hk";

            /// <summary>
            /// Nutaku
            /// Url format: https://www.nutaku.net/games/%s/
            /// </summary>
            public const string Nutaku = "nutaku";

            /// <summary>
            /// ErogameScape
            /// Url format: https://erogamescape.dyndns.org/~ap2/ero/toukei_kaiseki/game.php?game=%d
            /// </summary>
            public const string ErogameScape = "egs";

            /// <summary>
            /// Melonbooks.com
            /// Url format: https://www.melonbooks.com/index.php?main_page=product_info&products_id=IT%010d
            /// </summary>
            public const string Melonbooks = "melon";

            /// <summary>
            /// PlayStation Store (JP)
            /// Url format: https://store.playstation.com/ja-jp/product/%s
            /// </summary>
            public const string PlayStationJP = "playstation_jp";

            /// <summary>
            /// JAST USA
            /// Url format: https://jastusa.com/games/%s/vndb
            /// </summary>
            public const string JastUsa = "jastusa";

            /// <summary>
            /// Getchu
            /// Url format: http://www.getchu.com/soft.phtml?id=%d
            /// </summary>
            public const string Getchu = "getchu";

            /// <summary>
            /// Nintendo (JP)
            /// Url format: https://store-jp.nintendo.com/list/software/%d.html
            /// </summary>
            public const string NintendoJP = "nintendo_jp";

            /// <summary>
            /// PlayStation Store (EU)
            /// Url format: https://store.playstation.com/en-gb/product/%s
            /// </summary>
            public const string PlayStationEU = "playstation_eu";

            /// <summary>
            /// Itch.io
            /// Url format: https://%s
            /// </summary>
            public const string Itch = "itch";

            /// <summary>
            /// Patreon post
            /// Url format: https://www.patreon.com/posts/%d
            /// </summary>
            public const string PatreonPost = "patreonp";

            /// <summary>
            /// Toranoana
            /// Url format: https://ec.toranoana.shop/tora/ec/item/%012d/
            /// </summary>
            public const string Toranoana = "toranoana";

            /// <summary>
            /// BOOTH
            /// Url format: https://booth.pm/en/items/%d
            /// </summary>
            public const string BOOTH = "booth";

            /// <summary>
            /// SubscribeStar
            /// Url format: https://subscribestar.%s
            /// </summary>
            public const string SubscribeStar = "substar";

            /// <summary>
            /// NovelGame
            /// Url format: https://novelgame.jp/games/show/%d
            /// </summary>
            public const string NovelGame = "novelgam";

            /// <summary>
            /// Freem!
            /// Url format: https://www.freem.ne.jp/win/game/%d
            /// </summary>
            public const string Freem = "freem";

            /// <summary>
            /// Freegame Mugen
            /// Url format: https://freegame-mugen.jp/%s.html
            /// </summary>
            public const string FreegameMugen = "freegame";

            /// <summary>
            /// Melonbooks.co.jp
            /// Url format: https://www.melonbooks.co.jp/detail/detail.php?product_id=%d
            /// </summary>
            public const string MelonbooksJP = "melonjp";

            /// <summary>
            /// Google Play
            /// Url format: https://play.google.com/store/apps/details?id=%s
            /// </summary>
            public const string GooglePlay = "googplay";

            /// <summary>
            /// Digiket
            /// Url format: https://www.digiket.com/work/show/_data/ID=ITM%07d/
            /// </summary>
            public const string Digiket = "digiket";

            /// <summary>
            /// DMM
            /// Url format: https://%s
            /// </summary>
            public const string DMM = "dmm";

            /// <summary>
            /// App Store
            /// Url format: https://apps.apple.com/app/id%d
            /// </summary>
            public const string AppStore = "appstore";

            /// <summary>
            /// Game Jolt
            /// Url format: https://gamejolt.com/games/vn/%d
            /// </summary>
            public const string GameJolt = "gamejolt";

            /// <summary>
            /// DL.Getchu
            /// Url format: http://dl.getchu.com/i/item%d
            /// </summary>
            public const string GetchuDL = "getchudl";

            /// <summary>
            /// Fakku
            /// Url format: https://www.fakku.net/games/%s
            /// </summary>
            public const string Fakku = "fakku";

            /// <summary>
            /// MangaGamer
            /// Url format: https://www.mangagamer.com/r18/detail.php?product_code=%d
            /// </summary>
            public const string MangaGamer = "mg";

            /// <summary>
            /// GOG
            /// Url format: https://www.gog.com/game/%s
            /// </summary>
            public const string GOG = "gog";

            /// <summary>
            /// Animate Games
            /// Url format: https://www.animategames.jp/home/detail/%d
            /// </summary>
            public const string AnimateGames = "animateg";

            /// <summary>
            /// Patreon
            /// Url format: https://www.patreon.com/%s
            /// </summary>
            public const string Patreon = "patreon";

            /// <summary>
            /// Denpasoft
            /// Url format: https://denpasoft.com/product/%s/
            /// </summary>
            public const string Denpasoft = "denpa";

            /// <summary>
            /// DLsite
            /// Url format: https://www.dlsite.com/home/work/=/product_id/%s.html
            /// </summary>
            public const string DLSite = "dlsite";

            /// <summary>
            /// PlayStation Store (HK)
            /// Url format: https://store.playstation.com/en-hk/product/%s
            /// </summary>
            public const string PlayStationHK = "playstation_hk";

            /// <summary>
            /// PlayStation Store (NA)
            /// Url format: https://store.playstation.com/en-us/product/%s
            /// </summary>
            public const string PlayStationNA = "playstation_na";

            /// <summary>
            /// Gyutto
            /// Url format: https://gyutto.com/i/item%d
            /// </summary>
            public const string Gyutto = "gyutto";

            /// <summary>
            /// Nintendo
            /// Url format: https://www.nintendo.com/store/products/%s/
            /// </summary>
            public const string Nintendo = "nintendo";

            /// <summary>
            /// J-List
            /// Url format: https://www.jlist.com/shop/product/%s
            /// </summary>
            public const string JList = "jlist";

            /// <summary>
            /// Steam
            /// Url format: https://store.steampowered.com/app/%d/
            /// </summary>
            public const string Steam = "steam";
        }

        public static class Fields
        {
            /// <summary>
            /// String, URL.
            /// </summary>
            public const string Url = "url";

            /// <summary>
            /// String, English human-readable label for this link.
            /// </summary>
            public const string Label = "label";

            /// <summary>
            /// Internal identifier of the site, intended for applications that want to localize the label or to parse/format/extract remote identifiers. Keep in mind that the list of supported sites, their internal names and their ID types are subject to change, but I’ll try to keep things stable.
            /// </summary>
            public const string Name = "name";

            /// <summary>
            /// Remote identifier for this link. Not all sites have a sensible identifier as part of their URL format, in such cases this field is simply equivalent to the URL.
            /// </summary>
            public const string Id = "id";
        }
    }
}