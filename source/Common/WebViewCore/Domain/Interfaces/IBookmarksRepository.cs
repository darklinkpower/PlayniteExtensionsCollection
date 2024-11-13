using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebViewCore.Domain.Entities;

namespace WebViewCore.Domain.Interfaces
{
    /// <summary>
    /// Interface for managing the persistence of bookmarks, including loading and saving them.
    /// </summary>
    public interface IBookmarksRepository
    {
        /// <summary>
        /// Loads all stored bookmarks from the repository.
        /// </summary>
        /// <returns>A list of all stored bookmarks.</returns>
        List<BookmarkInternal> LoadBookmarks();

        /// <summary>
        /// Saves the provided list of bookmarks to the repository.
        /// </summary>
        /// <param name="bookmarks">The list of bookmarks to save.</param>
        void SaveBookmarks(List<BookmarkInternal> bookmarks);

        /// <summary>
        /// Clears the bookmarks the repository.
        /// </summary>
        void ClearBookmarks();
    }
}