using System;
using System.Linq;
using System.Text;
using affolterNET.Data.DtoHelper.Database;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace affolterNET.Data.DtoHelper.CodeGen
{
    public class RefreshGenerator
    {
        private readonly Table _tbl;

        public RefreshGenerator(Table tbl)
        {
            _tbl = tbl;
        }

        public void Generate(Action<MemberDeclarationSyntax> add)
        {
            var columnsBuilder = new StringBuilder();
            foreach (var c in _tbl.Columns)
            {
                if (c.IsPK)
                {
                    continue;
                }

                columnsBuilder.Append($"this.{c.PropertyName} = loaded.{c.PropertyName};");
            }
            var columns = columnsBuilder.ToString();

            var sgGetFromDb = new StringGenerator(
                $@"
                public {_tbl.ObjectName}? GetFromDb(IDbConnection connection, IDbTransaction transaction) {{
                    return connection.QueryFirstOrDefault<{_tbl.ObjectName}>(this.GetSelectCommand(1), this, transaction);
                }}

            ");
            sgGetFromDb.Generate(add);

            var sgReload = new StringGenerator(
                $@"
                public void Reload(IDbConnection connection, IDbTransaction transaction) {{
                    var loaded = this.GetFromDb(connection, transaction);
                    if (loaded == null) {{ throw new InvalidOperationException(""entity not found""); }}
                    {columns}
                }}
            ");

            sgReload.Generate(add);
        }
    }
}
