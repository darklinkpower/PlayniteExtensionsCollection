using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamWishlistDiscountNotifier.Domain.ValueObjects;

namespace SteamWishlistDiscountNotifier.Domain.Interfaces
{
    /// <summary>
    /// Interface for handling Steam JWT token management, including user authentication state,
    /// and providing mechanisms for handling token validity, retrieval, and callbacks on user login state.
    /// </summary>
    public interface ISteamJwtTokenService
    {
        /// <summary>
        /// Adds a callback to be invoked when the user is not logged in (i.e., when the JWT token is invalid or missing).
        /// </summary>
        /// <param name="callback">The callback action to be invoked when the user is not logged in.</param>
        void AddUserNotLoggedInCallback(Action callback);

        /// <summary>
        /// Removes a previously added callback for when the user is not logged in.
        /// </summary>
        /// <param name="callback">The callback action to remove.</param>
        void RemoveUserNotLoggedInCallback(Action callback);

        /// <summary>
        /// Adds a callback to be invoked when the user is logged in successfully (i.e., when a valid JWT token is obtained).
        /// </summary>
        /// <param name="callback">The callback action to be invoked when the user is logged in.</param>
        void AddUserLoggedInCallback(Action callback);

        /// <summary>
        /// Removes a previously added callback for when the user is logged in.
        /// </summary>
        /// <param name="callback">The callback action to remove.</param>
        void RemoveUserLoggedInCallback(Action callback);

        /// <summary>
        /// Retrieves the current JWT token if valid; if not, attempts to retrieve a new token. 
        /// If the token is invalid or missing, triggers the "user not logged in" callback(s).
        /// </summary>
        /// <returns>A <see cref="SteamAuthInfo"/> object containing the JWT token and user authentication information.</returns>
        SteamAuthInfo GetJwtToken();

        /// <summary>
        /// Invalidates the current JWT token, requiring the user to log in again the next time the token is needed.
        /// </summary>
        void InvalidateToken();

        /// <summary>
        /// Checks if the current JWT token is valid, based on the expiration time and token presence.
        /// </summary>
        /// <returns><c>true</c> if the token is valid; otherwise, <c>false</c>.</returns>
        bool IsTokenValid();
    }

}