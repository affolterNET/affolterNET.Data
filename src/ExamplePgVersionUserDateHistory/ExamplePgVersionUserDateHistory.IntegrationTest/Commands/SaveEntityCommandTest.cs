using System;
using Npgsql;
using affolterNET.Data.Commands;
using affolterNET.Data.Models;
using ExamplePgVersionUserDateHistory.Data;
using Xunit;

namespace ExamplePgVersionUserDateHistory.IntegrationTest.Commands
{
    [Collection(nameof(ExampleFixture))]
    public class SaveEntityCommandTest : IntegrationTest
    {
        public SaveEntityCommandTest(ExampleFixture dbFixture) : base(dbFixture)
        {
        }

        [Fact]
        public void SaveEntityCommandTestOk()
        {
            CQB<SaveInfo>().Arrange(db =>
            {
                var dto = new example_pg_version_user_date_history_demo_table
                {
                    Id = Guid.NewGuid(),
                    Message = "I was inserted!",
                    TypeId = ExamplePgVersionUserDateHistoryDemoTableTypes.Drei
                };
                return new SaveEntityCommand<example_pg_version_user_date_history_demo_table>(dto, "tinu", true, example_pg_version_user_date_history_demo_table.Cols.Status);
            }).ActAndAssert((result, ah) =>
            {
                Assert.Equal("inserted", result.Data.Action);
            });
        }

        [Fact]
        public void SaveEntityCommandTestNOk()
        {
            var ex = Assert.Throws<PostgresException>(() => CQB<SaveInfo>().Arrange(db =>
            {
                var dto = new example_pg_version_user_date_history_demo_table
                {
                    Id = Guid.NewGuid()
                };
                return new SaveEntityCommand<example_pg_version_user_date_history_demo_table>(dto, "tinu", true, example_pg_version_user_date_history_demo_table.Cols.Status);
            }).Act());
            Assert.Contains("message", ex.Message);
        }
    }
}
