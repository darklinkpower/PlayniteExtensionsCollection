using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.VndbDomain.Common.Interfaces
{
    public interface IVndbRequestFields
    {
        /// <summary>
        /// Enables the flags of all the request fields.
        /// </summary>
        void EnableAllFlags(bool enableSubfields);

        /// <summary>
        /// Disables the flags of all the request fields.
        /// </summary>
        void DisableAllFlags(bool disableSubfields);

        List<string> GetFlagsStringRepresentations(params string[] prefixParts);
    }
}
