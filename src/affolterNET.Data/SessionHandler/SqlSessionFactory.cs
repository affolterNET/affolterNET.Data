using System;
using System.Data;
using affolterNET.Data.Interfaces;
using affolterNET.Data.Interfaces.SessionHandler;
using Dapper;

namespace affolterNET.Data.SessionHandler
{
    public class SqlSessionFactory : ISqlSessionFactory
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SqlSessionFactory(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            SqlMapper.AddTypeMap(typeof(DateOnly), DbType.Date, true);
            SqlMapper.AddTypeMap(typeof(DateOnly?), DbType.Date, true);
        }

        public ISqlSession CreateSession()
        {
            return new SqlSession(_connectionFactory);
        }
    }
}