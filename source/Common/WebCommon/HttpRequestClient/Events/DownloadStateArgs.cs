using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCommon.Enums;

namespace WebCommon.HttpRequestClient.Events
{
    /// <summary>
    /// Represents a callback method that is invoked to report the state of a download operation.
    /// </summary>
    /// <param name="args">An instance of <see cref="DownloadStateArgs"/> containing information about the download state.</param>
    public delegate void DownloadStateChangedCallback(DownloadStateArgs args);

    /// <summary>
    /// Contains information about the state of a download operation.
    /// </summary>
    public class DownloadStateArgs
    {
        /// <summary>
        /// Gets the status of the download operation.
        /// </summary>
        public HttpRequestClientStatus Status { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadStateArgs"/> class with the specified status.
        /// </summary>
        /// <param name="status">The status of the download operation.</param>
        public DownloadStateArgs(HttpRequestClientStatus status)
        {
            Status = status;
        }
    }

}