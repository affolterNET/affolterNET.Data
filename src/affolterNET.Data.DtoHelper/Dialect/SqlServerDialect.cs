using System;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using affolterNET.Data.DtoHelper.Database;
using affolterNET.Data.Extensions;
using Microsoft.Data.SqlClient;

namespace affolterNET.Data.DtoHelper.Dialect;

public class SqlServerDialect : ISqlDialect
{
    private static readonly PluralizationServiceWrapper PluralizationService = new();
    private static readonly Regex RxCleanUp = new(@"[^\w\d_]", RegexOptions.Compiled);

    public QuoteStyle QuoteStyle => QuoteStyle.Brackets;

    public string QuoteIdentifier(string name) => $"[{name}]";

    public string QuoteTableName(string schema, string table) => $"[{schema}].[{table}]";

    public string FormatTableNameConstant(string schema, string table) => $"[{schema}].[{table}]";

    public string FormatColumnNameConstant(string dbColumnName) => $"[{dbColumnName}]";

    public string FormatSelectTop(string cols, string tableName, string where, int maxCount)
    {
        return $"select top({{maxCount}}) {cols} from {tableName}{where}";
    }

    public string FormatInsertReturning(string pkColumn)
    {
        return "; select scope_identity() as id;";
    }

    public string FormatSaveById(
        string tableName,
        string pkColumn,
        string updateCall,
        string insertCall,
        string selectCall,
        string pkParamName,
        bool hasAutoIncrementPk,
        bool hasSelect)
    {
        var schema = tableName.Contains(".") ? tableName.Split('.')[0].Trim('[', ']') : "";
        var table = tableName.Contains(".") ? tableName.Split('.')[1].Trim('[', ']') : tableName.Trim('[', ']');

        return $@"return
                        @$""
                        declare @rowcnt int
                        if exists (select {pkColumn} from {tableName} where {pkColumn} = @{pkParamName})
                            begin
                                {{{updateCall}}}; set @rowcnt = (select @@rowcount);
                                select '{schema}' as [Schema], '{table}' as [Table], convert(nvarchar(50), @{pkParamName}) as [Id], case when @rowcnt = 0 then '{{Constants.NoAction}}' else '{{Constants.Updated}}' end as [Action];
                            end
                        else
                            begin
                                {{{insertCall}}}; set @rowcnt = (select @@rowcount);
                                select '{schema}' as [Schema], '{table}' as [Table], convert(nvarchar(50), @{pkParamName}) as [Id], case when @rowcnt = 0 then '{{Constants.NoAction}}' else '{{Constants.Inserted}}' end as [Action];
                            end
                        {{{selectCall}}}"";";
    }

    public string FormatCastToString(string param)
    {
        return $"convert(nvarchar(50), @{param})";
    }

    public string CleanTableName(string name)
    {
        var cleanName = RxCleanUp.Replace(name, "_");
        if (char.IsDigit(cleanName[0]))
        {
            cleanName = "_" + cleanName;
        }

        if (cleanName.StartsWith("tbl_"))
        {
            cleanName = cleanName.Replace("tbl_", string.Empty);
        }

        if (cleanName.StartsWith("tbl"))
        {
            cleanName = cleanName.Replace("tbl", string.Empty);
        }

        cleanName = cleanName.Replace("_", string.Empty);
        return PluralizationService.Singularize(cleanName);
    }

    public string CleanColumnName(string name)
    {
        var cleaned = RxCleanUp.Replace(name, "_");
        if (char.IsDigit(cleaned[0]))
        {
            cleaned = "_" + cleaned;
        }

        return cleaned;
    }

    public string MapDataType(string dbType, string? udtName)
    {
        return dbType switch
        {
            "bigint" => "long",
            "smallint" => "short",
            "int" => "int",
            "uniqueidentifier" => "Guid",
            "date" => nameof(DateOnly),
            "smalldatetime" or "datetime" or "datetime2" or "time" => "DateTime",
            "datetimeoffset" => nameof(DateTimeOffset),
            "float" => "double",
            "real" => "float",
            "numeric" or "smallmoney" or "decimal" or "money" => "decimal",
            "tinyint" => "byte",
            "bit" => "bool",
            "image" or "binary" or "varbinary" or "timestamp" => "byte[]",
            "geography" => "Microsoft.SqlServer.Types.SqlGeography",
            "geometry" => "Microsoft.SqlServer.Types.SqlGeometry",
            _ => "string"
        };
    }

    public string EscapeForCSharp(string sql) => sql;

    public IDbConnection CreateConnection(string connString) => new SqlConnection(connString);

    public ISchemaReader CreateSchemaReader(TextWriter tw) => new SqlServerSchemaReader(tw);
}
