using System;
using System.Linq;
using affolterNET.Data.DtoHelper.Database;
using affolterNET.Data.DtoHelper.Dialect;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace affolterNET.Data.DtoHelper.CodeGen
{
    public class SaveByIdGenerator
    {
        private readonly Table tbl;
        private readonly ISqlDialect dialect;

        public SaveByIdGenerator(Table tbl, ISqlDialect dialect)
        {
            this.tbl = tbl;
            this.dialect = dialect;
        }

        public void Generate(Action<MemberDeclarationSyntax> add)
        {
            var pk = tbl.AllColumns.FirstOrDefault(t => t.IsPK);
            if (pk == null)
            {
                return;
            }

            var tableName = dialect.QuoteTableName(tbl.Schema, tbl.Name);
            var hasAutoIncrementPk = tbl.GetPrimaryKeyColumn()?.IsAutoIncrement == true;
            var insertReturnId = hasAutoIncrementPk ? "true" : "false";

            var body = dialect.FormatSaveById(
                tableName,
                pk.Name,
                $"GetUpdateCommand(excludedColumns)",
                $"GetInsertCommand({insertReturnId}, excludedColumns)",
                "(select ? GetSelectCommand(1, excludedColumns) : string.Empty)",
                pk.PropertyName!,
                hasAutoIncrementPk,
                true);

            var sg = new StringGenerator(
                $@"
                public string GetSaveByIdCommand(bool select = false, params string[] excludedColumns)
                {{
                    {body}
                }}
            ");
            sg.Generate(add);
        }
    }
}
