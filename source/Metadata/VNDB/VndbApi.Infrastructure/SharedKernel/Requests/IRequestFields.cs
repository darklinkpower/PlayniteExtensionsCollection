using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VndbApi.Infrastructure.SharedKernel.Requests
{
    public interface IRequestFields
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
