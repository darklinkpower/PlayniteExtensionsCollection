using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoverCollageMaker.Domain.Exceptions
{
    public class InsufficientSpaceForImageException : Exception
    {
        public InsufficientSpaceForImageException()
            : base("Unable to create image. Available space is insufficient for the requested dimensions.")
        {
        }

        public InsufficientSpaceForImageException(string message)
            : base(message)
        {
        }

        public InsufficientSpaceForImageException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}