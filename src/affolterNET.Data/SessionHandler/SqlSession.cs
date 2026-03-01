using System;
using System.Data;
using affolterNET.Data.Interfaces;
using affolterNET.Data.Interfaces.SessionHandler;

namespace affolterNET.Data.SessionHandler
{
    internal sealed class SqlSession : ISqlSession
    {
        private bool _disposed;

        public SqlSession(IDbConnectionFactory connectionFactory)
        {
            Connection = connectionFactory.CreateConnection();
            Connection.Open();
        }

        public bool HasTransaction => Transaction != null;

        public IDbConnection Connection { get; private set; }

        public IDbTransaction? Transaction { get; private set; }

        public void Begin(IsolationLevel isolationLevel = IsolationLevel.ReadUncommitted)
        {
            Transaction = Connection.BeginTransaction(isolationLevel);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Commit()
        {
            if (Transaction == null)
            {
                throw new InvalidOperationException("Transaction was null");
            }

            Transaction.Commit();
        }

        public void Rollback()
        {
            if (Transaction == null)
            {
                throw new InvalidOperationException("Transaction was null");
            }
            Transaction.Rollback();
        }

        // ReSharper disable once StyleCop.SA1201
        ~SqlSession()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Transaction Dispose (wenn noch da...)
                    Transaction?.Dispose();
                    Transaction = null;

                    // Connection Dispose
                    Connection.Close();
                    Connection?.Dispose();
                    Connection = null!;
                }

                _disposed = true;
            }
        }
    }
}