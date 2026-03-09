using System;
using System.Collections.Generic;
using System.Data;

namespace affolterNET.Data.Interfaces
{
    /// <summary>
    /// Base interface for database table DTOs. Extends <see cref="IViewBase"/> with
    /// write operations (INSERT, UPDATE, DELETE) and metadata accessors for
    /// audit fields, versioning, and soft-delete support.
    /// </summary>
    public interface IDtoBase: IViewBase
    {
        /// <summary>
        /// Returns an INSERT statement. When <paramref name="returnScopeIdentity"/> is true,
        /// appends a clause to return the generated identity value.
        /// </summary>
        string GetInsertCommand(bool returnScopeIdentity = false, params string[] excludedColumns);

        /// <summary>
        /// Returns an UPDATE statement with a WHERE clause on the primary key.
        /// </summary>
        string GetUpdateCommand(params string[] excludedColumns);

        /// <summary>
        /// Returns a DELETE statement with a WHERE clause on the primary key.
        /// </summary>
        string GetDeleteCommand();

        /// <summary>
        /// Returns a DELETE statement without a WHERE clause (deletes all rows).
        /// </summary>
        string GetDeleteAllCommand();

        /// <summary>
        /// Returns an upsert (INSERT or UPDATE) statement that checks for existing rows by primary key.
        /// </summary>
        string GetSaveByIdCommand(bool select = false, params string[] excludedColumns);

        /// <summary>
        /// Returns true if the primary key is auto-incremented (identity/serial).
        /// </summary>
        bool IsAutoincrementId();

        /// <summary>
        /// Sets the primary key value on this DTO instance.
        /// </summary>
        void SetId(object id);

        /// <summary>
        /// Returns the property name of the primary key column.
        /// </summary>
        string GetIdName();

        /// <summary>
        /// Returns the property name of the "updated by" audit column, or <see cref="Constants.NotAvailable"/>.
        /// </summary>
        string GetUpdatedUserName();

        /// <summary>
        /// Returns the property name of the "inserted by" audit column, or <see cref="Constants.NotAvailable"/>.
        /// </summary>
        string GetInsertedUserName();

        /// <summary>
        /// Returns the property name of the "updated date" audit column, or <see cref="Constants.NotAvailable"/>.
        /// </summary>
        string GetUpdatedDateName();

        /// <summary>
        /// Returns the property name of the "inserted date" audit column, or <see cref="Constants.NotAvailable"/>.
        /// </summary>
        string GetInsertedDateName();

        /// <summary>
        /// Returns the property name of the soft-delete "is active" column, or <see cref="Constants.NotAvailable"/>.
        /// </summary>
        string GetIsActiveName();

        /// <summary>
        /// Returns the property name of the optimistic concurrency version column, or <see cref="Constants.NotAvailable"/>.
        /// </summary>
        string GetVersionName();

        /// <summary>
        /// Sets the "inserted by" audit field value.
        /// </summary>
        void SetInsertedUser(string userName);

        /// <summary>
        /// Sets the "inserted date" audit field value.
        /// </summary>
        void SetInsertedDate(DateTime date);

        /// <summary>
        /// Sets the "updated by" audit field value.
        /// </summary>
        void SetUpdatedUser(string userName);

        /// <summary>
        /// Sets the "updated date" audit field value.
        /// </summary>
        void SetUpdatedDate(DateTime date);

        /// <summary>
        /// Reloads this DTO from the database using the current primary key value.
        /// </summary>
        void Reload(IDbConnection connection, IDbTransaction transaction);
    }
}
