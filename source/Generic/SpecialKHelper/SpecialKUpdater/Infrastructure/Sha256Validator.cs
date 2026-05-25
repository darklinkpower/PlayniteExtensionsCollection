using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.SpecialKUpdater.Infrastructure
{
    public class Sha256Validator
    {
        public bool Validate(
            string filePath,
            string expectedHash)
        {
            using (var stream = File.OpenRead(filePath))
            {
                using (var sha = SHA256.Create())
                {
                    var hash =
                        sha.ComputeHash(stream);

                    var actual = BitConverter
                        .ToString(hash)
                        .Replace("-", "")
                        .ToLowerInvariant();

                    return string.Equals(
                        actual,
                        expectedHash,
                        StringComparison.OrdinalIgnoreCase);
                }
            }
        }
    }
}
