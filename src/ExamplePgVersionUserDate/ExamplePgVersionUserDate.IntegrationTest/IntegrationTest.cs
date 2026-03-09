using affolterNET.Data.TestHelpers;
using ExamplePgVersionUserDate.Data;
using Xunit.Abstractions;

namespace ExamplePgVersionUserDate.IntegrationTest
{
    public class IntegrationTest : IntegrationTestBase
    {
        public IntegrationTest(ExampleFixture dbFixture, ITestOutputHelper? output = null) : base(dbFixture,
            new DtoFactory(), output)
        {
        }
    }
}
