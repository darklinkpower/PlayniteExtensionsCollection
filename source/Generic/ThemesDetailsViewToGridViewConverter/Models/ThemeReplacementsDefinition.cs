using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThemesDetailsViewToGridViewConverter.Models
{
    public class ThemeReplacementsDefinition
    {
        public List<Replacement> Replacements { get; set; }
        public List<string> Removals { get; set; }
    }

    public class Replacement
    {
        public string OriginalBlock { get; set; }

        public string ReplacementBlock { get; set; }
    }
}