using Playnite.SDK;
using ReviewViewer.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewViewer.Presentation
{
    public class ReactionViewModel
    {
        private readonly ReactionMetadata _meta;

        public uint Count { get; }

        public string IconPath => _meta.StaticIcon;
        public string AnimatedIconPath => _meta.AnimatedIcon;

        public string Name => _meta.Name;
        public string Description => _meta.Description;

        public bool ShowCount => Count > 1;

        public string DisplayCount => Count > 999 ? "999+" : Count.ToString();

        public ReactionViewModel(Reaction reaction, ReactionMetadata meta)
        {
            Count = reaction.Count;
            _meta = meta;
        }
    }
}
