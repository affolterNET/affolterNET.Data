using System;
using System.Threading.Tasks;
using affolterNET.Data.Commands;
using affolterNET.Data.SessionHandler;
using ExamplePg.Data;
using Xunit;

namespace ExamplePg.IntegrationTest.SessionHandler;

[Collection(nameof(ExampleFixture))]
public class FailingCommandTest
{
    private readonly ExampleFixture _dbFixture;

    public FailingCommandTest(ExampleFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }

    [Fact]
    public async Task TestSessionHandler()
    {
        var connString = _dbFixture.GetConnString();
        var handler = new SqlSessionHandler(new SqlSessionFactory(new NpgsqlConnectionFactory(connString)));
        var dto = new example_pg_demo_table
        {
            Id = Guid.NewGuid(),
            Message = "new Entry",
            Status = "new_ new_ new_ new_ new_ new_ new_ new_ new_ new_ TOOMUCH",
            DateTest = new DateOnly(2023, 1, 1)
        };
        var cmd = new SaveEntityCommand<example_pg_demo_table>(dto);
        var result = await handler.QueryAsync(cmd);
        Assert.True(result.HasException);
        Assert.NotNull(result.Exception);
    }
}
