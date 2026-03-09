using System;
using System.Data;
using affolterNET.Data.TestHelpers.Interfaces;

namespace affolterNET.Data.TestHelpers
{
    public sealed class TransactionDecorator : ITransactionDecorator
    {
        private readonly bool _allowCommit;

        public TransactionDecorator(IDbTransaction transaction, bool allowCommit = false)
        {
            WrappedTransaction = transaction;
            _allowCommit = allowCommit;
        }

        public IDbTransaction WrappedTransaction { get; private set; }

        public IDbConnection? Connection => WrappedTransaction.Connection;

        public IsolationLevel IsolationLevel => WrappedTransaction.IsolationLevel;

        public void Dispose()
        {
            WrappedTransaction?.Dispose();
        }

        public void Commit()
        {
            if (_allowCommit)
            {
                WrappedTransaction.Commit();
            }
            else
            {
                throw new InvalidOperationException(
                    "Die TestSqlTransaction wurde so konfiguriert, dass sie keine Commits zuzulässt.");
            }
        }

        public void Rollback()
        {
            WrappedTransaction.Rollback();
            WrappedTransaction.Dispose();
            WrappedTransaction = null!;
        }
    }
}