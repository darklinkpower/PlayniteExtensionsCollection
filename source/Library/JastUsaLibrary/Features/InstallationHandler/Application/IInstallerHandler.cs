using JastUsaLibrary.Features.InstallationHandler.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Features.InstallationHandler.Application
{
    /// <summary>
    /// Defines a contract for handling a specific type of installer (e.g., EXE, MSI, ZIP).
    /// Each implementation knows how to detect and install its own format.
    /// </summary>
    public interface IInstallerHandler
    {
        /// <summary>
        /// Gets the installer type that this handler supports.
        /// Used to distinguish between different handler implementations.
        /// </summary>
        InstallerType Type { get; }

        /// <summary>
        /// Determines whether this handler can handle the specified installer file.
        /// </summary>
        /// <param name="filePath">Full path to the installer file.</param>
        /// <param name="fileContent">Optional content of the file, if already read into memory.</param>
        /// <param name="executableMetadata">Executable metadata extracted from the file.</param>
        /// <returns><c>true</c> if the handler can process this installer; otherwise, <c>false</c>.</returns>
        bool CanHandle(string filePath, string fileContent, ExecutableMetadata executableMetadata);

        /// <summary>
        /// Performs the actual installation for this handler type.
        /// </summary>
        /// <param name="request">Request object containing installation parameters (source path, target directory, etc.).</param>
        /// <returns><c>true</c> if the installation completed successfully; otherwise, <c>false</c>.</returns>
        bool Install(InstallRequest request);
    }
}
