using affolterNET.Data.TestHelpers;
using ExamplePg.Data;
using Xunit.Abstractions;

namespace ExamplePg.IntegrationTest
{
    public class IntegrationTest : IntegrationTestBase
    {
        public IntegrationTest(ExampleFixture dbFixture, ITestOutputHelper? output = null) : base(dbFixture,
            new DtoFactory(), output)
        {
        }
    }
}
