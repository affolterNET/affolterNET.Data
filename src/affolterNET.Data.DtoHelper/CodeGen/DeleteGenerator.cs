using System;
using System.Linq;
using affolterNET.Data.DtoHelper.Database;
using affolterNET.Data.DtoHelper.Dialect;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace affolterNET.Data.DtoHelper.CodeGen
{
    public class DeleteGenerator
    {
        private readonly Table _tbl;
        private readonly ISqlDialect _dialect;

        public DeleteGenerator(Table tbl, ISqlDialect dialect)
        {
            _tbl = tbl;
            _dialect = dialect;
        }

        public void Generate(Action<MemberDeclarationSyntax> add)
        {
            var pkCol = _tbl.AllColumns.FirstOrDefault(c => c.IsPK);
            var versionCol = _tbl.AllColumns.FirstOrDefault(c => c.IsVersionCol());
            var tableName = _dialect.EscapeForCSharp(_dialect.QuoteTableName(_tbl.Schema, _tbl.Name));
            string sql;
            var sqlAll = $"return \"delete from {tableName}";
            if (pkCol == null)
            {
                sql = "throw new InvalidOperationException(\"Kein Primary Key\");";
            }
            else
            {
                var updateWhere = _dialect.EscapeForCSharp($" where {_dialect.QuoteIdentifier(pkCol.Name)}=@{pkCol.PropertyName}");
                var versionWhere = string.Empty;
                if (versionCol != null)
                {
                    versionWhere = _dialect.EscapeForCSharp($" and {_dialect.QuoteIdentifier(versionCol.Name)}=@{versionCol.PropertyName}");
                }

                sql = $"{sqlAll}{updateWhere}{versionWhere}\"";
            }

            var sg = new StringGenerator(
                $@"
                public string GetDeleteCommand() {{
                    {sql};
                }}
            ");

            sg.Generate(add);

            var sgAll = new StringGenerator(
                $@"
                public string GetDeleteAllCommand() {{
                    {sqlAll}"";
                }}
            ");

            sgAll.Generate(add);
        }
    }
}
