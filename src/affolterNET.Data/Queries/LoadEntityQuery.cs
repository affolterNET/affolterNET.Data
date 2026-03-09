using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using affolterNET.Data.Interfaces;
using affolterNET.Data.Models.Filters;
using affolterNET.Data.Result;
using Dapper;

namespace affolterNET.Data.Queries
{
    public class LoadEntityQuery<T>: CommandQueryBase<IEnumerable<T>>, IQuery<IEnumerable<T>> where T: class, IViewBase
    {
        public static LoadEntityQuery<T> CreateWithFilter(string colName, object? value, string userName = "", int maxcount = 1000)
        {
            var filter = new RootFilter(colName)
            {
                Value = value
            };
            return new LoadEntityQuery<T>(filter, userName, maxcount);
        }

        public LoadEntityQuery(string userName = "", int maxcount = 1000, string? idName = null): base(userName)
        {
            // command (can work with or without id)
            var dto = Activator.CreateInstance<T>();
            Sql = dto.GetSelectCommand(maxcount);
            if (idName == null && dto is IDtoBase dtobase)
            {
                idName = dtobase.GetIdName();
                AddParam(idName, null!);
            }
        }
        
        public LoadEntityQuery(RootFilter filter, string userName = "", int maxcount = 1000): base(userName)
        {
            // command (can work with or without id)
            var dto = Activator.CreateInstance<T>();
            var sql = dto.GetSelectCommand(maxcount);
            var whereIdx = sql.IndexOf(" where ", StringComparison.InvariantCultureIgnoreCase);
            if (whereIdx > -1)
            {
                Sql = $"{sql.Substring(0, whereIdx)} {filter}";
            }
            else
            {
                // Handle PostgreSQL LIMIT clause: insert WHERE before LIMIT
                var limitIdx = sql.IndexOf(" limit ", StringComparison.InvariantCultureIgnoreCase);
                if (limitIdx > -1)
                {
                    Sql = $"{sql.Substring(0, limitIdx)} {filter}{sql.Substring(limitIdx)}";
                }
                else
                {
                    Sql = $"{sql} {filter}";
                }
            }

            
            foreach (var p in filter.GetAllParameters())
            {
                AddParam(p.Key, p.Value);
            }
        }
        
        public LoadEntityQuery(object pkValue, string userName = "", string? idName = null): base(userName)
        {
            // command (can work with or without id)
            var dto = Activator.CreateInstance<T>();
            Sql = dto.GetSelectCommand();
            if (dto is IDtoBase dtobase)
            {
                idName = dtobase.GetIdName();
                AddParam(idName, pkValue);
            }
        }
        
        public override async Task<DataResult<IEnumerable<T>>> ExecuteAsync(IDbConnection connection, IDbTransaction transaction)
        {
            var iEnum = await connection.QueryAsync<T>(Sql, ParamsObject, transaction);
            var result = new DataResult<IEnumerable<T>>(iEnum)
            {
                SqlCommand = ToString()
            };
            return result;
        }
    }
}