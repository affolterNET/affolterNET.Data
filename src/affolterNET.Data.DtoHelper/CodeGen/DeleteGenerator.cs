using System;
using System.Linq;
using affolterNET.Data.DtoHelper.Database;
using affolterNET.Data.DtoHelper.Dialect;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace affolterNET.Data.DtoHelper.CodeGen
{
    public class DeleteGenerator
    {
        private readonly Table tbl;
        private readonly ISqlDialect dialect;

        public DeleteGenerator(Table tbl, ISqlDialect dialect)
        {
            this.tbl = tbl;
            this.dialect = dialect;
        }

        public void Generate(Action<MemberDeclarationSyntax> add)
        {
            var pkCol = tbl.AllColumns.FirstOrDefault(c => c.IsPK);
            var versionCol = tbl.AllColumns.FirstOrDefault(c => c.IsVersionCol());
            var tableName = dialect.QuoteTableName(tbl.Schema, tbl.Name);
            string sql;
            var sqlAll = $"return \"delete from {tableName}";
            if (pkCol == null)
            {
                sql = "throw new InvalidOperationException(\"Kein Primary Key\");";
            }
            else
            {
                var updateWhere = $" where {dialect.QuoteIdentifier(pkCol.Name)}=@{pkCol.PropertyName}";
                var versionWhere = string.Empty;
                if (versionCol != null)
                {
                    versionWhere = $" and {dialect.QuoteIdentifier(versionCol.Name)}=@{versionCol.PropertyName}";
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
