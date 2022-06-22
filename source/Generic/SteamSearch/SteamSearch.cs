using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SteamSearch
{
    public class SteamSearch : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private SteamSearchSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("78c7912a-32bb-4a42-8485-d348d10023ac");

        public SteamSearch(IPlayniteAPI api) : base(api)
        {
            settings = new SteamSearchSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            Searches = new List<SearchSupport>
            {
                new SearchSupport("st", "Search on Steam", new SteamSearcher(settings))
            };
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new SteamSearchSettingsView();
        }
    }
}