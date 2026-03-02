using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using affolterNET.Data.Commands;
using affolterNET.Data.Queries;
using affolterNET.Data.SessionHandler;
using Dapper;
using ExampleVersionUserDateHistory.Data;
using Xunit;
using Xunit.Abstractions;

namespace ExampleVersionUserDateHistory.IntegrationTest.Commands;

[Collection(nameof(ExampleFixture))]
public class HistorySaverTest: IntegrationTest
{
    private readonly ITestOutputHelper _log;
    public const string HistoryTableName = "T_TestHistoryTable";
    private readonly SqlSessionHandler _sessionHandler;
    private readonly string _connString;

    public HistorySaverTest(ExampleFixture dbFixture, ITestOutputHelper log) : base(dbFixture)
    {
        _log = log;
        _connString = Fixture.GetConnString();
        var sessionFactory = new SqlSessionFactory(new SqlServerConnectionFactory(_connString));
        var historySaver = new HistorySaver(_connString, EnumHistoryMode.CommandsOnlyAndCheck, HistoryTableName);
        _sessionHandler = new SqlSessionHandler(sessionFactory, historySaver);
    }

    [Fact]
    public void CommandsIncludedInHistory()
    {
        var qry = new SaveEntityCommand<ExampleVersionUserDateHistory_T_DemoTable>(new ExampleVersionUserDateHistory_T_DemoTable());
        Assert.False(qry.ExcludeFromHistory);
    }
    
    [Fact]
    public void QueriesExcludedFromHistory()
    {
        var qry = new LoadEntityQuery<ExampleVersionUserDateHistory_T_DemoTable>();
        Assert.True(qry.ExcludeFromHistory);
    }

    [Fact]
    public async Task TestHistoryEntry()
    {
        // timing is important - set breakpoints carefully
        const string txt = "From Insert Test";
        try
        {
            var toInsert = new ExampleVersionUserDateHistory_T_DemoTableType
            {
                Id = Guid.NewGuid(),
                Name = txt
            };
            var from = DateTime.UtcNow;
            var cmd = new SaveEntityCommand<ExampleVersionUserDateHistory_T_DemoTableType>(toInsert);
            var result = await _sessionHandler.QueryAsync(cmd);
            Assert.True(result.IsSuccessful);
            var to = DateTime.UtcNow.AddMilliseconds(200); // otherwise it could be faster than the db entry
            var cn = new SqlConnection(_connString);
            var entries =
                await cn.QueryAsync($"select * from {HistoryTableName} where Applied > @From and Applied < @To",
                    new { From = from, To = to });
            _log.WriteLine("From: {0:O}; To: {1:O}", from, to);
            var entry = Assert.Single(entries);
            Assert.Equal("SaveEntityCommand<ExampleVersionUserDateHistory_T_DemoTableType>", entry.Name);
        }
        finally
        {
            try
            {
                var cn = new SqlConnection(_connString);
                await cn.ExecuteAsync($"drop table {HistoryTableName}");
                await cn.ExecuteAsync($"delete from {ExampleVersionUserDateHistory_T_DemoTableType.TABLE_NAME} where Name = @Name",
                    new { Name = txt });
            }
            catch
            {
                _log.WriteLine("cleanup failed");
            }
        }
    }
}