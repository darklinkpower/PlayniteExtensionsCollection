using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.SpecialKHandler.Domain.Exceptions
{
    public class SpecialKPathNotFoundException : Exception
    {
        private const string _defaultMessage = "The Special K path could not be found in the registry.";
        public SpecialKPathNotFoundException() : base(_defaultMessage) { }
        public SpecialKPathNotFoundException(string message) : base(message) { }
        public SpecialKPathNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}