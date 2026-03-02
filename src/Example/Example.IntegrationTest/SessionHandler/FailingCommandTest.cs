using System;
using System.Threading.Tasks;
using affolterNET.Data.Commands;
using affolterNET.Data.SessionHandler;
using Example.Data;
using Xunit;

namespace Example.IntegrationTest.SessionHandler;

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
        // ;        services.AddScoped<ISqlSessionHandler, SqlSessionHandler>();
        // services.AddTransient<ISqlSession, SqlSession>();
        // services.AddSingleton<ISqlSessionFactory>((Func<IServiceProvider, ISqlSessionFactory>) (provider => (ISqlSessionFactory) new SqlSessionFactory(connString)));
        var handler = new SqlSessionHandler(new SqlSessionFactory(new SqlServerConnectionFactory(connString)));
        var dto = new Example_T_DemoTable
        {
            Id = Guid.NewGuid(),
            Message = "new Entry",
            Status = "new_ new_ new_ new_ new_ new_ new_ new_ new_ new_ TOOMUCH",
            DateTest = new DateOnly(2023, 1, 1)
        };
        var cmd = new SaveEntityCommand<Example_T_DemoTable>(dto);
        var result = await handler.QueryAsync(cmd);
        Assert.True(result.HasException);
        Assert.NotNull(result.Exception);
        Assert.StartsWith("String or binary data would be truncated in table 'example.Example.T_DemoTable'", result.Exception!.Message);
    }
}