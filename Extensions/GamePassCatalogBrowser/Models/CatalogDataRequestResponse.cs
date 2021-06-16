using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassCatalogBrowser.Models
{
    public partial class CatalogData
    {
        [JsonProperty("Products")]
        public CatalogProduct[] Products { get; set; }
    }

    public partial class CatalogProduct
    {
        [JsonProperty("LastModifiedDate")]
        public DateTimeOffset LastModifiedDate { get; set; }

        [JsonProperty("LocalizedProperties")]
        public ProductLocalizedProperty[] LocalizedProperties { get; set; }

        [JsonProperty("ProductId")]
        public string ProductId { get; set; }

        [JsonProperty("Properties")]
        public ProductProperties Properties { get; set; }
    }

    public partial class ProductLocalizedProperty
    {
        [JsonProperty("DeveloperName")]
        public string DeveloperName { get; set; }

        [JsonProperty("PublisherName")]
        public string PublisherName { get; set; }

        [JsonProperty("PublisherWebsiteUri")]
        public string PublisherWebsiteUri { get; set; }


        [JsonProperty("Franchises")]
        public object[] Franchises { get; set; }

        [JsonProperty("Images")]
        public Image[] Images { get; set; }

        [JsonProperty("ProductDescription")]
        public string ProductDescription { get; set; }

        [JsonProperty("ProductTitle")]
        public string ProductTitle { get; set; }

        [JsonProperty("ShortTitle")]
        public string ShortTitle { get; set; }

        [JsonProperty("SortTitle")]
        public string SortTitle { get; set; }

        [JsonProperty("ShortDescription")]
        public string ShortDescription { get; set; }

        [JsonProperty("Language")]
        public string Language { get; set; }

        [JsonProperty("Markets")]
        public Market[] Markets { get; set; }
    }

    public partial class ProductProperties
    {
        [JsonProperty("Attributes")]
        public Attribute[] Attributes { get; set; }

        [JsonProperty("Category")]
        public string Category { get; set; }

        [JsonProperty("Categories")]
        public string[] Categories { get; set; }

        [JsonProperty("PackageFamilyName")]
        public string PackageFamilyName { get; set; }

        [JsonProperty("PackageIdentityName")]
        public string PackageIdentityName { get; set; }

        [JsonProperty("PublisherCertificateName")]
        public string PublisherCertificateName { get; set; }

        [JsonProperty("HasAddOns")]
        public bool HasAddOns { get; set; }
    }

    public partial class Attribute
    {
        [JsonProperty("Name")]
        public string Name { get; set; }
    }

    public partial class Image
    {
        [JsonProperty("Height")]
        public long Height { get; set; }

        [JsonProperty("Width")]
        public long Width { get; set; }

        [JsonProperty("ImagePurpose")]
        public ImagePurpose ImagePurpose { get; set; }

        [JsonProperty("Uri")]
        public string Uri { get; set; }
    }

    public enum Market { Ad, Ae, Af, Ag, Ai, Al, Am, Ao, Aq, Ar, As, At, Au, Aw, Ax, Az, Ba, Bb, Bd, Be, Bf, Bg, Bh, Bi, Bj, Bl, Bm, Bn, Bo, Bq, Br, Bs, Bt, Bv, Bw, By, Bz, Ca, Cc, Cd, Cf, Cg, Ch, Ci, Ck, Cl, Cm, Cn, Co, Cr, Cv, Cw, Cx, Cy, Cz, De, Dj, Dk, Dm, Do, Dz, Ec, Ee, Eg, Eh, Er, Es, Et, Fi, Fj, Fk, Fm, Fo, Fr, Ga, Gb, Gd, Ge, Gf, Gg, Gh, Gi, Gl, Gm, Gn, Gp, Gq, Gr, Gs, Gt, Gu, Gw, Gy, Hk, Hm, Hn, Hr, Ht, Hu, Id, Ie, Il, Im, In, Io, Iq, Is, It, Je, Jm, Jo, Jp, Ke, Kg, Kh, Ki, Km, Kn, Kr, Kw, Ky, Kz, La, Lb, Lc, Li, Lk, Lr, Ls, Lt, Lu, Lv, Ly, Ma, Mc, Md, Me, Mf, Mg, Mh, Mk, Ml, Mm, Mn, Mo, Mp, Mq, Mr, Ms, Mt, Mu, Mv, Mw, Mx, My, Mz, Na, Nc, Ne, Neutral, Nf, Ng, Ni, Nl, No, Np, Nr, Nu, Nz, Om, Pa, Pe, Pf, Pg, Ph, Pk, Pl, Pm, Pn, Ps, Pt, Pw, Py, Qa, Re, Ro, Rs, Ru, Rw, Sa, Sb, Sc, Se, Sg, Sh, Si, Sj, Sk, Sl, Sm, Sn, So, Sr, St, Sv, Sx, Sz, Tc, Td, Tf, Tg, Th, Tj, Tk, Tl, Tm, Tn, To, Tr, Tt, Tv, Tw, Tz, Ua, Ug, Um, Us, Uy, Uz, Va, Vc, Ve, Vg, Vi, Vn, Vu, Wf, Ws, Ye, Yt, Za, Zm, Zw };

    public enum ImagePurpose { BoxArt, BrandedKeyArt, FeaturePromotionalSquareArt, Hero, Logo, Poster, Screenshot, SuperHeroArt, Tile, TitledHeroArt, Trailer };
}