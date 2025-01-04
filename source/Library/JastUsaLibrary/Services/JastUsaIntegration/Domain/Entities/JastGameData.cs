using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Services.JastUsaIntegration.Domain.Entities
{
    public class JastGameData : IEquatable<JastGameData>
    {
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int GameId { get; set; }
        public string ApiRoute { get; set; }
        public int? EnUsId { get; set; }
        public int? JaId { get; set; }
        public int? ZhHansId { get; set; }
        public int? ZhHantId { get; set; }

        public JastGameData(
            string productName,
            string productCode,
            int gameId,
            string apiRoute,
            int? enUsId,
            int? jaId,
            int? zhHansId,
            int? zhHantId)
        {
            ProductName = Guard.Against.NullOrEmpty(productName);
            ProductCode = Guard.Against.NullOrEmpty(productCode);
            GameId = Guard.Against.Null(gameId);
            ApiRoute = Guard.Against.NullOrEmpty(apiRoute);
            EnUsId = enUsId;
            JaId = jaId;
            ZhHansId = zhHansId;
            ZhHantId = zhHantId;
        }

        public bool Equals(JastGameData other)
        {
            if (other == null)
            {
                return false;
            }

            return string.Equals(ProductName, other.ProductName) &&
                   string.Equals(ProductCode, other.ProductCode) &&
                   GameId == other.GameId &&
                   string.Equals(ApiRoute, other.ApiRoute) &&
                   EnUsId == other.EnUsId &&
                   JaId == other.JaId &&
                   ZhHansId == other.ZhHansId &&
                   ZhHantId == other.ZhHantId;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
                

            if (obj is JastGameData other)
            {
                return Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (ProductName?.GetHashCode() ?? 0) ^
                   (ProductCode?.GetHashCode() ?? 0) ^
                   GameId.GetHashCode() ^
                   (ApiRoute?.GetHashCode() ?? 0) ^
                   (EnUsId?.GetHashCode() ?? 0) ^
                   (JaId?.GetHashCode() ?? 0) ^
                   (ZhHansId?.GetHashCode() ?? 0) ^
                   (ZhHantId?.GetHashCode() ?? 0);
        }

        public static bool operator ==(JastGameData left, JastGameData right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(JastGameData left, JastGameData right)
        {
            return !(left == right);
        }
    }
}