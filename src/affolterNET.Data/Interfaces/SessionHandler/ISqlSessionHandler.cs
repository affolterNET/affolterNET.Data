using System;
using System.Data;
using System.Threading.Tasks;
using affolterNET.Data.Result;

namespace affolterNET.Data.Interfaces.SessionHandler
{
    /// <summary>
    /// Manages database sessions, transactions, and query/command execution.
    /// Provides automatic connection and transaction lifecycle management
    /// with error handling that wraps results in <see cref="DataResult{T}"/>.
    /// </summary>
    public interface ISqlSessionHandler
    {
        /// <summary>
        /// Executes a command synchronously. Exceptions are caught and returned in the result.
        /// </summary>
        DataResult<int> Execute(ICommand command, IsolationLevel isolationLevel = IsolationLevel.ReadUncommitted);

        /// <summary>
        /// Executes a command asynchronously. Exceptions are caught and returned in the result.
        /// </summary>
        Task<DataResult<int>> ExecuteAsync(
            ICommand command,
            IsolationLevel isolationLevel = IsolationLevel.ReadUncommitted);

        /// <summary>
        /// Executes a query synchronously. Exceptions are caught and returned in the result.
        /// </summary>
        DataResult<TResult> Query<TResult>(
            IQuery<TResult> query,
            IsolationLevel isolationLevel = IsolationLevel.ReadUncommitted);

        /// <summary>
        /// Executes a query asynchronously. Exceptions are caught and returned in the result.
        /// </summary>
        Task<DataResult<TResult>> QueryAsync<TResult>(
            IQuery<TResult> query,
            IsolationLevel isolationLevel = IsolationLevel.ReadUncommitted);

        /// <summary>
        /// Creates a new SQL session with an open connection.
        /// </summary>
        ISqlSession CreateSqlSession();

        /// <summary>
        /// Executes multiple database operations within a single transaction.
        /// Unlike <see cref="QueryAsync{TResult}"/>, exceptions are NOT caught — they propagate to the caller.
        /// </summary>
        Task<DataResult<TResult>> QueryMultipleAsync<TResult>(
            Func<Task<DataResult<TResult>>> dbAction,
            IsolationLevel isolationLevel = IsolationLevel.ReadUncommitted);
    }
}
