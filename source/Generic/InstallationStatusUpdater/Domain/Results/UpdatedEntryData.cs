using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallationStatusUpdater.Domain.Results
{
    public class UpdatedEntryData
    {
        public string Name { get; }
        public Guid Id { get; }
        public string Source { get; }
        public Guid PluginId { get; }
        public IEnumerable<string> Platforms { get; }

        public UpdatedEntryData(Game game)
        {
            Name = game.Name ?? string.Empty;
            Source = game.Source?.Name ?? string.Empty;
            PluginId = game.PluginId;
            Platforms = game.Platforms?.Select(x => x.Name).ToList() ?? Enumerable.Empty<string>();
            Id = game.Id;
        }
    }
}
