using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using affolterNET.Data.Result;

namespace affolterNET.Data.Interfaces
{
    /// <summary>
    /// Represents a database query that returns a result of type <typeparamref name="T"/>.
    /// Queries are executed via <see cref="SessionHandler.ISqlSessionHandler"/> which manages
    /// connections, transactions, and error handling.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the query.</typeparam>
    public interface IQuery<T>
    {
        /// <summary>
        /// Gets the parameters dictionary used for parameterized SQL execution.
        /// </summary>
        IDictionary<string, object> ParamsDict { get; }

        /// <summary>
        /// Executes the query synchronously against the given connection and transaction.
        /// </summary>
        DataResult<T> Execute(IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        /// Executes the query asynchronously against the given connection and transaction.
        /// </summary>
        Task<DataResult<T>> ExecuteAsync(IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        /// Gets whether this query should be excluded from history logging.
        /// Read-only queries are typically excluded; write operations are included.
        /// </summary>
        bool ExcludeFromHistory { get; }

        /// <summary>
        /// Gets whether the <see cref="ExcludeFromHistory"/> flag was explicitly set
        /// or should be auto-detected from the namespace (Commands vs Queries).
        /// </summary>
        bool CheckNotExplicitlySetExcludeFromHistory { get; }

        /// <summary>
        /// Gets the SQL statement to be executed.
        /// </summary>
        string? Sql { get;  }

        /// <summary>
        /// Gets the user name associated with this query for audit/history purposes.
        /// </summary>
        string UserName { get; }
    }
}
