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
        private readonly Table tbl;
        private readonly ISqlDialect dialect;

        public SelectGenerator(Table tbl, ISqlDialect dialect)
        {
            this.tbl = tbl;
            this.dialect = dialect;
        }

        public void Generate(Action<MemberDeclarationSyntax> add)
        {
            var columns = tbl.AllColumns.Select(c => c.Name);
            var keys = tbl.GetPrimaryKeyColumns().ToList();
            var selectWhere = string.Empty;
            if (keys.Count > 0)
            {
                selectWhere = " where " + string.Join(" and ", keys.Select(WhereStatement));
            }

            var tableName = dialect.QuoteTableName(tbl.Schema, tbl.Name);
            var quoteStyle = dialect.QuoteStyle;
            var colsJoin = columns.JoinCols(false, quoteStyle);
            var selectTemplate = dialect.FormatSelectTop(
                $"{{cols.JoinCols(false, affolterNET.Data.Extensions.QuoteStyle.{quoteStyle})}}",
                tableName,
                selectWhere,
                0);

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
            return $"(@{col.PropertyName} is null or {dialect.QuoteIdentifier(col.Name)}=@{col.PropertyName})";
        }
    }
}
