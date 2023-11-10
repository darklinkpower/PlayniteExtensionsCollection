using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameRelations.Interfaces
{
    public interface IGameRelationsControlSettings
    {
        bool DisplayGameNames { get; set; }
        int MaxItems { get; set; }
        bool IsEnabled { get; set; }
        bool IsVisible { get; set; }
        bool DisplayOnlyInstalled { get; set; }
    }
}