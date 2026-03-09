using affolterNET.Data.Commands;
using ExamplePgVersion.Data;
using Xunit;

namespace ExamplePgVersion.IntegrationTest.Commands
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
                    var singleEntry = db.Select<example_pg_version_demo_table>().ExecuteSingle();
                    return new DeleteEntityCommand<example_pg_version_demo_table>(singleEntry.Id, singleEntry.VersionTimestamp);
                })
                .ActAndAssert((result, ah) =>
                {
                    Assert.True(result.Data);
                });
        }
    }
}
