using affolterNET.Data.TestHelpers;
using ExamplePgHistory.Data;
using Xunit.Abstractions;

namespace ExamplePgHistory.IntegrationTest
{
    public class IntegrationTest : IntegrationTestBase
    {
        public IntegrationTest(ExampleFixture dbFixture, ITestOutputHelper? output = null) : base(dbFixture,
            new DtoFactory(), output)
        {
        }
    }
}
