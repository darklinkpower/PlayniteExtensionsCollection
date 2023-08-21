using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchCollection.Interfaces
{
    public interface ISearchDefinition
    {
        string Name { get; }
        string Icon { get; }
        string GetSearchUrl(Game game);
    }
}