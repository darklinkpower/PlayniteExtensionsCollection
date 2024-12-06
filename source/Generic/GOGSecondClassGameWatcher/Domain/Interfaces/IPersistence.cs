using GOGSecondClassGameWatcher.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOGSecondClassGameWatcher.Domain.Interfaces
{
    public interface IPersistence
    {
        void SaveGames(IEnumerable<GogSecondClassGame> games);
    }
}
