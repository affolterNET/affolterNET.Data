using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using affolterNET.Data.Extensions;
using affolterNET.Data.Interfaces;
using Dapper;

namespace affolterNET.Data.TestHelpers.Builders
{
    public class UpdateBuilder<T> : CrudBase<T> where T : IDtoBase
    {
        private readonly IList<string> _updateStatements = new List<string>();

        private string sql = string.Empty;

        public UpdateBuilder(IDbConnection connection, IDbTransaction transaction, IDtoBase dto)
            : base(connection, transaction, dto)
        {
            if (dto.GetUpdatedUserName() != Constants.NotAvailable)
            {
                AddUpdate(dto.GetUpdatedDateName(), DateTime.Now);
                AddUpdate(dto.GetUpdatedUserName(), $"UpdateBuilder: {dto.GetTableName()}");
            }
        }

        public UpdateBuilder<T> WithUpdate(string col, object value)
        {
            AddUpdate(col, value);
            return this;
        }

        public UpdateBuilder<T> WithWhere(string col, object value, bool whereIn = false)
        {
            AddWhere(col, value, whereIn);
            return this;
        }

        public UpdateBuilder<T> WithWhereIn(string col, object values)
        {
            return WithWhere(col, values, true);
        }

        public int Execute()
        {
            sql = $"update {TableName}";
            if (_updateStatements.Count > 0)
            {
                sql += $" set {string.Join(", ", _updateStatements)}";
            }

            sql += BuildWhereClause();

            return Connection.Execute(sql, Paras, Transaction);
        }

        private void AddUpdate(string col, object value)
        {
            var stripped = col.StripQuoting();
            var columnRef = IsPostgres
                ? PascalCaseToQuotedSnakeCase(stripped)
                : stripped;
            var paramName = $"upd_{stripped}";
            _updateStatements.Add($"{columnRef}=@{paramName}");
            Paras.Add(paramName, value);
        }

        private static string PascalCaseToQuotedSnakeCase(string name)
        {
            if (name.Contains("_")) return $"\"{name}\"";
            var snake = Regex.Replace(name, "([A-Z])", "_$1").TrimStart('_').ToLower();
            return $"\"{snake}\"";
        }
    }
}