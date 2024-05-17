using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class LongExtensions
    {
        /// <summary>
        /// Formats a byte value into a human-readable format (e.g., "2.5 GB").
        /// </summary>
        /// <param name="bytes">The number of bytes to format.</param>
        /// <returns>The formatted byte value as a string.</returns>
        public static string ToReadableSize(this long bytes)
        {
            long absolute_i = (bytes < 0 ? -bytes : bytes);
            string suffix;
            double readable;
            if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (bytes >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (bytes >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (bytes >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = bytes;
            }
            else
            {
                return bytes.ToString("0 B"); // Byte
            }

            readable /= 1024;
            return readable.ToString("0.## ") + suffix;
        }
    }
}