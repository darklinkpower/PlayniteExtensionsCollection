using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.SpecialKHandler.Domain.Exceptions
{
    public class SpecialKFileNotFoundException : Exception
    {
        private const string _defaultMessage = "The Special K file could not be found";
        public SpecialKFileNotFoundException() : base(_defaultMessage) { }
        public SpecialKFileNotFoundException(string message) : base(message) { }
        public SpecialKFileNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}