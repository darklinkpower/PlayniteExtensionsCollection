using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewViewer.Presentation
{
    public class ReactionMetadata
    {
        public string StaticIcon { get; }
        public string AnimatedIcon { get; }

        public string Name { get; }
        public string Description { get; }

        public ReactionMetadata(
            string staticIcon,
            string animatedIcon,
            string name,
            string description)
        {
            StaticIcon = staticIcon;
            AnimatedIcon = animatedIcon;
            Name = name;
            Description = description;
        }
    }
}
