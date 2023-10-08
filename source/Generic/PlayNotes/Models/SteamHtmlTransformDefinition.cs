using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayNotes.Models
{
    public class SteamHtmlTransformDefinition
    {
        public readonly string Name;
        public readonly string OriginalClass;
        public readonly string NewName;

        public SteamHtmlTransformDefinition(string name, string originalClass, string newName)
        {
            Name = name;
            OriginalClass = originalClass;
            NewName = newName;
        }
    }
}