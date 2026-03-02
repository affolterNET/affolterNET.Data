using System;
using System.Linq;
using affolterNET.Data.DtoHelper.Database;
using affolterNET.Data.DtoHelper.Dialect;
using affolterNET.Data.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace affolterNET.Data.DtoHelper.CodeGen
{
    public class UpdateGenerator
    {
        private readonly Table tbl;
        private readonly ISqlDialect dialect;

        public UpdateGenerator(Table tbl, ISqlDialect dialect)
        {
            this.tbl = tbl;
            this.dialect = dialect;
        }

        public void Generate(Action<MemberDeclarationSyntax> add)
        {
            var updateWhere = string.Empty;
            var versionWhere = string.Empty;
            foreach (var col in tbl.AllColumns.Where(c => !c.Ignore))
            {
                if (col.IsVersionCol())
                {
                    versionWhere = $" and {dialect.QuoteIdentifier(col.Name)}=@{col.PropertyName}";
                }

                if (col.IsPK)
                {
                    updateWhere = $"where {dialect.QuoteIdentifier(col.Name)}=@{col.PropertyName}";
                }
            }

            var columns = tbl.AllColumns
                .Where(
                    c => !c.Ignore && !c.IsPkWithAutoincrement() && !c.IsVersionCol() &&
                         !c.IsInsertTriggerField() && !c.IsActiveCol())
                .Select(c => c.Name).ToList();
            var tableName = dialect.QuoteTableName(tbl.Schema, tbl.Name);
            var quoteStyle = dialect.QuoteStyle;
            var colsJoin = dialect.EscapeForCSharp(columns.JoinCols(false, quoteStyle));
            var escapedTableName = dialect.EscapeForCSharp(tableName);
            var escapedUpdateWhere = dialect.EscapeForCSharp(updateWhere);
            var escapedVersionWhere = dialect.EscapeForCSharp(versionWhere);

            var content = $@"
                var cols = ""{colsJoin}"".GetColumns(affolterNET.Data.Extensions.QuoteStyle.{quoteStyle}, excludedColumns);
                return $""update {escapedTableName} set {{cols.JoinForUpdate(affolterNET.Data.Extensions.QuoteStyle.{quoteStyle})}} {escapedUpdateWhere}{escapedVersionWhere}"";
            ";
            var inner = tbl.IsView
                ? "throw new InvalidOperationException(\"no updates on views\");"
                : content;
            var sg = new StringGenerator(
                $@"
                public string GetUpdateCommand(params string[] excludedColumns)
                {{
                    {inner}
                }}
            ");
            sg.Generate(add);
        }
    }
}
