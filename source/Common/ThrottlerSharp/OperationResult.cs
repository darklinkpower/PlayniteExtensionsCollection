using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThrottlerSharp
{
    /// <summary>
    /// Represents the result of an operation, including whether the operation was successful and its result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public class OperationResult<TResult>
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the result of the operation.
        /// </summary>
        public TResult Result { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationResult{TResult}"/> class.
        /// </summary>
        /// <param name="success">A value indicating whether the operation was successful.</param>
        /// <param name="result">The result of the operation.</param>
        public OperationResult(bool success, TResult result)
        {
            Success = success;
            Result = result;
        }
    }
}