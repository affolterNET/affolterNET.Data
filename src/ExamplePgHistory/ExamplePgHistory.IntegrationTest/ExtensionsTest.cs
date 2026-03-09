using System;
using ExamplePgHistory.Data;
using Xunit;

namespace ExamplePgHistory.IntegrationTest;

public class ExtensionsTest
{
    [Fact]
    public void GetDemoTableTypesStringTestNok()
    {
        var g = Guid.NewGuid().GetExamplePgHistoryDemoTableTypesString();
        Assert.Null(g);
    }

    [Fact]
    public void GetDemoTableTypesStringTestOk()
    {
        var g = ExamplePgHistoryDemoTableTypes.Eins.GetExamplePgHistoryDemoTableTypesString();
        Assert.Equal("Eins", g);
    }
}
