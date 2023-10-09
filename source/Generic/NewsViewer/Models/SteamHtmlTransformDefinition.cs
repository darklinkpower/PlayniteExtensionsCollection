using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsViewer.Models
{
    public class SteamHtmlTransformDefinition
    {
        public readonly string Name;
        public readonly string ClassName;
        public readonly string NewName;

        public SteamHtmlTransformDefinition(string name, string className, string newName)
        {
            Name = name;
            ClassName = className;
            NewName = newName;
        }
    }
}
