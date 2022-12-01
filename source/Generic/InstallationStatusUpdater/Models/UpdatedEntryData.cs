using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallationStatusUpdater.Models
{
    public class UpdatedEntryData
    {
        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
            }
        }

        private Guid id;
        public Guid Id
        {
            get => id;
            set
            {
                id = value;
            }
        }

        private string source;
        public string Source
        {
            get => source;
            set
            {
                source = value;
            }
        }

        private Guid pluginId;
        public Guid PluginId
        {
            get => pluginId;
            set
            {
                pluginId = value;
            }
        }

        private IEnumerable<string> platforms;
        public IEnumerable<string> Platforms
        {
            get => platforms;
            set
            {
                platforms = value;
            }
        }

        public UpdatedEntryData(Game game)
        {
            Name = game.Name ?? string.Empty;
            Source = game.Source?.Name ?? string.Empty;
            PluginId = game.PluginId;
            Platforms = game.Platforms?.Select(x => x.Name) ?? new List<string>();
            Id = game.Id;
        }

    }
}
