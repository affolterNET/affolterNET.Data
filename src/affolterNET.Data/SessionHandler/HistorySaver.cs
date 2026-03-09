using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using affolterNET.Data.Extensions;
using affolterNET.Data.Interfaces;
using affolterNET.Data.Interfaces.SessionHandler;
using Dapper;
using Serilog;

namespace affolterNET.Data.SessionHandler;

public class HistorySaver : IHistorySaver
{
    private readonly string _connectionString;
    private readonly string _historyTableName;

    public HistorySaver(string connectionString, EnumHistoryMode historyMode = EnumHistoryMode.None, string? historyTableName = null)
    {
        _connectionString = connectionString;
        if (historyMode != EnumHistoryMode.None)
        {
            if (string.IsNullOrWhiteSpace(historyTableName))
            {
                throw new InvalidOperationException(
                    $"please provide a tablename for the history table or set {nameof(HistorySaver)}.{nameof(HistoryMode)} = {nameof(EnumHistoryMode.None)}");
            }
        }

        HistoryMode = historyMode;
        _historyTableName = historyTableName ?? string.Empty;
    }

    /// <summary>
    /// Checks HistoryMode
    /// If HistoryMode == CommandsOnlyAndCheck:
    /// - if query exclusion or inclusion was set using constructor, the check will not run
    /// </summary>
    /// <param name="query"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public async Task<bool> SaveHistory<TResult>(IQuery<TResult> query)
    {
        if (HistoryMode == EnumHistoryMode.None)
        {
            return true;
        }

        if (HistoryMode == EnumHistoryMode.CommandsOnlyAndCheck)
        {
            // check if correctly set
            if (query.CheckNotExplicitlySetExcludeFromHistory)
            {
                CheckHistoryExclusion(query);
            }
        }

        // query.ExcludeFromHistory is explicitly set or will be derived from the query itself In CommandQueryBase.
        if ((HistoryMode == EnumHistoryMode.CommandsOnly || HistoryMode == EnumHistoryMode.CommandsOnlyAndCheck) && query.ExcludeFromHistory)
        {
            return true;
        }

        var name = query.GetType().GetGenericArgsFriendlyName();
        return await SaveHistory(name, query.ToString()!, query.UserName, query.ExcludeFromHistory ? "read" : "write");
    }

    /// <summary>
    /// if HistoryMode != None, all commands and queries will be saved
    /// </summary>
    /// <param name="name"></param>
    /// <param name="query"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<bool> SaveHistory(string name, string query, string user, string access)
    {
        if (HistoryMode == EnumHistoryMode.None)
        {
            return true;
        }

        var ok = await Insert(name, query, user, access);
        if (!ok)
        {
            var tok = await CreateTable();
            if (tok)
            {
                ok = await Insert(name, query, user, access);
                return ok;
            }
        }

        return false;
    }

    public EnumHistoryMode HistoryMode { get; }
    
    public async Task<bool> Insert(string name, string query, string user, string access)
    {
        try
        {
            var connection = new SqlConnection(_connectionString);
            var sql =
                $"insert into {_historyTableName} (Name, Script, Applied, UserName, Access) values (@Name, @Script, getutcdate(), @UserName, @Access)";
            var ok = await connection.ExecuteAsync(
                sql,
                new { Name = name, Script = query, UserName = user, Access = access }).ConfigureAwait(false);
            await connection.CloseAsync();
            await connection.DisposeAsync();
            if (ok != 1)
            {
                Log.Warning("SaveHistory failed");
            }

            return true;
        }
        catch (SqlException ex)
        {
            if (ex.Message == $"Invalid object name '{_historyTableName}'.")
            {
                return false;
            }

            throw;
        }
        catch
        {
            Log.Error("Table creation error");
            throw;
        }
    }

    public async Task<bool> CreateTable()
    {
        Log.Information("Creating history table {TableName}", _historyTableName);
        try
        {
            var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                $"create table {_historyTableName} (Id int identity not null primary key, Name nvarchar(2000) not null, Script nvarchar(max) not null, Applied datetime2 not null, UserName nvarchar(200) null, Access nvarchar(20) null)");
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void CheckHistoryExclusion<TResult>(IQuery<TResult> query)
    {
        bool containsWriteOperations = false;
        var lines = query.Sql?.Split(Environment.NewLine) ?? Array.Empty<string>();
        foreach (var line in lines)
        {
            var l = line.Trim().ToLower();
            var sc = StringComparison.CurrentCultureIgnoreCase;
            if (l.StartsWith("--"))
            {
                // ignore comments
                continue;
            }

            if (l.IndexOf("insert", sc) > -1 || l.IndexOf("update", sc) > -1 || l.IndexOf("delete", sc) > -1)
            {
                containsWriteOperations = true;
                break;
            }
        }

        if (query.ExcludeFromHistory && containsWriteOperations)
        {
            Log.Debug("{QueryType} contains insert, update or delete but will not be saved for history",
                query.GetType().FullName);
        }

        if (!query.ExcludeFromHistory && !containsWriteOperations)
        {
            Log.Debug("{QueryType} contains no write operations but will be saved for history",
                query.GetType().FullName);
        }
    }
}