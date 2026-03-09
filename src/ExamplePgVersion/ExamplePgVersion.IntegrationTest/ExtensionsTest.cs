using System;
using ExamplePgVersion.Data;
using Xunit;

namespace ExamplePgVersion.IntegrationTest;

public class ExtensionsTest
{
    [Fact]
    public void GetDemoTableTypesStringTestNok()
    {
        var g = Guid.NewGuid().GetExamplePgVersionDemoTableTypesString();
        Assert.Null(g);
    }

    [Fact]
    public void GetDemoTableTypesStringTestOk()
    {
        var g = ExamplePgVersionDemoTableTypes.Eins.GetExamplePgVersionDemoTableTypesString();
        Assert.Equal("Eins", g);
    }
}
