using System;
using System.Data;
using System.Threading.Tasks;
using affolterNET.Data.Extensions;
using affolterNET.Data.Interfaces;
using affolterNET.Data.Models;
using affolterNET.Data.Result;
using Dapper;

namespace affolterNET.Data.Commands
{
    public class SaveEntityCommand<T> : CommandQueryBase<SaveInfo>, IQuery<SaveInfo> where T : class, IDtoBase
    {
        private readonly bool _select;

        public SaveEntityCommand(T dto, bool select = false, params string[] excludedColumns)
        {
            _select = select;
            Sql = dto.GetSaveByIdCommand(select, excludedColumns);
            AddParams(dto);
        }
        
        public SaveEntityCommand(T dto, string userName, bool select = false, params string[] excludedColumns): base(userName)
        {
            _select = select;
            Sql = dto.GetSaveByIdCommand(select, excludedColumns);
            AddMeta(dto, userName);
            AddParams(dto);
        }

        public override async Task<DataResult<SaveInfo>> ExecuteAsync(IDbConnection connection,
            IDbTransaction transaction)
        {
            var reader = await connection.QueryMultipleAsync(Sql, ParamsDict, transaction);
            try
            {
                return await Transform(reader);
            }
            finally
            {
                reader.EndQueryMultiple();
            }
        }

        private async Task<DataResult<SaveInfo>> Transform(SqlMapper.GridReader reader)
        {
            var saveInfo = await reader.ReadFirstOrDefaultAsync<SaveInfo>()
                           ?? throw new InvalidOperationException("SaveById returned no result");
            if (_select)
            {
                var dto = reader.Read<T>();
                saveInfo.Dto = dto;
            }
            var result = new DataResult<SaveInfo>(saveInfo)
            {
                SqlCommand = ToString()
            };
            return result;
        }
    }
}