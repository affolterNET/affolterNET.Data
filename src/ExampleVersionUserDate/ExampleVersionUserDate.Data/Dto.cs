using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using affolterNET.Data;
using affolterNET.Data.Extensions;
using affolterNET.Data.Interfaces;
using Dapper;
using Da = System.ComponentModel.DataAnnotations;

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
namespace ExampleVersionUserDate.Data
{
    public class DtoFactory : IDtoFactory
    {
        public IDtoBase Get<T>()
            where T : IDtoBase
        {
            if (typeof(ExampleVersionUserDate_T_DemoTable) == typeof(T))
            {
                return new ExampleVersionUserDate_T_DemoTable();
            }

            if (typeof(ExampleVersionUserDate_T_DemoTableType) == typeof(T))
            {
                return new ExampleVersionUserDate_T_DemoTableType();
            }

            throw new InvalidOperationException();
        }
    }

    public class ViewFactory : IViewFactory
    {
        public IViewBase Get<T>()
            where T : IViewBase
        {
            if (typeof(ExampleVersionUserDate_V_Demo) == typeof(T))
            {
                return new ExampleVersionUserDate_V_Demo();
            }

            throw new InvalidOperationException();
        }
    }

    public class ExampleVersionUserDate_T_DemoTable : IDtoBase
    {
        public const string TABLE_NAME = "[ExampleVersionUserDate].[T_DemoTable]";
        [Da.DataType("uniqueidentifier")]
        [Da.Key]
        public Guid Id { get; set; }

        [Da.DataType("nvarchar")]
        [Da.MaxLength(1000)]
        [Da.Required]
        public string Message { get; set; } = null !;

        [Da.DataType("nvarchar")]
        [Da.MaxLength(50)]
        [Da.Required]
        public string Status { get; set; } = null !;

        [Da.DataType("uniqueidentifier")]
        public Guid? Type { get; set; }

        [Da.DataType("datetime")]
        [Da.Required]
        public DateTime InsertDate { get; set; }

        [Da.DataType("nvarchar")]
        [Da.MaxLength(1000)]
        [Da.Required]
        public string InsertUser { get; set; } = null !;

        [Da.DataType("datetime")]
        public DateTime? UpdateDate { get; set; }

        [Da.DataType("nvarchar")]
        [Da.MaxLength(1000)]
        public string? UpdateUser { get; set; }

        [Da.DataType("timestamp")]
        [Da.Required]
        public byte[] VersionTimestamp { get; set; } =
        {
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0
        };

        private static readonly List<string> colNames = new List<string>
        {
            "Id",
            "Message",
            "Status",
            "Type",
            "InsertDate",
            "InsertUser",
            "UpdateDate",
            "UpdateUser",
            "VersionTimestamp"
        };
        public IEnumerable<string> GetColumnNames() => colNames;
        public static IEnumerable<string> ColNames => colNames;

        public static class Cols
        {
            public const string Id = "[Id]";
            public const string Message = "[Message]";
            public const string Status = "[Status]";
            public const string Type = "[Type]";
            public const string InsertDate = "[InsertDate]";
            public const string InsertUser = "[InsertUser]";
            public const string UpdateDate = "[UpdateDate]";
            public const string UpdateUser = "[UpdateUser]";
            public const string VersionTimestamp = "[VersionTimestamp]";
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
            var cols = "[Id], [Message], [Type], [Status], [InsertDate], [InsertUser], [UpdateDate], [UpdateUser], [VersionTimestamp]".GetColumns(affolterNET.Data.Extensions.QuoteStyle.Brackets, excludedColumns);
            return $"select top({maxCount}) {cols.JoinCols(false, affolterNET.Data.Extensions.QuoteStyle.Brackets)} from [ExampleVersionUserDate].[T_DemoTable] where (@Id is null or [Id]=@Id)";
        }

        public string GetInsertCommand(bool returnScopeIdentity = false, params string[] excludedColumns)
        {
            var cols = "[Id], [Message], [Type], [Status], [InsertDate], [InsertUser]".GetColumns(affolterNET.Data.Extensions.QuoteStyle.Brackets, excludedColumns);
            var sql = $"insert into [ExampleVersionUserDate].[T_DemoTable] ({cols.JoinCols(false, affolterNET.Data.Extensions.QuoteStyle.Brackets)}) values ({cols.JoinCols(true, affolterNET.Data.Extensions.QuoteStyle.Brackets)})";
            if (returnScopeIdentity)
            {
                sql += "; select scope_identity() as id;";
            }

            return sql;
        }

        public string GetUpdateCommand(params string[] excludedColumns)
        {
            var cols = "[Id], [Message], [Type], [Status], [UpdateDate], [UpdateUser]".GetColumns(affolterNET.Data.Extensions.QuoteStyle.Brackets, excludedColumns);
            return $"update [ExampleVersionUserDate].[T_DemoTable] set {cols.JoinForUpdate(affolterNET.Data.Extensions.QuoteStyle.Brackets)} where [Id]=@Id and [VersionTimestamp]=@VersionTimestamp";
        }

        public string GetDeleteCommand()
        {
            return "delete from [ExampleVersionUserDate].[T_DemoTable] where [Id]=@Id and [VersionTimestamp]=@VersionTimestamp";
        }

        public string GetDeleteAllCommand()
        {
            return "delete from [ExampleVersionUserDate].[T_DemoTable]";
        }

        public string GetSaveByIdCommand(bool select = false, params string[] excludedColumns)
        {
            return @$"
                        declare @rowcnt int
                        if exists (select Id from [ExampleVersionUserDate].[T_DemoTable] where Id = @Id)
                            begin
                                {GetUpdateCommand(excludedColumns)}; set @rowcnt = (select @@rowcount);
                                select 'ExampleVersionUserDate' as [Schema], 'T_DemoTable' as [Table], convert(nvarchar(50), @Id) as [Id], case when @rowcnt = 0 then '{Constants.NoAction}' else '{Constants.Updated}' end as [Action];
                            end
                        else
                            begin
                                {GetInsertCommand(false, excludedColumns)}; set @rowcnt = (select @@rowcount);
                                select 'ExampleVersionUserDate' as [Schema], 'T_DemoTable' as [Table], convert(nvarchar(50), @Id) as [Id], case when @rowcnt = 0 then '{Constants.NoAction}' else '{Constants.Inserted}' end as [Action];
                            end
                        {(select ? GetSelectCommand(1, excludedColumns) : string.Empty)}";
        }

        public ExampleVersionUserDate_T_DemoTable? GetFromDb(IDbConnection conn, IDbTransaction trsact)
        {
            return conn.QueryFirstOrDefault<ExampleVersionUserDate_T_DemoTable>(this.GetSelectCommand(1), this, trsact);
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
            this.Type = loaded.Type;
            this.InsertDate = loaded.InsertDate;
            this.InsertUser = loaded.InsertUser;
            this.UpdateDate = loaded.UpdateDate;
            this.UpdateUser = loaded.UpdateUser;
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
            return "VersionTimestamp";
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
            return "UpdateUser";
        }

        public void SetUpdatedUser(string userName)
        {
            this.UpdateUser = userName;
        }

        public string GetInsertedUserName()
        {
            return "InsertUser";
        }

        public void SetInsertedUser(string userName)
        {
            this.InsertUser = userName;
        }

        public string GetUpdatedDateName()
        {
            return "UpdateDate";
        }

        public void SetUpdatedDate(DateTime date)
        {
            this.UpdateDate = date;
        }

        public string GetInsertedDateName()
        {
            return "InsertDate";
        }

        public void SetInsertedDate(DateTime date)
        {
            this.InsertDate = date;
        }

        public override string ToString()
        {
            return $"Id: {Id}; Message: {Message}; Type: {Type}; Status: {Status}; InsertDate: {InsertDate}; InsertUser: {InsertUser}; UpdateDate: {UpdateDate}; UpdateUser: {UpdateUser}; VersionTimestamp: {VersionTimestamp}";
        }
    }

    public class ExampleVersionUserDate_T_DemoTableType : IDtoBase
    {
        public const string TABLE_NAME = "[ExampleVersionUserDate].[T_DemoTableType]";
        [Da.DataType("uniqueidentifier")]
        [Da.Key]
        public Guid Id { get; set; }

        [Da.DataType("nvarchar")]
        [Da.MaxLength(1000)]
        [Da.Required]
        public string Name { get; set; } = null !;

        private static readonly List<string> colNames = new List<string>
        {
            "Id",
            "Name"
        };
        public IEnumerable<string> GetColumnNames() => colNames;
        public static IEnumerable<string> ColNames => colNames;

        public static class Cols
        {
            public const string Id = "[Id]";
            public const string Name = "[Name]";
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
            var cols = "[Id], [Name]".GetColumns(affolterNET.Data.Extensions.QuoteStyle.Brackets, excludedColumns);
            return $"select top({maxCount}) {cols.JoinCols(false, affolterNET.Data.Extensions.QuoteStyle.Brackets)} from [ExampleVersionUserDate].[T_DemoTableType] where (@Id is null or [Id]=@Id)";
        }

        public string GetInsertCommand(bool returnScopeIdentity = false, params string[] excludedColumns)
        {
            var cols = "[Id], [Name]".GetColumns(affolterNET.Data.Extensions.QuoteStyle.Brackets, excludedColumns);
            var sql = $"insert into [ExampleVersionUserDate].[T_DemoTableType] ({cols.JoinCols(false, affolterNET.Data.Extensions.QuoteStyle.Brackets)}) values ({cols.JoinCols(true, affolterNET.Data.Extensions.QuoteStyle.Brackets)})";
            if (returnScopeIdentity)
            {
                sql += "; select scope_identity() as id;";
            }

            return sql;
        }

        public string GetUpdateCommand(params string[] excludedColumns)
        {
            var cols = "[Id], [Name]".GetColumns(affolterNET.Data.Extensions.QuoteStyle.Brackets, excludedColumns);
            return $"update [ExampleVersionUserDate].[T_DemoTableType] set {cols.JoinForUpdate(affolterNET.Data.Extensions.QuoteStyle.Brackets)} where [Id]=@Id";
        }

        public string GetDeleteCommand()
        {
            return "delete from [ExampleVersionUserDate].[T_DemoTableType] where [Id]=@Id";
        }

        public string GetDeleteAllCommand()
        {
            return "delete from [ExampleVersionUserDate].[T_DemoTableType]";
        }

        public string GetSaveByIdCommand(bool select = false, params string[] excludedColumns)
        {
            return @$"
                        declare @rowcnt int
                        if exists (select Id from [ExampleVersionUserDate].[T_DemoTableType] where Id = @Id)
                            begin
                                {GetUpdateCommand(excludedColumns)}; set @rowcnt = (select @@rowcount);
                                select 'ExampleVersionUserDate' as [Schema], 'T_DemoTableType' as [Table], convert(nvarchar(50), @Id) as [Id], case when @rowcnt = 0 then '{Constants.NoAction}' else '{Constants.Updated}' end as [Action];
                            end
                        else
                            begin
                                {GetInsertCommand(false, excludedColumns)}; set @rowcnt = (select @@rowcount);
                                select 'ExampleVersionUserDate' as [Schema], 'T_DemoTableType' as [Table], convert(nvarchar(50), @Id) as [Id], case when @rowcnt = 0 then '{Constants.NoAction}' else '{Constants.Inserted}' end as [Action];
                            end
                        {(select ? GetSelectCommand(1, excludedColumns) : string.Empty)}";
        }

        public ExampleVersionUserDate_T_DemoTableType? GetFromDb(IDbConnection conn, IDbTransaction trsact)
        {
            return conn.QueryFirstOrDefault<ExampleVersionUserDate_T_DemoTableType>(this.GetSelectCommand(1), this, trsact);
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
            return $"Id: {Id}; Name: {Name}";
        }
    }

    public static class ExampleVersionUserDateDemoTableTypes
    {
        public static Guid Vier => _dict.First(kvp => kvp.Value == "Vier").Key;
        public static Guid Eins => _dict.First(kvp => kvp.Value == "Eins").Key;
        public static Guid Zwei => _dict.First(kvp => kvp.Value == "Zwei").Key;
        public static Guid Drei => _dict.First(kvp => kvp.Value == "Drei").Key;

        private static Dictionary<Guid, string> _dict = new()
        {
            {
                Guid.Parse("230a5728-acb6-4e91-aea3-05ef34c0755d"),
                "Vier"
            },
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
            }
        };
        public static string? GetExampleVersionUserDateDemoTableTypesString(this Guid g)
        {
            var entry = _dict.FirstOrDefault(kvp => kvp.Key.Equals(g));
            return entry.Equals(default(KeyValuePair<Guid, string>)) ? null : entry.Value;
        }
    }

    public class ExampleVersionUserDate_V_Demo : IViewBase
    {
        public const string TABLE_NAME = "[ExampleVersionUserDate].[V_Demo]";
        [Da.DataType("uniqueidentifier")]
        [Da.Required]
        public Guid Id { get; set; }

        [Da.DataType("nvarchar")]
        [Da.MaxLength(1000)]
        [Da.Required]
        public string Message { get; set; } = null !;

        [Da.DataType("nvarchar")]
        [Da.MaxLength(50)]
        [Da.Required]
        public string Status { get; set; } = null !;

        [Da.DataType("uniqueidentifier")]
        public Guid? Type { get; set; }

        [Da.DataType("datetime")]
        [Da.Required]
        public DateTime InsertDate { get; set; }

        [Da.DataType("nvarchar")]
        [Da.MaxLength(1000)]
        [Da.Required]
        public string InsertUser { get; set; } = null !;

        [Da.DataType("datetime")]
        public DateTime? UpdateDate { get; set; }

        [Da.DataType("nvarchar")]
        [Da.MaxLength(1000)]
        public string? UpdateUser { get; set; }

        [Da.DataType("timestamp")]
        [Da.Required]
        public byte[] VersionTimestamp { get; set; } =
        {
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0
        };

        private static readonly List<string> colNames = new List<string>
        {
            "Id",
            "Message",
            "Status",
            "Type",
            "InsertDate",
            "InsertUser",
            "UpdateDate",
            "UpdateUser",
            "VersionTimestamp"
        };
        public IEnumerable<string> GetColumnNames() => colNames;
        public static IEnumerable<string> ColNames => colNames;

        public static class Cols
        {
            public const string Id = "[Id]";
            public const string Message = "[Message]";
            public const string Status = "[Status]";
            public const string Type = "[Type]";
            public const string InsertDate = "[InsertDate]";
            public const string InsertUser = "[InsertUser]";
            public const string UpdateDate = "[UpdateDate]";
            public const string UpdateUser = "[UpdateUser]";
            public const string VersionTimestamp = "[VersionTimestamp]";
        }

        public string GetTableName()
        {
            return TABLE_NAME;
        }

        public string GetSelectCommand(int maxCount = 1000, params string[] excludedColumns)
        {
            var cols = "[Id], [Message], [Type], [Status], [InsertDate], [InsertUser], [UpdateDate], [UpdateUser], [VersionTimestamp]".GetColumns(affolterNET.Data.Extensions.QuoteStyle.Brackets, excludedColumns);
            return $"select top({maxCount}) {cols.JoinCols(false, affolterNET.Data.Extensions.QuoteStyle.Brackets)} from [ExampleVersionUserDate].[V_Demo]";
        }

        public override string ToString()
        {
            return $"Id: {Id}; Message: {Message}; Type: {Type}; Status: {Status}; InsertDate: {InsertDate}; InsertUser: {InsertUser}; UpdateDate: {UpdateDate}; UpdateUser: {UpdateUser}; VersionTimestamp: {VersionTimestamp}";
        }
    }
}