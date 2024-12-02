using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Application.Steam.JwtToken
{
    public class SteamJwtTokenPayload
    {
        [SerializationPropertyName("iss")]
        public string Iss { get; set; }

        [SerializationPropertyName("sub")]
        public string Sub { get; set; }

        [SerializationPropertyName("aud")]
        public string[] Aud { get; set; }

        [SerializationPropertyName("exp")]
        public long Exp { get; set; }

        [SerializationPropertyName("nbf")]
        public long Nbf { get; set; }

        [SerializationPropertyName("iat")]
        public long Iat { get; set; }

        [SerializationPropertyName("jti")]
        public string Jti { get; set; }

        [SerializationPropertyName("oat")]
        public long Oat { get; set; }

        [SerializationPropertyName("rt_exp")]
        public long RtExp { get; set; }

        [SerializationPropertyName("per")]
        public long Per { get; set; }

        [SerializationPropertyName("ip_subject")]
        public string IpSubject { get; set; }

        [SerializationPropertyName("ip_confirmer")]
        public string IpConfirmer { get; set; }
    }
}
