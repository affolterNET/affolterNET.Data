using affolterNET.Data.TestHelpers;
using ExamplePgVersion.Data;
using Xunit.Abstractions;

namespace ExamplePgVersion.IntegrationTest
{
    public class IntegrationTest : IntegrationTestBase
    {
        public IntegrationTest(ExampleFixture dbFixture, ITestOutputHelper? output = null) : base(dbFixture,
            new DtoFactory(), output)
        {
        }
    }
}
