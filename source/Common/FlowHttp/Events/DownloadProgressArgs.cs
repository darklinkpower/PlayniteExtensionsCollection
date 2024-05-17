using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowHttp.Events
{
    /// <summary>
    /// Represents a callback method that is invoked to report progress during a download operation.
    /// </summary>
    /// <param name="args">An instance of <see cref="DownloadProgressArgs"/> containing information about the download progress.</param>
    public delegate void DownloadProgressChangedCallback(DownloadProgressArgs args);

    /// <summary>
    /// Represents progress information for an ongoing HTTP download operation.
    /// </summary>
    public class DownloadProgressArgs
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
        public string FormattedBytesReceived => BytesReceived.ToReadableSize();

        /// <summary>
        /// Gets the total number of bytes to receive for the download in a human-readable format (e.g., "5 MB").
        /// </summary>
        public string FormattedTotalBytesToReceive => TotalBytesToReceive.ToReadableSize();

        /// <summary>
        /// Gets the download speed in bytes per second in a human-readable format (e.g., "5 MB/s").
        /// </summary>
        public string FormattedDownloadSpeedPerSecond => !IsComplete ? $"{DownloadSpeedBytesPerSecond.ToReadableSize()}/s" : string.Empty;

        public bool IsComplete => TotalBytesToReceive == BytesReceived;

        /// <summary>
        /// Initializes a new instance of the DownloadProgressReporter class with the specified values.
        /// </summary>
        public DownloadProgressArgs(long bytesReceived, long totalBytesToReceive, TimeSpan totalEllapsedDownloadTime, TimeSpan intervalElapsedTime, long bytesReadThisInterval)
        {
            BytesReceived = bytesReceived;
            TotalBytesToReceive = totalBytesToReceive;
            TimeElapsed = totalEllapsedDownloadTime;
            if (intervalElapsedTime.TotalSeconds > 0)
            {
                DownloadSpeedBytesPerSecond = (long)Math.Round(bytesReadThisInterval / intervalElapsedTime.TotalSeconds);
            }
            else
            {
                DownloadSpeedBytesPerSecond = 0;
            }

            TimeRemaining = CalculateTimeRemaining(bytesReadThisInterval, totalBytesToReceive);
            if (TotalBytesToReceive > 0 && BytesReceived > 0)
            {
                ProgressPercentage = Math.Round(BytesReceived / (double)TotalBytesToReceive * 100.0, 2);
            }
            else
            {
                ProgressPercentage = 0;
            }
        }

        /// <summary>
        /// Calculates the estimated time remaining for a download operation based on time elapsed, bytes received, and total bytes to receive.
        /// </summary>
        /// <returns>The estimated time remaining for the download operation.</returns>
        private TimeSpan CalculateTimeRemaining(long bytesReceivedInterval, long totalBytesToReceive)
        {
            if (IsComplete)
            {
                return TimeSpan.MinValue; // Download is completed
            }
            else if (DownloadSpeedBytesPerSecond == 0 || bytesReceivedInterval == 0 || totalBytesToReceive == 0)
            {
                return TimeSpan.MinValue; // Unable to calculate time remaining
            }

            var remainingBytes = TotalBytesToReceive - BytesReceived;
            double remainingSeconds = remainingBytes / DownloadSpeedBytesPerSecond;
            var remainingTime = TimeSpan.FromSeconds(remainingSeconds);
            return remainingTime;
        }
    }
}