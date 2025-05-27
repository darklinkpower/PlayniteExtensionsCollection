using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoverCollageMaker.Domain.ValueObjects
{
    public class ImageData
    {
        public string Path { get; }
        public string Name { get; }

        public ImageData(string path)
        {
            Path = path;
            Name = string.Empty;
        }

        public ImageData(string path, string title)
        {
            Path = path;
            Name = title;
        }
    }
}
