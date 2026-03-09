using System;
using ExamplePgVersionUserDateHistory.Data;
using Xunit;

namespace ExamplePgVersionUserDateHistory.IntegrationTest;

public class ExtensionsTest
{
    [Fact]
    public void GetDemoTableTypesStringTestNok()
    {
        var g = Guid.NewGuid().GetExamplePgVersionUserDateHistoryDemoTableTypesString();
        Assert.Null(g);
    }

    [Fact]
    public void GetDemoTableTypesStringTestOk()
    {
        var g = ExamplePgVersionUserDateHistoryDemoTableTypes.Eins.GetExamplePgVersionUserDateHistoryDemoTableTypesString();
        Assert.Equal("Eins", g);
    }
}
