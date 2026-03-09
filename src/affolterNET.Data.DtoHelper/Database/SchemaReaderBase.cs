using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using affolterNET.Data.DtoHelper.CodeGen;
using affolterNET.Data.DtoHelper.Dialect;

namespace affolterNET.Data.DtoHelper.Database;

public abstract class SchemaReaderBase : ISchemaReader
{
    protected readonly TextWriter Tw;

    protected SchemaReaderBase(TextWriter tw)
    {
        Tw = tw;
    }

    public abstract Tables ReadSchema(IDbConnection connection, GeneratorCfg cfg, ISqlDialect dialect);

    protected void LogRelationshipError(string tableName, Exception ex)
    {
        var error = ex.Message.Replace("\r\n", "\n").Replace("\n", " ");
        Tw.WriteLine(string.Empty);
        Tw.WriteLine(
            "// -----------------------------------------------------------------------------------------");
        Tw.WriteLine("// Failed to get relationships for `{0}` - {1}", tableName, error);
        Tw.WriteLine(
            "// -----------------------------------------------------------------------------------------");
        Tw.WriteLine(string.Empty);
    }

    protected static void FixPropertyNames(List<Key> result)
    {
        foreach (var key in result)
        {
            if (result.Count(k => k.PropertyName == key.PropertyName) > 1)
            {
                key.PropertyName = key.Name;
            }
        }
    }

    protected static string RenameSchema(Table tbl, Dictionary<string, string> schemaRenames)
    {
        var rename = schemaRenames.Where(s => s.Key == tbl.Schema).Select(s => s.Value).SingleOrDefault();
        if (string.IsNullOrWhiteSpace(rename))
        {
            rename = tbl.Schema;
        }

        return $"{rename}_{tbl.Name}";
    }
}
