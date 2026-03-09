using System;
using System.Collections.Generic;
using System.Linq;
using affolterNET.Data.Interfaces;

namespace affolterNET.Data.Extensions
{
    public static class DtoBaseExtensions
    {
        public static T GetId<T>(this IDtoBase dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Dto cannot be null");
            }

            var idName = dto.GetIdName();
            var idProp = dto.GetType().GetProperty(idName);
            if (idProp == null || idProp.GetMethod == null)
            {
                throw new InvalidOperationException("Invalid Id Prop on Dto");
            }

            var id = idProp.GetMethod.Invoke(dto, new object[] { });
            if (!(id is T))
            {
                throw new InvalidOperationException("id was null or not a guid");
            }

            return (T)id;
        }

        public static string? GetString(this IDtoBase dto, string propertyname)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Dto cannot be null");
            }

            if (string.IsNullOrWhiteSpace(propertyname))
            {
                throw new ArgumentNullException(nameof(propertyname), "Propertyname cannot be empty");
            }

            var idProp = dto.GetType().GetProperty(propertyname);
            if (idProp == null || idProp.GetMethod == null)
            {
                throw new InvalidOperationException("Invalid Id Prop on Dto");
            }

            var id = idProp.GetMethod.Invoke(dto, new object[] { });
            return id?.ToString();
        }

        public static IEnumerable<string> GetColumns(this string columnsString, params string[] excludedColumns) {
            var cols = columnsString
                .Split(",")
                .Select(s => s.Trim())
                .Where(s => excludedColumns.All(exc => s.ToLower().StripSquareBrackets() != exc.ToLower().StripSquareBrackets()));
            return cols;
        }

        public static IEnumerable<string> GetColumns(this string columnsString, QuoteStyle style, params string[] excludedColumns)
        {
            var cols = columnsString
                .Split(",")
                .Select(s => s.Trim())
                .Where(s =>
                {
                    var colPart = s.GetColumnPart();
                    return excludedColumns.All(exc => colPart.ToLower().StripQuoting() != exc.ToLower().StripQuoting());
                });
            return cols;
        }

        public static string JoinCols(this IEnumerable<string> cols, bool withAdd = false)
        {
            var columns = cols.Select(c => c.Trim());
            if (withAdd)
            {
                columns = columns.Select(c => $"@{c.GetParamName(c.StripSquareBrackets())}");
            }
            else
            {
                columns = columns.Select(c => c.GetColumnPart().EnsureSquareBrackets());
            }

            return string.Join(", ", columns);
        }

        public static string JoinCols(this IEnumerable<string> cols, bool withAdd, QuoteStyle style)
        {
            var columns = cols.Select(c => c.Trim());
            if (withAdd)
            {
                columns = columns.Select(c => $"@{c.GetParamName(c.GetColumnPart().StripQuoting())}");
            }
            else
            {
                columns = columns.Select(c => c.GetColumnPart().EnsureQuoting(style));
            }

            return string.Join(", ", columns);
        }

        public static string JoinForUpdate(this IEnumerable<string> cols)
        {
            var columns = cols.Select(c => c.Trim());
            return string.Join(", ", columns.Select(c =>
            {
                var colPart = c.GetColumnPart();
                var paramName = c.GetParamName(colPart.StripSquareBrackets());
                return $"{colPart.EnsureSquareBrackets()}=@{paramName}";
            }));
        }

        public static string JoinForUpdate(this IEnumerable<string> cols, QuoteStyle style)
        {
            var columns = cols.Select(c => c.Trim());
            return string.Join(", ", columns.Select(c =>
            {
                var colPart = c.GetColumnPart();
                var paramName = c.GetParamName(colPart.StripQuoting());
                return $"{colPart.EnsureQuoting(style)}=@{paramName}";
            }));
        }

        /// <summary>
        /// Joins columns for SELECT, adding aliases when column name differs from property name.
        /// Handles "col"|PropName format to produce "col" AS "PropName" when needed.
        /// </summary>
        public static string JoinColsForSelect(this IEnumerable<string> cols, QuoteStyle style)
        {
            var columns = cols.Select(c =>
            {
                var trimmed = c.Trim();
                var pipeIdx = trimmed.IndexOf('|');
                var colPart = pipeIdx >= 0 ? trimmed.Substring(0, pipeIdx) : trimmed;
                var quoted = colPart.EnsureQuoting(style);
                if (pipeIdx >= 0)
                {
                    var propName = trimmed.Substring(pipeIdx + 1);
                    var colName = colPart.StripQuoting();
                    if (!string.Equals(colName, propName, StringComparison.OrdinalIgnoreCase))
                    {
                        return $"{quoted} AS {propName.EnsureQuoting(style)}";
                    }
                }
                return quoted;
            });
            return string.Join(", ", columns);
        }

        /// <summary>
        /// Joins column entries preserving property name mappings (col|PropName format).
        /// Used at code generation time to produce column strings for embedding in generated code.
        /// </summary>
        public static string JoinColsForCodeGen(this IEnumerable<string> cols, QuoteStyle style)
        {
            var columns = cols.Select(c =>
            {
                var trimmed = c.Trim();
                var pipeIdx = trimmed.IndexOf('|');
                var colName = pipeIdx >= 0 ? trimmed.Substring(0, pipeIdx) : trimmed;
                var quoted = colName.EnsureQuoting(style);
                return pipeIdx >= 0 ? $"{quoted}|{trimmed.Substring(pipeIdx + 1)}" : quoted;
            });
            return string.Join(", ", columns);
        }

        private static string GetColumnPart(this string col)
        {
            var pipeIdx = col.IndexOf('|');
            return pipeIdx >= 0 ? col.Substring(0, pipeIdx) : col;
        }

        private static string GetParamName(this string col, string fallback)
        {
            var pipeIdx = col.IndexOf('|');
            return pipeIdx >= 0 ? col.Substring(pipeIdx + 1) : fallback;
        }
    }
}
