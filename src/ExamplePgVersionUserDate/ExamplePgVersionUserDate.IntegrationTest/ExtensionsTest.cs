using System;
using ExamplePgVersionUserDate.Data;
using Xunit;

namespace ExamplePgVersionUserDate.IntegrationTest;

public class ExtensionsTest
{
    [Fact]
    public void GetDemoTableTypesStringTestNok()
    {
        var g = Guid.NewGuid().GetExamplePgVersionUserDateDemoTableTypesString();
        Assert.Null(g);
    }

    [Fact]
    public void GetDemoTableTypesStringTestOk()
    {
        var g = ExamplePgVersionUserDateDemoTableTypes.Eins.GetExamplePgVersionUserDateDemoTableTypesString();
        Assert.Equal("Eins", g);
    }
}
