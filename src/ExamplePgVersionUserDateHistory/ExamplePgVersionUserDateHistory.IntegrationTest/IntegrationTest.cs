using affolterNET.Data.TestHelpers;
using ExamplePgVersionUserDateHistory.Data;
using Xunit.Abstractions;

namespace ExamplePgVersionUserDateHistory.IntegrationTest
{
    public class IntegrationTest : IntegrationTestBase
    {
        public IntegrationTest(ExampleFixture dbFixture, ITestOutputHelper? output = null) : base(dbFixture,
            new DtoFactory(), output)
        {
        }
    }
}
