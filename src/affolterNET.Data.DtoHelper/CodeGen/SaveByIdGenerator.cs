using System;
using System.Linq;
using affolterNET.Data.DtoHelper.Database;
using affolterNET.Data.DtoHelper.Dialect;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace affolterNET.Data.DtoHelper.CodeGen
{
    public class SaveByIdGenerator
    {
        private readonly Table _tbl;
        private readonly ISqlDialect _dialect;

        public SaveByIdGenerator(Table tbl, ISqlDialect dialect)
        {
            _tbl = tbl;
            _dialect = dialect;
        }

        public void Generate(Action<MemberDeclarationSyntax> add)
        {
            var pk = _tbl.AllColumns.FirstOrDefault(t => t.IsPK);
            if (pk == null)
            {
                return;
            }

            var tableName = _dialect.QuoteTableName(_tbl.Schema, _tbl.Name);
            var hasAutoIncrementPk = _tbl.GetPrimaryKeyColumn()?.IsAutoIncrement == true;
            var insertReturnId = hasAutoIncrementPk ? "true" : "false";

            var body = _dialect.FormatSaveById(new SaveByIdParams(
                tableName,
                pk.Name,
                $"GetUpdateCommand(excludedColumns)",
                $"GetInsertCommand({insertReturnId}, excludedColumns)",
                "(select ? GetSelectCommand(1, excludedColumns) : string.Empty)",
                pk.PropertyName!,
                hasAutoIncrementPk,
                true));

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
