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
        private static readonly HashSet<Type> _numericTypes = new HashSet<Type>
        {
            typeof(byte), typeof(sbyte),
            typeof(short), typeof(ushort),
            typeof(int), typeof(uint),
            typeof(long), typeof(ulong),
            typeof(float), typeof(double),
            typeof(decimal)
        };

        private static bool IsNumericType(Type type)
        {
            return _numericTypes.Contains(type);
        }

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

            public static T NullEnum<T>(T? argumentValue, string message = null, [CallerMemberName] string argumentName = null) where T : struct, Enum
            {
                if (!argumentValue.HasValue)
                {
                    throw new ArgumentNullException(argumentName, message ?? "Argument cannot be null.");
                }

                return argumentValue.Value;
            }

            public static int Null(int? argumentValue, string message = null, [CallerMemberName] string argumentName = null) =>
                Null<int>(argumentValue, message, argumentName);

            public static uint Null(uint? argumentValue, string message = null, [CallerMemberName] string argumentName = null) =>
                Null<uint>(argumentValue, message, argumentName);

            public static double Null(double? argumentValue, string message = null, [CallerMemberName] string argumentName = null) =>
                Null<double>(argumentValue, message, argumentName);

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

            /// <summary>
            /// Ensures that the specified argument is within the specified range.
            /// </summary>
            /// <typeparam name="T">The type of the argument (must be a comparable numeric type).</typeparam>
            /// <param name="argumentValue">The argument to check.</param>
            /// <param name="min">The minimum value (inclusive).</param>
            /// <param name="max">The maximum value (inclusive).</param>
            /// <param name="message">The error message to include in the exception if the argument is out of range (optional).</param>
            /// <param name="argumentName">The name of the argument (automatically populated).</param>
            /// <returns>The validated argument value.</returns>
            private static T NotInRange<T>(T argumentValue, T min, T max, string message = null, [CallerMemberName] string argumentName = null) where T : IComparable<T>
            {
                if (!IsNumericType(typeof(T)))
                {
                    throw new ArgumentException("Type must be a numeric type.", nameof(T));
                }

                if (argumentValue.CompareTo(min) < 0 || argumentValue.CompareTo(max) > 0)
                {
                    throw new ArgumentOutOfRangeException(argumentName, argumentValue, message ?? $"Value must be between {min} and {max}.");
                }

                return argumentValue;
            }

            /// <summary>
            /// Ensures that the specified argument is not less than the specified minimum value.
            /// </summary>
            /// <typeparam name="T">The type of the argument (must be a comparable numeric type).</typeparam>
            /// <param name="argumentValue">The argument to check.</param>
            /// <param name="min">The minimum value.</param>
            /// <param name="message">The error message to include in the exception if the argument is less than the minimum value (optional).</param>
            /// <param name="argumentName">The name of the argument (automatically populated).</param>
            /// <returns>The validated argument value.</returns>
            private static T NotLessThan<T>(T argumentValue, T min, string message = null, [CallerMemberName] string argumentName = null) where T : IComparable<T>
            {
                if (!IsNumericType(typeof(T)))
                {
                    throw new ArgumentException("Type must be a numeric type.", nameof(T));
                }

                if (argumentValue.CompareTo(min) < 0)
                {
                    throw new ArgumentOutOfRangeException(argumentName, argumentValue, message ?? $"Value must be greater than or equal to {min}.");
                }

                return argumentValue;
            }

            /// <summary>
            /// Ensures that the specified argument is not greater than the specified maximum value.
            /// </summary>
            /// <typeparam name="T">The type of the argument (must be a comparable numeric type).</typeparam>
            /// <param name="argumentValue">The argument to check.</param>
            /// <param name="max">The maximum value.</param>
            /// <param name="message">The error message to include in the exception if the argument is greater than the maximum value (optional).</param>
            /// <param name="argumentName">The name of the argument (automatically populated).</param>
            /// <returns>The validated argument value.</returns>
            private static T NotGreaterThan<T>(T argumentValue, T max, string message = null, [CallerMemberName] string argumentName = null) where T : IComparable<T>
            {
                if (!IsNumericType(typeof(T)))
                {
                    throw new ArgumentException("Type must be a numeric type.", nameof(T));
                }

                if (argumentValue.CompareTo(max) > 0)
                {
                    throw new ArgumentOutOfRangeException(argumentName, argumentValue, message ?? $"Value must be less than or equal to {max}.");
                }

                return argumentValue;
            }

            public static int NotInRange(int argumentValue, int min, int max, string message = null, [CallerMemberName] string argumentName = null) =>
                NotInRange<int>(argumentValue, min, max, message, argumentName);

            public static uint NotInRange(uint argumentValue, uint min, uint max, string message = null, [CallerMemberName] string argumentName = null) =>
                NotInRange<uint>(argumentValue, min, max, message, argumentName);

            public static double NotInRange(double argumentValue, double min, double max, string message = null, [CallerMemberName] string argumentName = null) =>
                NotInRange<double>(argumentValue, min, max, message, argumentName);

            public static int NotLessThan(int argumentValue, int min, string message = null, [CallerMemberName] string argumentName = null) =>
                NotLessThan<int>(argumentValue, min, message, argumentName);

            public static uint NotLessThan(uint argumentValue, uint min, string message = null, [CallerMemberName] string argumentName = null) =>
                NotLessThan<uint>(argumentValue, min, message, argumentName);

            public static double NotLessThan(double argumentValue, double min, string message = null, [CallerMemberName] string argumentName = null) =>
                NotLessThan<double>(argumentValue, min, message, argumentName);

            public static int NotGreaterThan(int argumentValue, int max, string message = null, [CallerMemberName] string argumentName = null) =>
                NotGreaterThan<int>(argumentValue, max, message, argumentName);

            public static uint NotGreaterThan(uint argumentValue, uint max, string message = null, [CallerMemberName] string argumentName = null) =>
                NotGreaterThan<uint>(argumentValue, max, message, argumentName);

            public static double NotGreaterThan(double argumentValue, double max, string message = null, [CallerMemberName] string argumentName = null) =>
                NotGreaterThan<double>(argumentValue, max, message, argumentName);
        }
    }
}