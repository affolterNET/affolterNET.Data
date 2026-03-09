using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using affolterNET.Data.DtoHelper.Database;
using affolterNET.Data.Extensions;
using Npgsql;

namespace affolterNET.Data.DtoHelper.Dialect;

public class PostgreSqlDialect : ISqlDialect
{
    private static readonly PluralizationServiceWrapper PluralizationService = new();
    private static readonly Regex RxCleanUp = new(@"[^\w\d_]", RegexOptions.Compiled);

    public QuoteStyle QuoteStyle => QuoteStyle.DoubleQuotes;

    public string QuoteIdentifier(string name) => $"\"{name}\"";

    public string QuoteTableName(string schema, string table)
    {
        if (string.Equals(schema, "public", StringComparison.OrdinalIgnoreCase))
        {
            return $"\"{table}\"";
        }

        return $"\"{schema}\".\"{table}\"";
    }

    public string FormatTableNameConstant(string schema, string table)
    {
        if (string.Equals(schema, "public", StringComparison.OrdinalIgnoreCase))
        {
            return table;
        }

        return $"{schema}.{table}";
    }

    public string FormatColumnNameConstant(string dbColumnName) => $"\\\"{dbColumnName}\\\"";

    public string FormatNullableWhereClause(string paramName, string quotedColumnName, string? dataType)
    {
        var pgType = dataType ?? "text";
        return $"(@{paramName}::{pgType} is null or {quotedColumnName}=@{paramName})";
    }

    public string FormatSelectTop(string cols, string tableName, string where, int maxCount)
    {
        return $"select {cols} from {tableName}{where} limit {{maxCount}}";
    }

    public string FormatInsertReturning(string pkColumn)
    {
        return $" returning \"{pkColumn}\" as id";
    }

    public string FormatSaveById(SaveByIdParams p)
    {
        // Extract schema/table for result set
        string schema;
        string table;
        if (p.TableName.Contains("."))
        {
            var parts = p.TableName.Split('.');
            schema = parts[0].Trim('"');
            table = parts[1].Trim('"');
        }
        else
        {
            schema = "public";
            table = p.TableName.Trim('"');
        }

        // PostgreSQL CTE upsert: INSERT must use SELECT syntax (not VALUES) to support WHERE NOT EXISTS.
        // We transform the INSERT command at runtime by replacing "values (" with "select " and removing the trailing ")".
        return $@"
                        var insertSql = {p.InsertCall};
                        var insertAsSelect = insertSql.Replace(""values ("", ""select "").TrimEnd(')');
                        return
                        @$""
                        WITH upsert AS (
                            {{{p.UpdateCall}}} RETURNING *
                        ), inserted AS (
                            {{insertAsSelect}}
                            WHERE NOT EXISTS (SELECT 1 FROM upsert)
                            ON CONFLICT (""""{p.PkColumn}"""") DO NOTHING
                            RETURNING *
                        )
                        SELECT '{schema}' AS """"Schema"""", '{table}' AS """"Table"""",
                            @{p.PkParamName}::text AS """"Id"""",
                            CASE
                                WHEN EXISTS (SELECT 1 FROM upsert) THEN '{{Constants.Updated}}'
                                WHEN EXISTS (SELECT 1 FROM inserted) THEN '{{Constants.Inserted}}'
                                ELSE '{{Constants.NoAction}}'
                            END AS """"Action"""";
                        {{{p.SelectCall}}}"";";
    }

    public string FormatCastToString(string param)
    {
        return $"@{param}::text";
    }

    public string CleanTableName(string name)
    {
        // snake_case → PascalCase + singularize
        var cleaned = SnakeCaseToPascalCase(name);
        return PluralizationService.Singularize(cleaned);
    }

    public string CleanColumnName(string name)
    {
        // snake_case → PascalCase
        return SnakeCaseToPascalCase(name);
    }

    public string MapDataType(string dbType, string? udtName)
    {
        // PostgreSQL reports USER-DEFINED types; use udt_name for those
        var effectiveType = string.Equals(dbType, "USER-DEFINED", StringComparison.OrdinalIgnoreCase)
            ? udtName ?? dbType
            : udtName ?? dbType;

        return effectiveType switch
        {
            "uuid" => "Guid",
            "text" or "varchar" or "bpchar" or "character varying" or "character" => "string",
            "int4" or "integer" => "int",
            "int8" or "bigint" => "long",
            "int2" or "smallint" => "short",
            "bool" or "boolean" => "bool",
            "timestamptz" or "timestamp with time zone" => nameof(DateTimeOffset),
            "timestamp" or "timestamp without time zone" => "DateTime",
            "date" => nameof(DateOnly),
            "numeric" or "decimal" => "decimal",
            "float4" or "real" => "float",
            "float8" or "double precision" => "double",
            "bytea" => "byte[]",
            "jsonb" or "json" or "tsvector" or "xml" or "inet" or "citext" => "string",
            _ => "string"
        };
    }

    public string EscapeForCSharp(string sql) => sql.Replace("\"", "\\\"");

    public IDbConnection CreateConnection(string connString) => new NpgsqlConnection(connString);

    public ISchemaReader CreateSchemaReader(TextWriter tw) => new PostgreSqlSchemaReader(tw);

    private static string SnakeCaseToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return string.Concat(
            input.Split('_', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => char.ToUpper(part[0]) + part.Substring(1).ToLower()));
    }
}
