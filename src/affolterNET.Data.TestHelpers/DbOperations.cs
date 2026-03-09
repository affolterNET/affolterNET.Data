using System;
using System.Data;
using System.Linq;
using affolterNET.Data.Interfaces;
using affolterNET.Data.TestHelpers.Builders;
using Dapper;
using FluentAssertions;

namespace affolterNET.Data.TestHelpers
{
    public class DbOperations
    {
        private readonly IDtoFactory _dtoFactory;

        public readonly IDbConnection Connection;
        public readonly IDbTransaction Transaction;

        public DbOperations(IDbConnection connection, IDbTransaction transaction, IDtoFactory dtoFactory)
        {
            Connection = connection;
            Transaction = transaction;
            _dtoFactory = dtoFactory;
        }

        public SelectBuilder<T> Select<T>()
            where T : IDtoBase
        {
            var dto = _dtoFactory.Get<T>();
            return new SelectBuilder<T>(Connection, Transaction, dto);
        }

        public T SelectById<T>(object id, string idName = "Id")
            where T : IDtoBase
        {
            return Select<T>().WithWhere(idName, id).ExecuteSingle();
        }

        public T GetFreeId<TDto, T>(string idName = "Id")
            where TDto : IDtoBase
        {
            var dto = _dtoFactory.Get<TDto>();
            var ids = Connection.Query<T>($"select {idName} from {dto.GetTableName()}", null, Transaction).ToList();
            var id = 0;
            while (true)
            {
                id++;
                var idCheck = (T)Convert.ChangeType(id, typeof(T));
                if (ids.IndexOf(idCheck) == -1)
                {
                    break;
                }
            }

            return (T)Convert.ChangeType(id, typeof(T));
        }

        public SoftDeleteBuilder<T> SoftDelete<T>()
            where T : IDtoBase
        {
            var dto = _dtoFactory.Get<T>();
            return new SoftDeleteBuilder<T>(Connection, Transaction, dto);
        }

        public int SoftDeleteById<T>(object id, string idName = "Id", bool check = true)
            where T : IDtoBase
        {
            var resultat = SoftDelete<T>().WithWhere(idName, id).Execute();
            if (check)
            {
                resultat.Should().Be(1, "Datensatz wurde nicht gelöscht");
            }

            return resultat;
        }

        public UpdateBuilder<T> Update<T>()
            where T : IDtoBase
        {
            var dto = _dtoFactory.Get<T>();
            return new UpdateBuilder<T>(Connection, Transaction, dto);
        }

        public UpdateBuilder<T> UpdateById<T>(object id, string idName = "Id")
            where T : IDtoBase
        {
            var dto = _dtoFactory.Get<T>();
            return new UpdateBuilder<T>(Connection, Transaction, dto).WithWhere(idName, id);
        }

        public T Insert<T>(T item)
            where T : IDtoBase
        {
            if (item.IsAutoincrementId())
            {
                // Id wird durch autoincrement vergeben
                var insert = item.GetInsertCommand(true);
                var result = Connection.Query<int>(insert, item, Transaction).First();
                item.SetId(result);
            }
            else
            {
                // id muss selber vergeben werden und kann danach nicht über scope_identity abgefragt werden
                var insert = item.GetInsertCommand();
                var result = Connection.Execute(insert, item, Transaction);
                result.Should().Be(1, "Datensatz wurde nicht eingefügt");
            }

            item.Reload(Connection, Transaction);
            return item;
        }
    }
}