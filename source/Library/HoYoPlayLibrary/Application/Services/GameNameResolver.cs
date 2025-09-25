using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoYoPlayLibrary.Application.Services
{
    internal static class GameNameResolver
    {
        private static readonly Dictionary<string, string> GameNames = new Dictionary<string, string>
        {
            { "hk4e", "Genshin Impact" },
            { "bh3", "Honkai Impact 3rd" },
            { "hkrpg", "Honkai: Star Rail" },
            { "nap", "Zenless Zone Zero" }
        };

        public static string Resolve(string id)
        {
            if (id.IsNullOrWhiteSpace())
            {
                return id;
            }

            // Use only the first part before '_', ignoring region suffix, such as global in nap_global
            var baseId = id.Split('_')[0];

            return GameNames.TryGetValue(baseId, out var name) ? name : id;
        }
    }
}
