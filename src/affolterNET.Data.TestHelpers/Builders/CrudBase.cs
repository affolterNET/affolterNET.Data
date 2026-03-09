using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using affolterNET.Data.Extensions;
using affolterNET.Data.Interfaces;

namespace affolterNET.Data.TestHelpers.Builders
{
    public abstract class CrudBase<T>
        where T : IDtoBase
    {
        protected readonly IDbConnection Connection;

        protected readonly IDtoBase Dto;

        protected readonly IDictionary<string, object> Paras = new ExpandoObject()!;

        protected readonly IDbTransaction Transaction;

        protected readonly IList<string> WhereStatements = new List<string>();

        protected CrudBase(IDbConnection connection, IDbTransaction transaction, IDtoBase dto)
        {
            Connection = connection;
            Transaction = transaction;
            Dto = dto;
            TableName = Dto.GetTableName();
        }

        protected string TableName { get; }

        protected bool IsPostgres => !TableName.Contains("[");

        protected string BuildWhereClause()
        {
            return WhereStatements.Count > 0
                ? $" where {string.Join(" and ", WhereStatements)}"
                : string.Empty;
        }

        protected void AddWhere(string col, object value, bool whereIn)
        {
            var symbol = whereIn ? " in " : "=";
            var stripped = col.StripQuoting();
            WhereStatements.Add($"{col}{symbol}@{stripped}");
            Paras.Add(stripped, value);
        }
    }
}