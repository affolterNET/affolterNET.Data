using System.Data;
using System.IO;
using affolterNET.Data.DtoHelper.Database;
using affolterNET.Data.Extensions;

namespace affolterNET.Data.DtoHelper.Dialect;

public interface ISqlDialect
{
    QuoteStyle QuoteStyle { get; }

    // Quoting
    string QuoteIdentifier(string name);
    string QuoteTableName(string schema, string table);
    string EscapeForCSharp(string sql);
    string FormatTableNameConstant(string schema, string table);
    string FormatColumnNameConstant(string dbColumnName);

    // SQL generation
    string FormatNullableWhereClause(string paramName, string quotedColumnName, string? dataType);
    string FormatSelectTop(string cols, string tableName, string where, int maxCount);
    string FormatInsertReturning(string pkColumn);
    string FormatSaveById(SaveByIdParams p);
    string FormatCastToString(string param);

    // Name cleaning
    string CleanTableName(string name);
    string CleanColumnName(string name);

    // Type mapping
    string MapDataType(string dbType, string? udtName);

    // Factories
    IDbConnection CreateConnection(string connString);
    ISchemaReader CreateSchemaReader(TextWriter tw);
}
