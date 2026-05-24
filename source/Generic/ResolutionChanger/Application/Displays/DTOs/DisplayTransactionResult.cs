using DisplayHelper.Domain.Displays.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Application.Displays.DTOs
{
    public sealed class DisplayTransactionResult
    {
        public TransactionState State { get; }
        public string Error { get; }

        public bool Success =>
            State == TransactionState.Succeeded ||
            State == TransactionState.NoChangesApplied;

        private DisplayTransactionResult(
            TransactionState state,
            string error)
        {
            State = state;
            Error = error;
        }

        public static DisplayTransactionResult Succeeded()
            => new DisplayTransactionResult(TransactionState.Succeeded, null);

        public static DisplayTransactionResult NoChangesApplied()
            => new DisplayTransactionResult(TransactionState.NoChangesApplied, null);

        public static DisplayTransactionResult FailedRecovered(string error)
            => new DisplayTransactionResult(TransactionState.FailedRecovered, error);

        public static DisplayTransactionResult FailedUnrecovered(string error)
            => new DisplayTransactionResult(TransactionState.FailedUnrecovered, error);

        public static DisplayTransactionResult InvalidConfiguration(string error)
            => new DisplayTransactionResult(TransactionState.InvalidConfiguration, error);
    }
}
