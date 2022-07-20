using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamesSizeCalculator.Models
{
    public class SteamAppListRoot
    {
        [SerializationPropertyName("applist")]
        public AppList Applist { get; set; }
    }

    public class AppList
    {
        [SerializationPropertyName("apps")]
        public List<SteamApp> Apps { get; set; } = new List<SteamApp>();
    }

    public class SteamApp
    {
        [SerializationPropertyName("appid")]
        public int Appid { get; set; }
        [SerializationPropertyName("name")]
        public string Name { get; set; }
    }
}