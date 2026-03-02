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
                .Where(s => excludedColumns.All(exc => s.ToLower().StripQuoting() != exc.ToLower().StripQuoting()));
            return cols;
        }

        public static string JoinCols(this IEnumerable<string> cols, bool withAdd = false)
        {
            var columns = cols.Select(c => c.Trim());
            if (withAdd)
            {
                columns = columns.Select(c => $"@{c.StripSquareBrackets()}");
            }
            else
            {
                columns = columns.Select(c => c.EnsureSquareBrackets());
            }

            return string.Join(", ", columns);
        }

        public static string JoinCols(this IEnumerable<string> cols, bool withAdd, QuoteStyle style)
        {
            var columns = cols.Select(c => c.Trim());
            if (withAdd)
            {
                columns = columns.Select(c => $"@{c.StripQuoting()}");
            }
            else
            {
                columns = columns.Select(c => c.EnsureQuoting(style));
            }

            return string.Join(", ", columns);
        }

        public static string JoinForUpdate(this IEnumerable<string> cols)
        {
            var columns = cols.Select(c => c.Trim());
            return string.Join(", ", columns.Select(c => $"{c.EnsureSquareBrackets()}=@{c.StripSquareBrackets()}"));
        }

        public static string JoinForUpdate(this IEnumerable<string> cols, QuoteStyle style)
        {
            var columns = cols.Select(c => c.Trim());
            return string.Join(", ", columns.Select(c => $"{c.EnsureQuoting(style)}=@{c.StripQuoting()}"));
        }
    }
}
