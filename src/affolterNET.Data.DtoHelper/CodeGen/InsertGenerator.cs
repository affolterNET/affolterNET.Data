using System;
using System.Linq;
using affolterNET.Data.DtoHelper.Database;
using affolterNET.Data.DtoHelper.Dialect;
using affolterNET.Data.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace affolterNET.Data.DtoHelper.CodeGen
{
    public class InsertGenerator
    {
        private readonly Table _tbl;
        private readonly ISqlDialect _dialect;

        public InsertGenerator(Table tbl, ISqlDialect dialect)
        {
            _tbl = tbl;
            _dialect = dialect;
        }

        public void Generate(Action<MemberDeclarationSyntax> add)
        {
            var cols = _tbl.AllColumns
                .Where(
                    c => !c.Ignore && !c.IsPkWithAutoincrement() && !c.IsVersionCol() &&
                         !c.IsUpdateTriggerField(true) && !c.IsActiveCol())
                .Select(c => c.Name != c.PropertyName ? $"{c.Name}|{c.PropertyName}" : c.Name).ToList();
            var tableName = _dialect.QuoteTableName(_tbl.Schema, _tbl.Name);
            var quoteStyle = _dialect.QuoteStyle;
            var colsJoin = _dialect.EscapeForCSharp(cols.JoinColsForCodeGen(quoteStyle));
            var pkCol = _tbl.GetPrimaryKeyColumn();
            var returning = _dialect.EscapeForCSharp(_dialect.FormatInsertReturning(pkCol.Name));
            var escapedTableName = _dialect.EscapeForCSharp(tableName);

            var sg = new StringGenerator(
                $@"
                public string GetInsertCommand(bool returnScopeIdentity = false, params string[] excludedColumns) {{
                    var cols = ""{colsJoin}"".GetColumns(affolterNET.Data.Extensions.QuoteStyle.{quoteStyle}, excludedColumns);
                    var sql = $""insert into {escapedTableName} ({{cols.JoinCols(false, affolterNET.Data.Extensions.QuoteStyle.{quoteStyle})}}) values ({{cols.JoinCols(true, affolterNET.Data.Extensions.QuoteStyle.{quoteStyle})}})"";
                    if (returnScopeIdentity) {{
                        sql += ""{returning}"";
                    }}
                    return sql;
                }}
            ");
            sg.Generate(add);
        }
    }
}
