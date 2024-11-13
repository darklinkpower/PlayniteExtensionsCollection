using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebViewCore.Domain.Interfaces
{
    /// <summary>
    /// Interface for managing bookmark icons, including caching and retrieving icons for bookmarks.
    /// </summary>
    public interface IBookmarksIconRepository
    {
        /// <summary>
        /// Caches the icon for a given address URI (e.g., favicon) and returns a reference to the cached icon.
        /// </summary>
        /// <param name="addressUri">The URI of the address to cache the icon for.</param>
        /// <returns>The reference or identifier of the cached icon, or an empty string if caching failed.</returns>
        string CacheIcon(Uri addressUri);

        /// <summary>
        /// Checks if an icon with the given name exists in the repository.
        /// </summary>
        /// <param name="iconName">The name of the icon to check for.</param>
        /// <returns>True if the icon exists, false otherwise.</returns>
        bool IconExists(string iconName);

        /// <summary>
        /// Retrieves the full path or reference to the icon with the given name.
        /// </summary>
        /// <param name="iconName">The name of the icon to retrieve.</param>
        /// <returns>The path or reference to the icon, or a default icon path/reference if the icon does not exist.</returns>
        string GetIconPath(string iconName);

        /// <summary>
        /// Copies an existing icon to the repository and returns a reference to the copied icon.
        /// </summary>
        /// <param name="iconPath">The path or reference to the existing icon to copy.</param>
        /// <returns>A reference or identifier for the copied icon, or an empty string if the operation failed.</returns>
        string CopyIconToCache(string iconPath);
    }
}