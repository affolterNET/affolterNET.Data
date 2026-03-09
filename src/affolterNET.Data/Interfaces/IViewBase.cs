using System.Collections.Generic;

namespace affolterNET.Data.Interfaces
{
    /// <summary>
    /// Base interface for database view DTOs. Provides read-only access
    /// (SELECT) to a database table or view.
    /// </summary>
    public interface IViewBase
    {
        /// <summary>
        /// Returns a SELECT statement for this table/view, limited to <paramref name="maxCount"/> rows.
        /// </summary>
        string GetSelectCommand(int maxCount = 1000, params string[] excludedColumns);

        /// <summary>
        /// Returns the database column names for this table/view.
        /// </summary>
        IEnumerable<string> GetColumnNames();

        /// <summary>
        /// Returns the fully qualified table/view name (e.g. "[dbo].[MyTable]" or "\"my_table\"").
        /// </summary>
        string GetTableName();
    }
}
