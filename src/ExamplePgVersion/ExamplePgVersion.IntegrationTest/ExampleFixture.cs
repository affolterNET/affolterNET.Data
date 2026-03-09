using affolterNET.Data.TestHelpers;
using Xunit;

namespace ExamplePgVersion.IntegrationTest
{
    public class ExampleFixture : PgDbFixture
    {
        public ExampleFixture() : base("CONNSTRING_PG", "83694dd8-458d-4674-af47-af19a35a4527")
        {
        }
    }

    [CollectionDefinition(nameof(ExampleFixture))]
    public class IntegrationTestCollection : ICollectionFixture<ExampleFixture>
    {
    }
}
