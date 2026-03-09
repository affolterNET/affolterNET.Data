using affolterNET.Data.TestHelpers;
using ExamplePgUserDate.Data;
using Xunit.Abstractions;

namespace ExamplePgUserDate.IntegrationTest
{
    public class IntegrationTest : IntegrationTestBase
    {
        public IntegrationTest(ExampleFixture dbFixture, ITestOutputHelper? output = null) : base(dbFixture,
            new DtoFactory(), output)
        {
        }
    }
}
