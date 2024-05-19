using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginsCommon
{
    /// <summary>
    /// Provides methods to guard against common argument validation scenarios.
    /// </summary>
    public static class Guard
    {
        /// <summary>
        /// Provides methods to guard against common argument validation scenarios.
        /// </summary>
        public static class Against
        {
            /// <summary>
            /// Ensures that the specified argument value is not null.
            /// </summary>
            /// <typeparam name="T">The type of the argument.</typeparam>
            /// <param name="argumentValue">The argument value to check.</param>
            /// <param name="message">The error message to include in the exception if the argument is null (optional).</param>
            /// <param name="argumentName">The name of the argument (automatically populated).</param>
            /// <returns>The validated argument value.</returns>
            public static T Null<T>(T argumentValue, string message = null, [CallerMemberName] string argumentName = null) where T : class
            {
                if (argumentValue is null)
                {
                    throw new ArgumentNullException(message ?? "Argument cannot be Null.", argumentName);
                }

                return argumentValue;
            }

            /// <summary>
            /// Ensures that the specified argument value is not null for nullable value types.
            /// </summary>
            /// <typeparam name="T">The type of the argument.</typeparam>
            /// <param name="argumentValue">The argument value to check.</param>
            /// <param name="message">The error message to include in the exception if the argument is null (optional).</param>
            /// <param name="argumentName">The name of the argument (automatically populated).</param>
            /// <returns>The validated argument value.</returns>
            public static T Null<T>(T? argumentValue, string message = null, [CallerMemberName] string argumentName = null) where T : struct
            {
                if (argumentValue is null)
                {
                    throw new ArgumentNullException(message ?? "Argument cannot be null.", argumentName);
                }

                return argumentValue.Value;
            }

            /// <summary>
            /// Ensures that the specified string argument is not null or empty.
            /// </summary>
            /// <param name="argumentValue">The string argument to check.</param>
            /// <param name="message">The error message to include in the exception if the argument is null or empty (optional).</param>
            /// <param name="argumentName">The name of the argument (automatically populated).</param>
            /// <returns>The validated argument value.</returns>
            public static string NullOrEmpty(string argumentValue, string message = null, [CallerMemberName] string argumentName = null)
            {
                if (string.IsNullOrEmpty(argumentValue))
                {
                    throw new ArgumentException(message ?? "String cannot be null or empty.", argumentName);
                }

                return argumentValue;
            }

            /// <summary>
            /// Ensures that the specified string argument is not null, empty, or consists only of white-space characters.
            /// </summary>
            /// <param name="argumentValue">The string argument to check.</param>
            /// <param name="message">The error message to include in the exception if the argument is null, empty, or consists only of white-space characters (optional).</param>
            /// <param name="argumentName">The name of the argument (automatically populated).</param>
            /// <returns>The validated argument value.</returns>
            public static string NullOrWhiteSpace(string argumentValue, string message = null, [CallerMemberName] string argumentName = null)
            {
                if (string.IsNullOrWhiteSpace(argumentValue))
                {
                    throw new ArgumentException(message ?? "String cannot be null, empty, or consist only of white-space characters.", argumentName);
                }

                return argumentValue;
            }
        }
    }
}