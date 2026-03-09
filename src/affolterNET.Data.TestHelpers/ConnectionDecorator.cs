using System.Data;
using System.Diagnostics.CodeAnalysis;
using affolterNET.Data.TestHelpers.Interfaces;

namespace affolterNET.Data.TestHelpers
{
    public sealed class ConnectionDecorator : IConnectionDecorator
    {
        private readonly IDbConnection _connection;

        private IDbTransaction? _transaction;

        public ConnectionDecorator(IDbConnection connection)
        {
            _connection = connection;
            ConnectionString = "";
        }

#pragma warning disable 8767
        public string ConnectionString {
            get => _connection.ConnectionString;
            set
            {}
        } 
#pragma warning restore 8767

        public int ConnectionTimeout => _connection.ConnectionTimeout;

        public string Database => _connection.Database;

        public ConnectionState State => _connection.State;

        public void Dispose()
        {
            _connection.Dispose();
        }

        public IDbTransaction BeginTransaction()
        {
            return BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            if (_transaction == null)
            {
                var trans = _connection.BeginTransaction(il);
                _transaction = new TransactionDecorator(trans);
            }

            return _transaction;
        }

        public void Close()
        {
            _connection.Close();
        }

        public void ChangeDatabase(string databaseName)
        {
            _connection.ChangeDatabase(databaseName);
        }

        public IDbCommand CreateCommand()
        {
            return _connection.CreateCommand();
        }

        public void Open()
        {
            _connection.Open();
        }

        public void RollbackTestTransaction()
        {
            _transaction?.Rollback();
            _transaction = null;
        }
    }
}