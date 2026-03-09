using affolterNET.Data.Commands;
using ExamplePgHistory.Data;
using Xunit;

namespace ExamplePgHistory.IntegrationTest.Commands
{
    [Collection(nameof(ExampleFixture))]
    public class DeleteEntityCommandTest: IntegrationTest
    {
        public DeleteEntityCommandTest(ExampleFixture dbFixture) : base(dbFixture)
        { }

        [Fact]
        public void DeleteByIdTest()
        {
            CQB<bool>()
                .Arrange(db =>
                {
                    var singleEntry = db.Select<example_pg_history_demo_table>().ExecuteSingle();
                    return new DeleteEntityCommand<example_pg_history_demo_table>(singleEntry.Id);
                })
                .ActAndAssert((result, ah) =>
                {
                    Assert.True(result.Data);
                });
        }
    }
}
