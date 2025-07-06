using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallationStatusUpdater.Domain.Results
{
    public class StatusUpdateResults
    {
        private readonly List<UpdatedEntryData> _installed = new List<UpdatedEntryData>();
        private readonly List<UpdatedEntryData> _uninstalled = new List<UpdatedEntryData>();

        public IReadOnlyList<UpdatedEntryData> Installed => _installed;
        public IReadOnlyList<UpdatedEntryData> Uninstalled => _uninstalled;

        internal void AddInstalled(Game game)
        {
            _installed.Add(new UpdatedEntryData(game));
        }

        internal void AddUninstalled(Game game)
        {
            _uninstalled.Add(new UpdatedEntryData(game));
        }
    }
}