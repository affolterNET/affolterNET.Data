using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using affolterNET.Data.Extensions;
using affolterNET.Data.Interfaces;
using affolterNET.Data.Models;
using affolterNET.Data.Result;
using Dapper;

namespace affolterNET.Data
{
    public abstract class CommandQueryBase<TResult>
    {
        public const string ScopeIdentity = "select scope_identity() as id";
        public const string LastChangedDate = "LastChangedDate";

        protected CommandQueryBase(string user = "", bool? excludeFromHistory = null)
        {
            UserName = user;
            Params = new ExpandoObject();
            if (excludeFromHistory == null)
            {
                CheckNotExplicitlySetExcludeFromHistory = true;
                excludeFromHistory =
                    GetType()!.Namespace!.IndexOf("Commands", StringComparison.CurrentCultureIgnoreCase) == -1;
            }

            ExcludeFromHistory = excludeFromHistory.Value;
        }

        public string UserName { get; set; }

        public bool CheckNotExplicitlySetExcludeFromHistory { get; }

        public bool ExcludeFromHistory { get; }

        protected dynamic Params { get; }

        public IDictionary<string, object> ParamsDict => Params;

        public Dictionary<string, string> SqlDataTypes { get; } = new();

        protected object ParamsObject => Params;

        public string Sql { get; protected set; } = string.Empty;

        protected void AddParams<T>(T paramsObject, Func<string, bool>? predicate = null)
        {
            if (paramsObject == null)
            {
                throw new ArgumentNullException(nameof(paramsObject), "object cannot be null");
            }

            foreach (var property in paramsObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (predicate != null && !predicate(property.Name))
                {
                    continue;
                }

                var sqlDataType =
                    property.CustomAttributes.FirstOrDefault(da => da.AttributeType.Name == "DataTypeAttribute")
                        ?.ConstructorArguments.FirstOrDefault();
                if (sqlDataType != null)
                {
                    SqlDataTypes.Add(property.Name, sqlDataType.ToString()!.TrimStart('"').TrimEnd('"'));
                }

                var val = property.GetValue(paramsObject);
                AddParam(property.Name, val!);
            }
        }

        protected void AddParam(string propertyName, object propertyValue, string? sqlDataType = null)
        {
            var pn = propertyName.StripSquareBrackets();
            if (ParamsDict.ContainsKey(pn))
            {
                ParamsDict[pn] = propertyValue;
            }
            else
            {
                ParamsDict.Add(pn, propertyValue);
            }

            if (sqlDataType != null)
            {
                SqlDataTypes.Add(pn, sqlDataType);
            }
        }

        protected void AddMeta(IDtoBase dto, string? userName)
        {
            var dtNow = DateTime.UtcNow;
            dto.SetInsertedDate(dtNow);
            dto.SetInsertedUser(userName ?? string.Empty);
            dto.SetUpdatedDate(dtNow);
            dto.SetUpdatedUser(userName ?? string.Empty);
        }

        public virtual DataResult<TResult> Execute(IDbConnection connection, IDbTransaction transaction)
        {
            return ExecuteAsync(connection, transaction).GetAwaiter().GetResult();
        }

        public abstract Task<DataResult<TResult>> ExecuteAsync(IDbConnection connection, IDbTransaction transaction);

        protected async Task<DataResult<IEnumerable<SaveInfo>>> ExecuteSaveInfo(IDbConnection connection,
            IDbTransaction transaction)
        {
            var reader = await connection.QueryMultipleAsync(Sql, ParamsObject, transaction);
            try
            {
                return await TransformSaveInfo(reader);
            }
            finally
            {
                reader.EndQueryMultiple();
            }
        }
        
        protected async Task<DataResult<IEnumerable<SaveInfo>>> TransformSaveInfo(SqlMapper.GridReader reader)
        {
            var results = new List<SaveInfo>();
            do
            {
                var info = await reader.ReadFirstOrDefaultAsync<SaveInfo>();
                if (info != null)
                {
                    results.Add(info);
                }
            } while (!reader.IsConsumed);

            return new DataResult<IEnumerable<SaveInfo>>(results);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(
                "-- WIP: Falls die Variablen nicht richtig ausgegeben werden, bitte eLog2.Basis.Daten.Queries.CommandQueryBase.SchreibeWert erweitern");
            sb.AppendLine();
            if (ParamsDict != null)
            {
                foreach (var key in ParamsDict.Keys)
                {
                    var val = ParamsDict[key];
                    var propertyType =
                        "nvarchar(100) -- Parameter Typ konnte nicht evaluiert werden (Wert=null 0der kein passender SqlType)";
                    if (SqlDataTypes.ContainsKey(key))
                    {
                        propertyType = SqlDataTypes[key];
                    }
                    else
                    {
                        propertyType = val == null ? propertyType : SqlType(val.GetType(), propertyType);
                    }

                    var propertyTypeDef = propertyType;
                    if (propertyTypeDef == "nvarchar")
                    {
                        propertyTypeDef = "nvarchar(max)";
                    }

                    sb.AppendLine($"declare @{key} {propertyTypeDef};");
                    var wert = SchreibeWert(propertyTypeDef, val ?? "null");
                    sb.AppendLine($"set @{key} = {wert};");
                    sb.Append(Environment.NewLine);
                }
            }

            sb.Append(Environment.NewLine);
            sb.Append(Sql);
            return sb.ToString();
        }

        private string SchreibeWert(string propertyType, object val)
        {
            if (val == null)
            {
                return "null";
            }

            switch (propertyType)
            {
                case "uniqueidentifier":
                case "nvarchar(max)":
                    return $"'{val}'";
                case "int":
                    var i = val as int?;
                    return i?.ToString() ?? "null";
                case "bit":
                    var v = val as bool?;
                    return v.HasValue ? v.Value ? "1" : "0" : "null";
                case "binary":
                    // hex wert bauen
                    var hex = BitConverter.ToString((byte[])val);
                    hex = $"0x{hex.Replace("-", string.Empty)}";
                    return hex;
                case "date":
                    var d = val as DateOnly?;
                    return $"'{d?.ToString("yyyy-MM-dd")}'";
                case "datetime":
                    var dt = val as DateTime?;
                    return $"'{dt?.ToString("yyyy-MM-dd HH:mm:ss.fff")}'";
            }

            return $"{val}";
        }

        private string SqlType(Type csharpType, string def)
        {
            var underlying = Nullable.GetUnderlyingType(csharpType);
            if (underlying != null)
            {
                // nullable
                csharpType = underlying;
            }

            switch (csharpType.Name.ToLower())
            {
                case "string":
                    return "nvarchar(max)";
                case "long":
                    return "bigint";
                case "short":
                    return "smallint";
                case "int16":
                case "int32":
                    return "int";
                case "guid":
                    return "uniqueidentifier";
                case "datetime":
                    return "datetime";
                case "float":
                case "double":
                    return "double";
                case "decimal":
                    return "decimal";
                case "tinyint":
                    return "byte";
                case "bool":
                case "boolean":
                    return "bit";
                case "byte[]":
                    return "binary";
                case "Microsoft.SqlServer.Types.SqlGeography":
                    return "geography";
                case "Microsoft.SqlServer.Types.SqlGeometry":
                    return "geometry";
            }

            Console.WriteLine($"type {csharpType.Name.ToLower()} ist nicht implementiert, gebe default zurück");
            return def;
        }
    }
}