using System;
using affolterNET.Data.Interfaces;
using affolterNET.Data.Interfaces.SessionHandler;
using affolterNET.Data.TestHelpers.Builders;
using Xunit;
using Xunit.Abstractions;

namespace affolterNET.Data.TestHelpers
{
    [Trait("Category", "Integration")]
    public abstract class IntegrationTestBase : IDisposable
    {
        private readonly IDtoFactory _dtoFactory;

        private readonly DateTime _startTime;
        protected readonly DbFixture Fixture;

        private DbOperations? _ops;

        protected IntegrationTestBase(DbFixture dbFixture, IDtoFactory dtoFactory, ITestOutputHelper? output = null)
        {
            Fixture = dbFixture;
            _dtoFactory = dtoFactory;
            _startTime = DateTime.Now;

            dbFixture.StartTest();
        }

        protected ISqlSessionHandler TestSqlSessionHandler => Fixture.SqlSessionHandler;

        public DbOperations TestOperations
        {
            get
            {
                if (_ops == null)
                {
                    if (Fixture.Connection == null)
                    {
                        throw new InvalidOperationException($"{nameof(Fixture.Connection)} was null");
                    }
                    if (Fixture.Transaction == null)
                    {
                        throw new InvalidOperationException($"{nameof(Fixture.Transaction)} was null");
                    }

                    _ops = new DbOperations(
                        Fixture.Connection,
                        Fixture.Transaction,
                        _dtoFactory);
                }

                return _ops;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public CommandQueryBuilder<TResult> CQB<TResult>(bool checkParameters = true)
        {
            return new CommandQueryBuilder<TResult>(Fixture, _dtoFactory, checkParameters);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Fixture.EndTest();

                var endTime = DateTime.Now;

                // ReSharper disable once UnusedVariable
                var t = endTime - _startTime;

                // Schöne Ausgabe
                // var prettyString = t.ToPrettyString(1, UnitStringRepresentation.Long, TimeSpanUnit.Minutes, TimeSpanUnit.Milliseconds);
                // prettyString = $"{Environment.NewLine}Duration: {prettyString}{Environment.NewLine}";
                // Console.WriteLine(prettyString);
                // Debug.WriteLine(prettyString);
            }
        }
    }
}