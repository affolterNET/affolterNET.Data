using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using affolterNET.Data;
using affolterNET.Data.Extensions;
using affolterNET.Data.Interfaces;
using Dapper;
using Da = System.ComponentModel.DataAnnotations;

#pragma warning disable CS8618
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable StyleCop.SA1001
// ReSharper disable StyleCop.SA1402
// ReSharper disable StyleCop.SA1101
// ReSharper disable StyleCop.SA1310
// ReSharper disable StyleCop.SA1201
// ReSharper disable StyleCop.SA1401
// ReSharper disable StyleCop.SA1311
// ReSharper disable StyleCop.SA1516
// ReSharper disable StyleCop.SA1015
// ReSharper disable StyleCop.SA1012
// ReSharper disable StyleCop.SA1013
// ReSharper disable StyleCop.SA1113
// ReSharper disable StyleCop.SA1115
// ReSharper disable StyleCop.SA1116
namespace ExamplePgVersion.Data
{
    public class DtoFactory : IDtoFactory
    {
        public IDtoBase Get<T>()
            where T : IDtoBase
        {
            if (typeof(example_pg_version_demo_table) == typeof(T))
            {
                return new example_pg_version_demo_table();
            }

            if (typeof(example_pg_version_demo_table_type) == typeof(T))
            {
                return new example_pg_version_demo_table_type();
            }

            throw new InvalidOperationException();
        }
    }

    public class ViewFactory : IViewFactory
    {
        public IViewBase Get<T>()
            where T : IViewBase
        {
            if (typeof(example_pg_version_v_demo) == typeof(T))
            {
                return new example_pg_version_v_demo();
            }

            throw new InvalidOperationException();
        }
    }

    public class example_pg_version_demo_table : IDtoBase
    {
        public const string TABLE_NAME = "example_pg_version.demo_table";
        [Da.DataType("uuid")]
        [Da.Key]
        public Guid Id { get; set; }

        [Da.DataType("text")]
        [Da.Required]
        public string Message { get; set; }

        [Da.DataType("varchar")]
        [Da.MaxLength(50)]
        [Da.Required]
        public string Status { get; set; }

        [Da.DataType("uuid")]
        public Guid? TypeId { get; set; }

        [Da.DataType("int4")]
        [Da.Required]
        public int VersionTimestamp { get; set; }

        private static readonly List<string> colNames = new List<string>
        {
            "id",
            "message",
            "status",
            "type_id",
            "version_timestamp"
        };
        public IEnumerable<string> GetColumnNames() => colNames;
        public static IEnumerable<string> ColNames => colNames;

        public static class Cols
        {
            public const string Id = """id""";
            public const string Message = """message""";
            public const string Status = """status""";
            public const string TypeId = """type_id""";
            public const string VersionTimestamp = """version_timestamp""";
        }

        public bool IsAutoincrementId()
        {
            return false;
        }

        public string GetTableName()
        {
            return TABLE_NAME;
        }

        public string GetSelectCommand(int maxCount = 1000, params string[] excludedColumns)
        {
            var cols = "\"id\", \"message\", \"type_id\", \"status\", \"version_timestamp\"".GetColumns(affolterNET.Data.Extensions.QuoteStyle.DoubleQuotes, excludedColumns);
            return $"select {cols.JoinCols(false, affolterNET.Data.Extensions.QuoteStyle.DoubleQuotes)} from \"example_pg_version\".\"demo_table\" where (@Id is null or \"id\"=@Id) limit {maxCount}";
        }

        public string GetInsertCommand(bool returnScopeIdentity = false, params string[] excludedColumns)
        {
            var cols = "\"id\", \"message\", \"type_id\", \"status\"".GetColumns(affolterNET.Data.Extensions.QuoteStyle.DoubleQuotes, excludedColumns);
            var sql = $"insert into \"example_pg_version\".\"demo_table\" ({cols.JoinCols(false, affolterNET.Data.Extensions.QuoteStyle.DoubleQuotes)}) values ({cols.JoinCols(true, affolterNET.Data.Extensions.QuoteStyle.DoubleQuotes)})";
            if (returnScopeIdentity)
            {
                sql += " returning \"id\" as id";
            }

            return sql;
        }

        public string GetUpdateCommand(params string[] excludedColumns)
        {
            var cols = "\"id\", \"message\", \"type_id\", \"status\"".GetColumns(affolterNET.Data.Extensions.QuoteStyle.DoubleQuotes, excludedColumns);
            return $"update \"example_pg_version\".\"demo_table\" set {cols.JoinForUpdate(affolterNET.Data.Extensions.QuoteStyle.DoubleQuotes)} where \"id\"=@Id and \"version_timestamp\"=@VersionTimestamp";
        }

        public string GetDeleteCommand()
        {
            return "delete from \"example_pg_version\".\"demo_table\" where \"id\"=@Id and \"version_timestamp\"=@VersionTimestamp";
        }

        public string GetDeleteAllCommand()
        {
            return "delete from \"example_pg_version\".\"demo_table\"";
        }

        public string GetSaveByIdCommand(bool select = false, params string[] excludedColumns)
        {
            return @$"
                        WITH upsert AS (
                            {GetUpdateCommand(excludedColumns)} RETURNING *
                        ), inserted AS (
                            {GetInsertCommand(false, excludedColumns)}
                            WHERE NOT EXISTS (SELECT 1 FROM upsert)
                            RETURNING *
                        )
                        SELECT 'example_pg_version' AS ""Schema"", 'demo_table' AS ""Table"",
                            Id::text AS ""Id"",
                            CASE
                                WHEN EXISTS (SELECT 1 FROM upsert) THEN '{Constants.Updated}'
                                WHEN EXISTS (SELECT 1 FROM inserted) THEN '{Constants.Inserted}'
                                ELSE '{Constants.NoAction}'
                            END AS ""Action"";
                        {(select ? GetSelectCommand(1, excludedColumns) : string.Empty)}";
        }

        public example_pg_version_demo_table? GetFromDb(IDbConnection conn, IDbTransaction trsact)
        {
            return conn.QueryFirstOrDefault<example_pg_version_demo_table>(this.GetSelectCommand(1), this, trsact);
        }

        public void Reload(IDbConnection conn, IDbTransaction trsact)
        {
            var loaded = this.GetFromDb(conn, trsact);
            if (loaded == null)
            {
                throw new InvalidOperationException("entity not found");
            }

            this.Message = loaded.Message;
            this.Status = loaded.Status;
            this.TypeId = loaded.TypeId;
            this.VersionTimestamp = loaded.VersionTimestamp;
        }

        public string GetIdName()
        {
            return "Id";
        }

        public void SetId(object id)
        {
            if (!Guid.TryParse(id.ToString(), out var guidId))
            {
                throw new InvalidOperationException("invalid id");
            }

            Id = guidId;
        }

        public void SetInserted(string userName)
        {
            SetInsertedUser(userName);
            SetInsertedDate(DateTime.UtcNow);
        }

        public void SetUpdated(string userName)
        {
            SetUpdatedUser(userName);
            SetUpdatedDate(DateTime.UtcNow);
        }

        public string GetVersionName()
        {
            return "version_timestamp";
        }

        public string GetIsActiveName()
        {
            return "n.a.";
        }

        public void SetIsActive(bool isActive)
        {
        }

        public string GetUpdatedUserName()
        {
            return "n.a.";
        }

        public void SetUpdatedUser(string userName)
        {
        }

        public string GetInsertedUserName()
        {
            return "n.a.";
        }

        public void SetInsertedUser(string userName)
        {
        }

        public string GetUpdatedDateName()
        {
            return "n.a.";
        }

        public void SetUpdatedDate(DateTime date)
        {
        }

        public string GetInsertedDateName()
        {
            return "n.a.";
        }

        public void SetInsertedDate(DateTime date)
        {
        }

        public override string ToString()
        {
            return $"id: {Id}; message: {Message}; type_id: {TypeId}; status: {Status}; version_timestamp: {VersionTimestamp}";
        }
    }

    public class example_pg_version_demo_table_type : IDtoBase
    {
        public const string TABLE_NAME = "example_pg_version.demo_table_type";
        [Da.DataType("uuid")]
        [Da.Key]
        public Guid Id { get; set; }

        [Da.DataType("text")]
        [Da.Required]
        public string Name { get; set; }

        private static readonly List<string> colNames = new List<string>
        {
            "id",
            "name"
        };
        public IEnumerable<string> GetColumnNames() => colNames;
        public static IEnumerable<string> ColNames => colNames;

        public static class Cols
        {
            public const string Id = """id""";
            public const string Name = """name""";
        }

        public bool IsAutoincrementId()
        {
            return false;
        }

        public string GetTableName()
        {
            return TABLE_NAME;
        }

        public string GetSelectCommand(int maxCount = 1000, params string[] excludedColumns)
        {
            var cols = "\"id\", \"name\"".GetColumns(affolterNET.Data.Extensions.QuoteStyle.DoubleQuotes, excludedColumns);
            return $"select {cols.JoinCols(false, affolterNET.Data.Extensions.QuoteStyle.DoubleQuotes)} from \"example_pg_version\".\"demo_table_type\" where (@Id is null or \"id\"=@Id) limit {maxCount}";
        }

        public string GetInsertCommand(bool returnScopeIdentity = false, params string[] excludedColumns)
        {
            var cols = "\"id\", \"name\"".GetColumns(affolterNET.Data.Extensions.QuoteStyle.DoubleQuotes, excludedColumns);
            var sql = $"insert into \"example_pg_version\".\"demo_table_type\" ({cols.JoinCols(false, affolterNET.Data.Extensions.QuoteStyle.DoubleQuotes)}) values ({cols.JoinCols(true, affolterNET.Data.Extensions.QuoteStyle.DoubleQuotes)})";
            if (returnScopeIdentity)
            {
                sql += " returning \"id\" as id";
            }

            return sql;
        }

        public string GetUpdateCommand(params string[] excludedColumns)
        {
            var cols = "\"id\", \"name\"".GetColumns(affolterNET.Data.Extensions.QuoteStyle.DoubleQuotes, excludedColumns);
            return $"update \"example_pg_version\".\"demo_table_type\" set {cols.JoinForUpdate(affolterNET.Data.Extensions.QuoteStyle.DoubleQuotes)} where \"id\"=@Id";
        }

        public string GetDeleteCommand()
        {
            return "delete from \"example_pg_version\".\"demo_table_type\" where \"id\"=@Id";
        }

        public string GetDeleteAllCommand()
        {
            return "delete from \"example_pg_version\".\"demo_table_type\"";
        }

        public string GetSaveByIdCommand(bool select = false, params string[] excludedColumns)
        {
            return @$"
                        WITH upsert AS (
                            {GetUpdateCommand(excludedColumns)} RETURNING *
                        ), inserted AS (
                            {GetInsertCommand(false, excludedColumns)}
                            WHERE NOT EXISTS (SELECT 1 FROM upsert)
                            RETURNING *
                        )
                        SELECT 'example_pg_version' AS ""Schema"", 'demo_table_type' AS ""Table"",
                            Id::text AS ""Id"",
                            CASE
                                WHEN EXISTS (SELECT 1 FROM upsert) THEN '{Constants.Updated}'
                                WHEN EXISTS (SELECT 1 FROM inserted) THEN '{Constants.Inserted}'
                                ELSE '{Constants.NoAction}'
                            END AS ""Action"";
                        {(select ? GetSelectCommand(1, excludedColumns) : string.Empty)}";
        }

        public example_pg_version_demo_table_type? GetFromDb(IDbConnection conn, IDbTransaction trsact)
        {
            return conn.QueryFirstOrDefault<example_pg_version_demo_table_type>(this.GetSelectCommand(1), this, trsact);
        }

        public void Reload(IDbConnection conn, IDbTransaction trsact)
        {
            var loaded = this.GetFromDb(conn, trsact);
            if (loaded == null)
            {
                throw new InvalidOperationException("entity not found");
            }

            this.Name = loaded.Name;
        }

        public string GetIdName()
        {
            return "Id";
        }

        public void SetId(object id)
        {
            if (!Guid.TryParse(id.ToString(), out var guidId))
            {
                throw new InvalidOperationException("invalid id");
            }

            Id = guidId;
        }

        public void SetInserted(string userName)
        {
            SetInsertedUser(userName);
            SetInsertedDate(DateTime.UtcNow);
        }

        public void SetUpdated(string userName)
        {
            SetUpdatedUser(userName);
            SetUpdatedDate(DateTime.UtcNow);
        }

        public string GetVersionName()
        {
            return "n.a.";
        }

        public string GetIsActiveName()
        {
            return "n.a.";
        }

        public void SetIsActive(bool isActive)
        {
        }

        public string GetUpdatedUserName()
        {
            return "n.a.";
        }

        public void SetUpdatedUser(string userName)
        {
        }

        public string GetInsertedUserName()
        {
            return "n.a.";
        }

        public void SetInsertedUser(string userName)
        {
        }

        public string GetUpdatedDateName()
        {
            return "n.a.";
        }

        public void SetUpdatedDate(DateTime date)
        {
        }

        public string GetInsertedDateName()
        {
            return "n.a.";
        }

        public void SetInsertedDate(DateTime date)
        {
        }

        public override string ToString()
        {
            return $"id: {Id}; name: {Name}";
        }
    }

    public static class ExamplePgVersionDemoTableTypes
    {
        public static Guid Eins => _dict.First(kvp => kvp.Value == "Eins").Key;
        public static Guid Zwei => _dict.First(kvp => kvp.Value == "Zwei").Key;
        public static Guid Drei => _dict.First(kvp => kvp.Value == "Drei").Key;
        public static Guid Vier => _dict.First(kvp => kvp.Value == "Vier").Key;

        private static Dictionary<Guid, string> _dict = new()
        {
            {
                Guid.Parse("c1060bb2-07b0-4e5d-ad0b-35f3993d823d"),
                "Eins"
            },
            {
                Guid.Parse("d749abff-6a43-4348-839f-61323fdc52d1"),
                "Zwei"
            },
            {
                Guid.Parse("36a072b9-7216-4b99-bf8d-79730a4a1f37"),
                "Drei"
            },
            {
                Guid.Parse("230a5728-acb6-4e91-aea3-05ef34c0755d"),
                "Vier"
            }
        };
        public static string? GetExamplePgVersionDemoTableTypesString(this Guid g)
        {
            var entry = _dict.FirstOrDefault(kvp => kvp.Key.Equals(g));
            return entry.Equals(default(KeyValuePair<Guid, string>)) ? null : entry.Value;
        }
    }

    public class example_pg_version_v_demo : IViewBase
    {
        public const string TABLE_NAME = "example_pg_version.v_demo";
        [Da.DataType("uuid")]
        public Guid? Id { get; set; }

        [Da.DataType("text")]
        public string Message { get; set; }

        [Da.DataType("varchar")]
        [Da.MaxLength(50)]
        public string Status { get; set; }

        [Da.DataType("uuid")]
        public Guid? TypeId { get; set; }

        [Da.DataType("int4")]
        public int? VersionTimestamp { get; set; }

        private static readonly List<string> colNames = new List<string>
        {
            "id",
            "message",
            "status",
            "type_id",
            "version_timestamp"
        };
        public IEnumerable<string> GetColumnNames() => colNames;
        public static IEnumerable<string> ColNames => colNames;

        public static class Cols
        {
            public const string Id = """id""";
            public const string Message = """message""";
            public const string Status = """status""";
            public const string TypeId = """type_id""";
            public const string VersionTimestamp = """version_timestamp""";
        }

        public string GetTableName()
        {
            return TABLE_NAME;
        }

        public string GetSelectCommand(int maxCount = 1000, params string[] excludedColumns)
        {
            var cols = "\"id\", \"message\", \"type_id\", \"status\", \"version_timestamp\"".GetColumns(affolterNET.Data.Extensions.QuoteStyle.DoubleQuotes, excludedColumns);
            return $"select {cols.JoinCols(false, affolterNET.Data.Extensions.QuoteStyle.DoubleQuotes)} from \"example_pg_version\".\"v_demo\" limit {maxCount}";
        }

        public override string ToString()
        {
            return $"id: {Id}; message: {Message}; type_id: {TypeId}; status: {Status}; version_timestamp: {VersionTimestamp}";
        }
    }
}