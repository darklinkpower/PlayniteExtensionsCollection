using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Services.JastUsaIntegration.Domain.Entities
{
    public class JastGameData
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
    }
}