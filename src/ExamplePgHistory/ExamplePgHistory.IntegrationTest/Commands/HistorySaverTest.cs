using affolterNET.Data.Commands;
using affolterNET.Data.Queries;
using ExamplePgHistory.Data;
using Xunit;
using Xunit.Abstractions;

namespace ExamplePgHistory.IntegrationTest.Commands;

[Collection(nameof(ExampleFixture))]
public class HistorySaverTest: IntegrationTest
{
    public HistorySaverTest(ExampleFixture dbFixture, ITestOutputHelper log) : base(dbFixture)
    {
    }

    [Fact]
    public void CommandsIncludedInHistory()
    {
        var qry = new SaveEntityCommand<example_pg_history_demo_table>(new example_pg_history_demo_table());
        Assert.False(qry.ExcludeFromHistory);
    }

    [Fact]
    public void QueriesExcludedFromHistory()
    {
        var qry = new LoadEntityQuery<example_pg_history_demo_table>();
        Assert.True(qry.ExcludeFromHistory);
    }
}
