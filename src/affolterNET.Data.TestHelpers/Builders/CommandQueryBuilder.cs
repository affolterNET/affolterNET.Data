using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using affolterNET.Data.Interfaces;
using affolterNET.Data.Result;
using affolterNET.Data.TestHelpers.Helpers;
using affolterNET.Data.TestHelpers.Interfaces;
using Xunit;

namespace affolterNET.Data.TestHelpers.Builders
{
    public class CommandQueryBuilder<TResult>
    {
        private readonly AssertHelper _assertHelper;

        private readonly bool _checkParameters;

        private readonly DbOperations _dbOperations;

        private Func<DbOperations, IQuery<TResult>>? _arrange;

        private Action<DbOperations>? _arrangeSimple;

        public CommandQueryBuilder(DbFixture dbFixture, IDtoFactory dtoFactory, bool checkParameters = true)
        {
            Connection = dbFixture.Connection ??
                         throw new InvalidOperationException($"{nameof(dbFixture.Connection)} was null");
            Transaction = dbFixture.Transaction ??
                          throw new InvalidOperationException($"{nameof(dbFixture.Transaction)} was null");
            _dbOperations = new DbOperations(Connection, Transaction, dtoFactory);
            _assertHelper = new AssertHelper(_dbOperations, dtoFactory);
            _checkParameters = checkParameters;
        }

        public IConnectionDecorator Connection { get; }

        public IDbTransaction Transaction { get; }


        public void Rollback()
        {
            Connection.RollbackTestTransaction();
            if (!_assertHelper.InputChecked && _checkParameters)
            {
                Assert.Fail("Input Parameter wurden nicht überprüft");
            }
        }

        [DebuggerStepThrough]
        public CommandQueryBuilder<TResult> Arrange(Func<DbOperations, IQuery<TResult>> arr)
        {
            _arrange = arr;
            return this;
        }

        [DebuggerStepThrough]
        public CommandQueryBuilder<TResult> ActAndAssert(Action<DataResult<TResult>, AssertHelper> act)
        {
            var result = Act();
            act(result, _assertHelper);
            return this;
        }

        [DebuggerStepThrough]
        public CommandQueryBuilder<TResult> DbAssert(Action<AssertHelper> assert)
        {
            assert(_assertHelper);
            return this;
        }

        [DebuggerStepThrough]
        public CommandQueryBuilder<TResult> ArrangeSimple(Action<DbOperations> arr)
        {
            _arrangeSimple = arr;
            return this;
        }

        public DataResult<TResult> Act()
        {
            // zum sicher sein
            _assertHelper.InputChecked = false;

            // ReSharper disable once UseNullPropagation
            if (_arrangeSimple != null)
            {
                _arrangeSimple(_dbOperations);
            }

            if (_arrange != null)
            {
                var query = _arrange(_dbOperations);
                _assertHelper.SetParams(query.ParamsDict);
                var result = Task.Run(() => query.ExecuteAsync(Connection, Transaction)).GetAwaiter().GetResult();
                result.SqlCommand = query.ToString();
                return result;
            }

            return default!;
        }

        public void Commit()
        {
            Transaction.Commit();
        }
    }
}