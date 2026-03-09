using System.Collections.Generic;
using System.Data;
using System.Linq;
using affolterNET.Data.Interfaces;
using Dapper;

namespace affolterNET.Data.TestHelpers.Builders
{
    public class SelectBuilder<T> : CrudBase<T>
        where T : IDtoBase
    {
        private string sql = string.Empty;
        private string _suffix = string.Empty;

        public SelectBuilder(IDbConnection connection, IDbTransaction transaction, IDtoBase dto)
            : base(connection, transaction, dto)
        {
        }

        public SelectBuilder<T> WithWhere(string col, object value, bool whereIn = false)
        {
            AddWhere(col, value, whereIn);
            return this;
        }

        public SelectBuilder<T> WithWhereIn(string col, object values)
        {
            return WithWhere(col, values, true);
        }

        public T ExecuteSingle()
        {
            if (IsPostgres)
            {
                sql = $"select * from {TableName}";
                _suffix = " limit 1";
            }
            else
            {
                sql = $"select top(1) * from {TableName}";
            }
            return RunExecute().FirstOrDefault()!;
        }

        public IEnumerable<T> Execute()
        {
            sql = $"select * from {TableName}";
            return RunExecute();
        }

        private IEnumerable<T> RunExecute()
        {
            sql += BuildWhereClause();
            sql += _suffix;
            var list = Connection.Query<T>(sql, Paras, Transaction);
            return list;
        }
    }
}