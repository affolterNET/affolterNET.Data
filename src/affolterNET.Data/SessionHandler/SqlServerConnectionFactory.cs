using System.Data;
using affolterNET.Data.Interfaces;
using Microsoft.Data.SqlClient;

namespace affolterNET.Data.SessionHandler
{
    public class SqlServerConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqlServerConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
