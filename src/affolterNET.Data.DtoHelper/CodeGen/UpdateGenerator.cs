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
        private readonly Table _tbl;
        private readonly ISqlDialect _dialect;

        public UpdateGenerator(Table tbl, ISqlDialect dialect)
        {
            _tbl = tbl;
            _dialect = dialect;
        }

        public void Generate(Action<MemberDeclarationSyntax> add)
        {
            var updateWhere = string.Empty;
            var versionWhere = string.Empty;
            foreach (var col in _tbl.AllColumns.Where(c => !c.Ignore))
            {
                if (col.IsVersionCol())
                {
                    versionWhere = $" and {_dialect.QuoteIdentifier(col.Name)}=@{col.PropertyName}";
                }

                if (col.IsPK)
                {
                    updateWhere = $"where {_dialect.QuoteIdentifier(col.Name)}=@{col.PropertyName}";
                }
            }

            var columns = _tbl.AllColumns
                .Where(
                    c => !c.Ignore && !c.IsPkWithAutoincrement() && !c.IsVersionCol() &&
                         !c.IsInsertTriggerField() && !c.IsActiveCol())
                .Select(c => c.Name != c.PropertyName ? $"{c.Name}|{c.PropertyName}" : c.Name).ToList();
            var tableName = _dialect.QuoteTableName(_tbl.Schema, _tbl.Name);
            var quoteStyle = _dialect.QuoteStyle;
            var colsJoin = _dialect.EscapeForCSharp(columns.JoinColsForCodeGen(quoteStyle));
            var escapedTableName = _dialect.EscapeForCSharp(tableName);
            var escapedUpdateWhere = _dialect.EscapeForCSharp(updateWhere);
            var escapedVersionWhere = _dialect.EscapeForCSharp(versionWhere);

            var content = $@"
                var cols = ""{colsJoin}"".GetColumns(affolterNET.Data.Extensions.QuoteStyle.{quoteStyle}, excludedColumns);
                return $""update {escapedTableName} set {{cols.JoinForUpdate(affolterNET.Data.Extensions.QuoteStyle.{quoteStyle})}} {escapedUpdateWhere}{escapedVersionWhere}"";
            ";
            var inner = _tbl.IsView
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
