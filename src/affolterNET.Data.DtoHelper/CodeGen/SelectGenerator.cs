using System;
using System.Linq;
using affolterNET.Data.DtoHelper.Database;
using affolterNET.Data.DtoHelper.Dialect;
using affolterNET.Data.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace affolterNET.Data.DtoHelper.CodeGen
{
    public class SelectGenerator
    {
        private readonly Table _tbl;
        private readonly ISqlDialect _dialect;

        public SelectGenerator(Table tbl, ISqlDialect dialect)
        {
            _tbl = tbl;
            _dialect = dialect;
        }

        public void Generate(Action<MemberDeclarationSyntax> add)
        {
            var columns = _tbl.AllColumns.Select(c => c.Name != c.PropertyName ? $"{c.Name}|{c.PropertyName}" : c.Name);
            var keys = _tbl.GetPrimaryKeyColumns().ToList();
            var selectWhere = string.Empty;
            if (keys.Count > 0)
            {
                selectWhere = " where " + string.Join(" and ", keys.Select(WhereStatement));
            }

            var tableName = _dialect.QuoteTableName(_tbl.Schema, _tbl.Name);
            var quoteStyle = _dialect.QuoteStyle;
            var colsJoin = _dialect.EscapeForCSharp(columns.JoinColsForCodeGen(quoteStyle));
            var selectTemplate = _dialect.EscapeForCSharp(_dialect.FormatSelectTop(
                $"{{cols.JoinColsForSelect(affolterNET.Data.Extensions.QuoteStyle.{quoteStyle})}}",
                tableName,
                selectWhere,
                0));

            var sgSelect = new StringGenerator(
                $@"
                public string GetSelectCommand(int maxCount = 1000, params string[] excludedColumns)
                {{
                    var cols = ""{colsJoin}"".GetColumns(affolterNET.Data.Extensions.QuoteStyle.{quoteStyle}, excludedColumns);
                    return $""{selectTemplate}"";
                }}");
            sgSelect.Generate(add);
        }

        private string WhereStatement(Column col)
        {
            return _dialect.FormatNullableWhereClause(col.PropertyName!, _dialect.QuoteIdentifier(col.Name), col.DataType);
        }
    }
}
