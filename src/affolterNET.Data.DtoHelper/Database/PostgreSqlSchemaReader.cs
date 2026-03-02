using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using affolterNET.Data.DtoHelper.CodeGen;
using affolterNET.Data.DtoHelper.Dialect;
using Npgsql;

namespace affolterNET.Data.DtoHelper.Database;

public class PostgreSqlSchemaReader : ISchemaReader
{
    private const string TableSql = @"
        SELECT table_schema, table_name, table_type
        FROM information_schema.tables
        WHERE table_schema NOT IN ('pg_catalog', 'information_schema')
        ORDER BY table_schema, table_name";

    private const string ColumnSql = @"
        SELECT
            column_name,
            ordinal_position,
            column_default,
            is_nullable,
            data_type,
            udt_name,
            character_maximum_length,
            is_identity,
            is_generated
        FROM information_schema.columns
        WHERE table_name = @tableName AND table_schema = @schemaName
        ORDER BY ordinal_position ASC";

    private const string PrimaryKeySql = @"
        SELECT kcu.column_name
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu
            ON tc.constraint_name = kcu.constraint_name
            AND tc.table_schema = kcu.table_schema
        WHERE tc.table_name = @tableName
            AND tc.table_schema = @schemaName
            AND tc.constraint_type = 'PRIMARY KEY'
        ORDER BY kcu.ordinal_position";

    private const string ForeignKeysSql = @"
        SELECT
            con.conname AS fk_name,
            att_ref.attname AS referencing_column,
            ref_class.relname AS referenced_table,
            ref_ns.nspname AS referenced_schema,
            att_fk.attname AS referenced_column
        FROM pg_constraint con
        JOIN pg_class cls ON con.conrelid = cls.oid
        JOIN pg_namespace ns ON cls.relnamespace = ns.oid
        JOIN pg_class ref_class ON con.confrelid = ref_class.oid
        JOIN pg_namespace ref_ns ON ref_class.relnamespace = ref_ns.oid
        JOIN pg_attribute att_ref ON att_ref.attrelid = cls.oid AND att_ref.attnum = ANY(con.conkey)
        JOIN pg_attribute att_fk ON att_fk.attrelid = ref_class.oid AND att_fk.attnum = ANY(con.confkey)
        WHERE con.contype = 'f'
            AND ns.nspname = @schemaName
            AND cls.relname = @tableName";

    private const string IncomingForeignKeysSql = @"
        SELECT
            con.conname AS fk_name,
            src_ns.nspname AS referencing_schema,
            src_class.relname AS referencing_table,
            att_src.attname AS referencing_column,
            att_tgt.attname AS referenced_column
        FROM pg_constraint con
        JOIN pg_class src_class ON con.conrelid = src_class.oid
        JOIN pg_namespace src_ns ON src_class.relnamespace = src_ns.oid
        JOIN pg_class tgt_class ON con.confrelid = tgt_class.oid
        JOIN pg_namespace tgt_ns ON tgt_class.relnamespace = tgt_ns.oid
        JOIN pg_attribute att_src ON att_src.attrelid = src_class.oid AND att_src.attnum = ANY(con.conkey)
        JOIN pg_attribute att_tgt ON att_tgt.attrelid = tgt_class.oid AND att_tgt.attnum = ANY(con.confkey)
        WHERE con.contype = 'f'
            AND tgt_ns.nspname = @schemaName
            AND tgt_class.relname = @tableName";

    private readonly TextWriter _tw;

    public PostgreSqlSchemaReader(TextWriter tw)
    {
        _tw = tw;
    }

    public Tables ReadSchema(IDbConnection cn, GeneratorCfg cfg, ISqlDialect dialect)
    {
        var npgsqlCn = (NpgsqlConnection)cn;
        var result = new Tables();

        using (var cmd = new NpgsqlCommand(TableSql, npgsqlCn))
        using (var rdr = cmd.ExecuteReader())
        {
            while (rdr.Read())
            {
                var tableType = rdr["table_type"].ToString()!;
                var isView = string.Equals(tableType, "VIEW", StringComparison.OrdinalIgnoreCase);

                var tbl = new Table(cfg)
                {
                    FullName = rdr["table_schema"] + "_" + rdr["table_name"],
                    Name = rdr["table_name"].ToString()!,
                    Schema = rdr["table_schema"].ToString()!,
                    IsView = isView
                };

                tbl.CleanName = dialect.CleanTableName(tbl.Name);
                tbl.ClassName = tbl.CleanName;
                tbl.ObjectName = RenameSchema(tbl, cfg.RenameTableSchemas);
                result.Add(tbl);
            }
        }

        foreach (var tbl in result)
        {
            LoadColumns(npgsqlCn, tbl, cfg, dialect);
            MarkPrimaryKeys(npgsqlCn, tbl);

            try
            {
                LoadOuterKeys(npgsqlCn, tbl, dialect);
                LoadInnerKeys(npgsqlCn, tbl, dialect);
            }
            catch (Exception x)
            {
                var error = x.Message.Replace("\r\n", "\n").Replace("\n", " ");
                _tw.WriteLine(string.Empty);
                _tw.WriteLine(
                    "// -----------------------------------------------------------------------------------------");
                _tw.WriteLine("// Failed to get relationships for `{0}` - {1}", tbl.Name, error);
                _tw.WriteLine(
                    "// -----------------------------------------------------------------------------------------");
                _tw.WriteLine(string.Empty);
            }
        }

        return result;
    }

    private void LoadColumns(NpgsqlConnection cn, Table tbl, GeneratorCfg cfg, ISqlDialect dialect)
    {
        tbl.AllColumns.Clear();
        using var cmd = new NpgsqlCommand(ColumnSql, cn);
        cmd.Parameters.AddWithValue("@tableName", tbl.Name);
        cmd.Parameters.AddWithValue("@schemaName", tbl.Schema);

        using var rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            var columnName = rdr["column_name"].ToString()!;
            if (cfg.IsColumnExcluded(columnName))
            {
                continue;
            }

            var dataType = rdr["data_type"].ToString()!;
            var udtName = rdr["udt_name"].ToString();
            var maxLength = rdr["character_maximum_length"] as int?;
            var isNullable = rdr["is_nullable"].ToString() == "YES";
            var isIdentity = rdr["is_identity"].ToString() == "YES";
            var columnDefault = rdr["column_default"]?.ToString();

            // serial columns use nextval sequences
            var isSerial = !isIdentity &&
                           columnDefault != null &&
                           columnDefault.StartsWith("nextval", StringComparison.OrdinalIgnoreCase);

            var col = new Column(cfg)
            {
                Name = columnName,
                MaxLength = maxLength
            };

            col.PropertyName = dialect.CleanColumnName(col.Name);
            col.DataType = udtName ?? dataType;
            col.PropertyType = dialect.MapDataType(dataType, udtName);
            col.IsNullable = isNullable;
            col.IsAutoIncrement = isIdentity || isSerial;
            if (col.MaxLength < 1)
            {
                col.MaxLength = null;
            }

            tbl.AllColumns.Add(col);
        }
    }

    private void MarkPrimaryKeys(NpgsqlConnection cn, Table tbl)
    {
        using var cmd = new NpgsqlCommand(PrimaryKeySql, cn);
        cmd.Parameters.AddWithValue("@tableName", tbl.Name);
        cmd.Parameters.AddWithValue("@schemaName", tbl.Schema);

        using var rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            var pkColumnName = rdr.GetString(0);
            var pkColumn = tbl.AllColumns.SingleOrDefault(
                x => string.Equals(x.Name, pkColumnName, StringComparison.OrdinalIgnoreCase));
            if (pkColumn != null)
            {
                pkColumn.IsPK = true;
            }
        }
    }

    private void LoadOuterKeys(NpgsqlConnection cn, Table tbl, ISqlDialect dialect)
    {
        using var cmd = new NpgsqlCommand(ForeignKeysSql, cn);
        cmd.Parameters.AddWithValue("@tableName", tbl.Name);
        cmd.Parameters.AddWithValue("@schemaName", tbl.Schema);

        var result = new List<Key>();
        using (var rdr = cmd.ExecuteReader())
        {
            while (rdr.Read())
            {
                var key = new Key
                {
                    Name = rdr["fk_name"].ToString(),
                    PropertyName = dialect.CleanTableName(rdr["referenced_table"].ToString()!),
                    ReferencedTableName = rdr["referenced_schema"] + "_" + rdr["referenced_table"],
                    ReferencedTableColumnName = rdr["referenced_column"].ToString(),
                    ReferencingTableColumnName = rdr["referencing_column"].ToString()
                };
                result.Add(key);
            }
        }

        FixPropertyNames(result);
        foreach (var key in result)
        {
            tbl.OuterKeys.Add(key);
        }
    }

    private void LoadInnerKeys(NpgsqlConnection cn, Table tbl, ISqlDialect dialect)
    {
        using var cmd = new NpgsqlCommand(IncomingForeignKeysSql, cn);
        cmd.Parameters.AddWithValue("@tableName", tbl.Name);
        cmd.Parameters.AddWithValue("@schemaName", tbl.Schema);

        var result = new List<Key>();
        using (var rdr = cmd.ExecuteReader())
        {
            while (rdr.Read())
            {
                var key = new Key
                {
                    Name = rdr["fk_name"].ToString(),
                    PropertyName = dialect.CleanTableName(rdr["referencing_table"].ToString()!),
                    ReferencingTableName = rdr["referencing_schema"] + "_" + rdr["referencing_table"],
                    ReferencedTableColumnName = rdr["referenced_column"].ToString(),
                    ReferencingTableColumnName = rdr["referencing_column"].ToString()
                };
                result.Add(key);
            }
        }

        FixPropertyNames(result);
        foreach (var key in result)
        {
            tbl.InnerKeys.Add(key);
        }
    }

    private void FixPropertyNames(List<Key> result)
    {
        foreach (var key in result)
        {
            if (result.Count(k => k.PropertyName == key.PropertyName) > 1)
            {
                key.PropertyName = key.Name;
            }
        }
    }

    private string RenameSchema(Table tbl, Dictionary<string, string> schemaRenames)
    {
        var rename = schemaRenames.Where(s => s.Key == tbl.Schema).Select(s => s.Value).SingleOrDefault();
        if (string.IsNullOrWhiteSpace(rename))
        {
            rename = tbl.Schema;
        }

        return $"{rename}_{tbl.Name}";
    }
}
