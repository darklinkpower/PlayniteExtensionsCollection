using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCommon.Models
{
    /// <summary>
    /// Represents progress information for an ongoing HTTP download operation.
    /// </summary>
    internal class DownloadProgressReport
    {
        /// <summary>
        /// Gets the number of bytes received during the download.
        /// </summary>
        public long BytesReceived { get; }

        /// <summary>
        /// Gets the total number of bytes to receive for the download.
        /// </summary>
        public long TotalBytesToReceive { get; }

        /// <summary>
        /// Gets the amount of time elapsed during the download operation.
        /// </summary>
        public TimeSpan TimeElapsed { get; }

        /// <summary>
        /// Gets the estimated time remaining for the download operation.
        /// </summary>
        public TimeSpan TimeRemaining { get; }

        /// <summary>
        /// Gets the download progress as a percentage, calculated based on bytes received and total bytes to receive.
        /// If total bytes to receive is zero, the progress is set to 0.
        /// </summary>
        public double ProgressPercentage { get; }

        /// <summary>
        /// Gets the download speed in bytes per second.
        /// </summary>
        public long DownloadSpeedBytesPerSecond { get; }

        /// <summary>
        /// Gets the number of bytes received during the download in a human-readable format (e.g., "2.5 GB").
        /// </summary>
        public string FormattedBytesReceived => FormatBytes(BytesReceived);

        /// <summary>
        /// Gets the total number of bytes to receive for the download in a human-readable format (e.g., "5 MB").
        /// </summary>
        public string FormattedTotalBytesToReceive => FormatBytes(TotalBytesToReceive);

        /// <summary>
        /// Gets the download speed in bytes per second in a human-readable format (e.g., "5 MB/s").
        /// </summary>
        public string FormattedDownloadSpeedPerSecond => $"{FormatBytes(DownloadSpeedBytesPerSecond)}/s";

        /// <summary>
        /// Initializes a new instance of the DownloadProgressReporter class with the specified values.
        /// </summary>
        /// <param name="bytesReceived">The number of bytes received during the download.</param>
        /// <param name="totalBytesToReceive">The total number of bytes to receive for the download.</param>
        /// <param name="timeElapsed">The amount of time elapsed during the download operation.</param>
        /// <param name="timeRemaining">The estimated time remaining for the download operation.</param>
        /// <param name="downloadSpeedBytesPerSecond">The current download speed in bytes per second.</param>
        public DownloadProgressReport(long bytesReceived, long totalBytesToReceive, TimeSpan timeElapsed, TimeSpan timeRemaining, long downloadSpeedBytesPerSecond)
        {
            BytesReceived = bytesReceived;
            TotalBytesToReceive = totalBytesToReceive;
            TimeElapsed = timeElapsed;
            TimeRemaining = timeRemaining;
            DownloadSpeedBytesPerSecond = downloadSpeedBytesPerSecond;
            ProgressPercentage = TotalBytesToReceive == 0 ? 0.0 : Math.Round(BytesReceived * 100.0 / TotalBytesToReceive, 2);
        }

        /// <summary>
        /// Formats a byte value into a human-readable format (e.g., "2.5 GB").
        /// </summary>
        /// <param name="bytes">The number of bytes to format.</param>
        /// <returns>The formatted byte value as a string.</returns>
        private static string FormatBytes(long bytes)
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