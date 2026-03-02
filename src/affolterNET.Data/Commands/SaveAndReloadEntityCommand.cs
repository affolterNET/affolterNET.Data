using System;
using System.Data;
using System.Threading.Tasks;
using affolterNET.Data.Interfaces;
using affolterNET.Data.Result;
using Dapper;

namespace affolterNET.Data.Commands
{
    public class SaveAndReloadEntityCommand<T> : CommandQueryBase<T>, IQuery<T> where T : class, IDtoBase
    {
        public SaveAndReloadEntityCommand(T dto, string? userName = null): base(userName ?? string.Empty)
        {
            Sql = dto.GetSaveByIdCommand();
            AddMeta(dto, userName);
            AddParams(dto);
        }

        public override async Task<DataResult<T>> ExecuteAsync(IDbConnection connection, IDbTransaction transaction)
        {
            var firstOrDefault = await connection.QueryFirstOrDefaultAsync<T>(Sql, ParamsObject, transaction)
                                 ?? throw new InvalidOperationException("SaveAndReload returned no result");
            var result = new DataResult<T>(firstOrDefault)
            {
                SqlCommand = ToString()
            };
            return result;
        }
    }
}