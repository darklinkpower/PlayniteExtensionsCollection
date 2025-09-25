using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoYoPlayLibrary.Domain.Entities
{
    internal class HoyoPlayGame
    {
        public string Id { get; }
        public string Name { get; }
        public string InstallDirectory { get; }
        public string ExePath { get; }

        public HoyoPlayGame(string id, string name, string installDirectory, string exePath)
        {
            if (id.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Game ID cannot be null or empty.", nameof(id));
            }

            if (name.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Game name cannot be null or empty.", nameof(name));
            }

            if (installDirectory.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Install directory cannot be null or empty.", nameof(installDirectory));
            }

            if (exePath.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Exe path cannot be null or empty.", nameof(exePath));
            }

            Id = id;
            Name = name;
            InstallDirectory = installDirectory;
            ExePath = exePath;
        }
    }
}