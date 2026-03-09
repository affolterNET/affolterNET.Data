using System;
using System.Linq;
using affolterNET.Data.DtoHelper.Database;
using affolterNET.Data.DtoHelper.Dialect;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace affolterNET.Data.DtoHelper.CodeGen
{
    public class ColumnNamesGenerator
    {
        private readonly Table _tbl;
        private readonly ISqlDialect _dialect;

        public ColumnNamesGenerator(Table tbl, ISqlDialect dialect)
        {
            _tbl = tbl;
            _dialect = dialect;
        }

        public void Generate(Action<MemberDeclarationSyntax> add)
        {
            var cols = _tbl.Columns.Select(c => $"public const string {c.PropertyName} = \"{_dialect.FormatColumnNameConstant(c.Name)}\";");
            var allCols = string.Join("\", \"", _tbl.Columns.Select(c => c.Name));
            var sg1 = new StringGenerator(
                $@"
                private static readonly List<string> colNames = new List<string> {{ ""{allCols}"" }};
            ");
            sg1.Generate(add);
            var sg2 = new StringGenerator($@"
                public IEnumerable<string> GetColumnNames() => colNames;
            ");
            sg2.Generate(add);
            var sg3 = new StringGenerator($@"
                public static IEnumerable<string> ColNames => colNames;
            ");
            sg3.Generate(add);
            var sg4 = new StringGenerator($@"
                public static class Cols {{
                    {string.Join(Environment.NewLine, cols)}
                }}
            ");
            sg4.Generate(add);
        }
    }
}
