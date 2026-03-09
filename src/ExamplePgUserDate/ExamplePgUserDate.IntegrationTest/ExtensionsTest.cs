using System;
using ExamplePgUserDate.Data;
using Xunit;

namespace ExamplePgUserDate.IntegrationTest;

public class ExtensionsTest
{
    [Fact]
    public void GetDemoTableTypesStringTestNok()
    {
        var g = Guid.NewGuid().GetExamplePgUserDateDemoTableTypesString();
        Assert.Null(g);
    }

    [Fact]
    public void GetDemoTableTypesStringTestOk()
    {
        var g = ExamplePgUserDateDemoTableTypes.Eins.GetExamplePgUserDateDemoTableTypesString();
        Assert.Equal("Eins", g);
    }
}
