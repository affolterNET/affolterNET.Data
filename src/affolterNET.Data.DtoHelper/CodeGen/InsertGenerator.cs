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
        private readonly Table tbl;
        private readonly ISqlDialect dialect;

        public InsertGenerator(Table tbl, ISqlDialect dialect)
        {
            this.tbl = tbl;
            this.dialect = dialect;
        }

        public void Generate(Action<MemberDeclarationSyntax> add)
        {
            var cols = tbl.AllColumns
                .Where(
                    c => !c.Ignore && !c.IsPkWithAutoincrement() && !c.IsVersionCol() &&
                         !c.IsUpdateTriggerField(true) && !c.IsActiveCol())
                .Select(c => c.Name).ToList();
            var tableName = dialect.QuoteTableName(tbl.Schema, tbl.Name);
            var quoteStyle = dialect.QuoteStyle;
            var colsJoin = cols.JoinCols(false, quoteStyle);
            var pkCol = tbl.GetPrimaryKeyColumn();
            var returning = dialect.FormatInsertReturning(pkCol.Name);

            var sg = new StringGenerator(
                $@"
                public string GetInsertCommand(bool returnScopeIdentity = false, params string[] excludedColumns) {{
                    var cols = ""{colsJoin}"".GetColumns(affolterNET.Data.Extensions.QuoteStyle.{quoteStyle}, excludedColumns);
                    var sql = $""insert into {tableName} ({{cols.JoinCols(false, affolterNET.Data.Extensions.QuoteStyle.{quoteStyle})}}) values ({{cols.JoinCols(true, affolterNET.Data.Extensions.QuoteStyle.{quoteStyle})}})"";
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
