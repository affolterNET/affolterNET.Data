using System;
using ExamplePg.Data;
using Xunit;

namespace ExamplePg.IntegrationTest;

public class ExtensionsTest
{
    [Fact]
    public void GetDemoTableTypesStringTestNok()
    {
        var g = Guid.NewGuid().GetExamplePgDemoTableTypesString();
        Assert.Null(g);
    }

    [Fact]
    public void GetDemoTableTypesStringTestOk()
    {
        var g = ExamplePgDemoTableTypes.Eins.GetExamplePgDemoTableTypesString();
        Assert.Equal("Eins", g);
    }
}
