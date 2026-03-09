using System;

namespace affolterNET.Data;

public class ConcurrencyException : InvalidOperationException
{
    public ConcurrencyException(string schema, string table, string? id)
        : base($"Concurrency conflict on {schema}.{table} (Id: {id}): the row was modified by another transaction")
    {
        Schema = schema;
        Table = table;
        Id = id;
    }

    public string Schema { get; }
    public string Table { get; }
    public string? Id { get; }
}
