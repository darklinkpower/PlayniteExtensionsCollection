using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.VndbDomain.Common.Interfaces
{
    public interface IVndbRequestSubfields
    {
        /// <summary>
        /// Enables the flags of all the request fields.
        /// </summary>
        void EnableAllFlags();

        /// <summary>
        /// Disables the flags of all the request fields.
        /// </summary>
        void DisableAllFlags();

        List<string> GetFlagsStringRepresentations(params string[] prefixParts);
    }
}
