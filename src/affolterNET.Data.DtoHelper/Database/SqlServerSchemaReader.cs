using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using affolterNET.Data.DtoHelper.CodeGen;
using affolterNET.Data.DtoHelper.Dialect;

namespace affolterNET.Data.DtoHelper.Database
{
    public class SqlServerSchemaReader : ISchemaReader
    {
        private const string TableSql = @"SELECT *
		FROM  INFORMATION_SCHEMA.TABLES
		WHERE TABLE_TYPE='BASE TABLE' OR TABLE_TYPE='VIEW'
        ORDER BY TABLE_SCHEMA, TABLE_NAME";

        private const string ColumnSql = @"SELECT
			TABLE_CATALOG AS [Database],
			TABLE_SCHEMA AS Owner,
			TABLE_NAME AS TableName,
			COLUMN_NAME AS ColumnName,
			ORDINAL_POSITION AS OrdinalPosition,
			COLUMN_DEFAULT AS DefaultSetting,
			IS_NULLABLE AS IsNullable, DATA_TYPE AS DataType,
			CHARACTER_MAXIMUM_LENGTH AS MaxLength,
			DATETIME_PRECISION AS DatePrecision,
			COLUMNPROPERTY(object_id('[' + TABLE_SCHEMA + '].[' + TABLE_NAME + ']'), COLUMN_NAME, 'IsIdentity') AS IsIdentity,
			COLUMNPROPERTY(object_id('[' + TABLE_SCHEMA + '].[' + TABLE_NAME + ']'), COLUMN_NAME, 'IsComputed') as IsComputed
		FROM  INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME=@tableName AND TABLE_SCHEMA=@schemaName
		ORDER BY OrdinalPosition ASC";

        private const string GetPkSql = @"SELECT COLUMN_NAME as ColumnName
            FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
            JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu ON tc.CONSTRAINT_NAME = ccu.Constraint_name
            WHERE tc.TABLE_NAME = @tableName and tc.CONSTRAINT_TYPE = 'Primary Key'";

        private const string OuterKeysSql = @"SELECT
			FK = OBJECT_NAME(pt.constraint_object_id),
            Referenced_schema = OBJECT_SCHEMA_NAME(pt.referenced_object_id),
			Referenced_tbl = OBJECT_NAME(pt.referenced_object_id),
			Referencing_col = pc.name,
			Referenced_col = rc.name
		FROM sys.foreign_key_columns AS pt
		INNER JOIN sys.columns AS pc
		ON pt.parent_object_id = pc.[object_id]
		AND pt.parent_column_id = pc.column_id
		INNER JOIN sys.columns AS rc
		ON pt.referenced_column_id = rc.column_id
		AND pt.referenced_object_id = rc.[object_id]
		WHERE pt.parent_object_id = OBJECT_ID(@tableName)
        Order By Referenced_tbl;";

        private const string InnerKeysSql = @"SELECT
			[Schema] = OBJECT_SCHEMA_NAME(pt.parent_object_id),
			Referencing_tbl = OBJECT_NAME(pt.parent_object_id),
			FK = OBJECT_NAME(pt.constraint_object_id),
			Referencing_col = pc.name,
			Referenced_col = rc.name
		FROM sys.foreign_key_columns AS pt
		INNER JOIN sys.columns AS pc
		ON pt.parent_object_id = pc.[object_id]
		AND pt.parent_column_id = pc.column_id
		INNER JOIN sys.columns AS rc
		ON pt.referenced_column_id = rc.column_id
		AND pt.referenced_object_id = rc.[object_id]
		WHERE pt.referenced_object_id = OBJECT_ID(@tableName);";

        // SchemaReader.ReadSchema
        private readonly TextWriter tw;

        private SqlConnection? connection;

        public SqlServerSchemaReader(TextWriter tw)
        {
            this.tw = tw;
        }

        public Tables ReadSchema(IDbConnection cn, GeneratorCfg cfg, ISqlDialect dialect)
        {
            var sqlCn = (SqlConnection)cn;
            return ReadSchema(sqlCn, cfg, dialect);
        }

        private Tables ReadSchema(SqlConnection cn, GeneratorCfg cfg, ISqlDialect dialect)
        {
            var result = new Tables();

            connection = cn;

            var cmd = new SqlCommand
            {
                Connection = cn,
                CommandText = TableSql
            };

            // pull the tables in a reader
            using (cmd)
            {
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var tbl = new Table(cfg)
                        {
                            FullName = rdr["TABLE_SCHEMA"] + "_" + rdr["TABLE_NAME"],
                            Name = rdr["TABLE_NAME"].ToString()!,
                            Schema = rdr["TABLE_SCHEMA"].ToString()!,
                            IsView = string.Compare(
                                         rdr["TABLE_TYPE"].ToString(),
                                         "View",
                                         StringComparison.OrdinalIgnoreCase) == 0
                        };
                        tbl.CleanName = dialect.CleanTableName(tbl.Name);
                        tbl.ClassName = tbl.CleanName;
                        tbl.ObjectName = RenameSchema(tbl, cfg.RenameTableSchemas);
                        result.Add(tbl);
                    }
                }
            }

            foreach (var tbl in result)
            {
                LoadColumns(cn, tbl, cfg, dialect);
                MarkPrimaryKeys(cn, tbl);

                try
                {
                    LoadOuterKeys(tbl);
                    LoadInnerKeys(tbl);
                }
                catch (Exception x)
                {
                    var error = x.Message.Replace("\r\n", "\n").Replace("\n", " ");
                    tw.WriteLine(string.Empty);
                    tw.WriteLine(
                        "// -----------------------------------------------------------------------------------------");
                    tw.WriteLine("// Failed to get relationships for `{0}` - {1}", tbl.Name, error);
                    tw.WriteLine(
                        "// -----------------------------------------------------------------------------------------");
                    tw.WriteLine(string.Empty);
                }
            }

            return result;
        }

        private void LoadColumns(SqlConnection cn, Table tbl, GeneratorCfg cfg, ISqlDialect dialect)
        {
            tbl.AllColumns.Clear();
            using var cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandText = ColumnSql;

            var p = cmd.CreateParameter();
            p.ParameterName = "@tableName";
            p.Value = tbl.Name;
            cmd.Parameters.Add(p);

            p = cmd.CreateParameter();
            p.ParameterName = "@schemaName";
            p.Value = tbl.Schema;
            cmd.Parameters.Add(p);

            using IDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                if (cfg.IsColumnExcluded(rdr["ColumnName"].ToString()!))
                {
                    continue;
                }

                var col = new Column(cfg)
                {
                    Name = rdr["ColumnName"].ToString()!,
                    MaxLength = rdr["MaxLength"] as int?
                };

                col.PropertyName = dialect.CleanColumnName(col.Name);
                var dataType = rdr["DataType"].ToString()!;
                col.DataType = dataType;
                col.PropertyType = dialect.MapDataType(dataType, null);
                col.IsNullable = rdr["IsNullable"].ToString() == "YES";
                col.IsAutoIncrement = (int)rdr["IsIdentity"] == 1;
                if (col.MaxLength < 1)
                {
                    col.MaxLength = null;
                }

                tbl.AllColumns.Add(col);
            }
        }

        private void MarkPrimaryKeys(SqlConnection cn, Table tbl)
        {
            var primaryKeys = GetPk(cn, tbl.Name);

            foreach (var primaryKey in primaryKeys)
            {
                var pkColumn = tbl.AllColumns.SingleOrDefault(x => x.Name?.ToLower().Trim() == primaryKey.ToLower().Trim());
                if (pkColumn != null)
                {
                    pkColumn.IsPK = true;
                }
            }
        }

        private string[] GetPk(SqlConnection cn, string tableName)
        {
            var primaryKeys = new List<string>();

            using var cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandText = GetPkSql;

            var p = cmd.CreateParameter();
            p.ParameterName = "@tableName";
            p.Value = tableName;
            cmd.Parameters.Add(p);

            using var result = cmd.ExecuteReader();
            if (result.HasRows)
            {
                while (result.Read())
                {
                    primaryKeys.Add(result.GetString(0));
                }
            }

            return primaryKeys.ToArray();
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

        private void LoadInnerKeys(Table tbl)
        {
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = InnerKeysSql;

                var p = cmd.CreateParameter();
                p.ParameterName = "@tableName";
                p.Value = tbl.Schema + "." + tbl.Name;
                cmd.Parameters.Add(p);

                var result = new List<Key>();
                using (IDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var key = new Key
                        {
                            Name = rdr["FK"].ToString(),
                            PropertyName = rdr["Referencing_tbl"].ToString()!
                                    .Replace("_", string.Empty),
                            ReferencingTableName = rdr["Schema"] + "_" + rdr["Referencing_tbl"],
                            ReferencedTableColumnName = rdr["Referenced_col"].ToString(),
                            ReferencingTableColumnName = rdr["Referencing_col"].ToString()
                        };
                        result.Add(key);
                    }
                }

                FixPropertyNames(result);

                // add to table object
                foreach (var key in result)
                {
                    tbl.InnerKeys.Add(key);
                }
            }
        }

        private void LoadOuterKeys(Table tbl)
        {
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = OuterKeysSql;

                var p = cmd.CreateParameter();
                p.ParameterName = "@tableName";
                p.Value = tbl.Schema + "." + tbl.Name;
                cmd.Parameters.Add(p);

                var result = new List<Key>();
                using (IDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var key = new Key
                        {
                            Name = rdr["FK"].ToString(),
                            PropertyName = rdr["Referenced_tbl"].ToString()!
                                .Replace("_", string.Empty),
                            ReferencedTableName =
                                rdr["Referenced_schema"] + "_" + rdr["Referenced_tbl"],
                            ReferencedTableColumnName = rdr["Referenced_col"].ToString(),
                            ReferencingTableColumnName = rdr["Referencing_col"].ToString()
                        };
                        result.Add(key);
                    }
                }

                FixPropertyNames(result);

                // add to table object
                foreach (var key in result)
                {
                    tbl.OuterKeys.Add(key);
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
}
