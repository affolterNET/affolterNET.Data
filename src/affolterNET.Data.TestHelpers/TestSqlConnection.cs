using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace affolterNET.Data.TestHelpers
{
    public sealed class TestSqlConnection : DbConnection
    {
        private readonly SqlConnection _connection;

        private DbTransaction _transaction;

        public TestSqlConnection(SqlConnection connection)
        {
            _connection = connection;
            ConnectionString = _connection.ConnectionString;
        }

        public override string ConnectionString
        {
            get => _connection.ConnectionString;
            set => _connection.ConnectionString = value;
        }

        public override string Database => _connection.Database;

        public override ConnectionState State => _connection.State;

        public override string DataSource => _connection.DataSource;

        public override string ServerVersion => _connection.ServerVersion;

        protected override DbTransaction BeginDbTransaction(IsolationLevel il)
        {
            if (_transaction == null)
            {
                var trans = _connection.BeginTransaction(il);
                _transaction = new TestSqlTransaction(trans);
            }

            return _transaction;
        }

        public override void ChangeDatabase(string databaseName)
        {
            _connection.ChangeDatabase(databaseName);
        }

        public override void Close()
        {
            _connection.Close();
        }

        public override void Open()
        {
            _connection.Open();
        }

        protected override DbCommand CreateDbCommand()
        {
            return _connection.CreateCommand();
        }
    }
}