using affolterNET.Data.Extensions;

namespace affolterNET.Data.Models.Filters
{
    public class SqlAttribute
    {
        private string? _column;
        private QuoteStyle _quoteStyle = QuoteStyle.Brackets;

        public SqlAttribute() { }

        public SqlAttribute(string column, string prefix = "")
        {
            if (column.Contains("\""))
            {
                _quoteStyle = QuoteStyle.DoubleQuotes;
            }

            Column = column;
            Prefix = prefix;
        }

        public string? Prefix { get; set; }

        public string Column
        {
            get => _column!;
            set => _column = value?.StripQuoting();
        }

        public override string ToString()
        {
            var prefix = string.IsNullOrWhiteSpace(Prefix) ? "" : $"{Prefix}.";
            return $"{prefix}{Column.EnsureQuoting(_quoteStyle)}";
        }

        public string ToSqlParamIdentifier(int index)
        {
            return $"@{ToParam(index)}";
        }

        public string ToParam(int index)
        {
            return $"{Prefix}{index}{Column}";
        }
    }
}