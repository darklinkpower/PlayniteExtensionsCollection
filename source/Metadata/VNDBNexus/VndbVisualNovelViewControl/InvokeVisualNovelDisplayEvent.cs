using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.VisualNovelAggregate;

namespace VNDBNexus.VndbVisualNovelViewControlAggregate
{
    public class InvokeVisualNovelDisplayEvent
    {
        public VisualNovel VisualNovel { get; }
        public bool Handled { get; set; }

        public InvokeVisualNovelDisplayEvent(VisualNovel visualNovel)
        {
            VisualNovel = visualNovel;
            Handled = false;
        }
    }
}
