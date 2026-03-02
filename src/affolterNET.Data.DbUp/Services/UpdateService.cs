using System.ComponentModel;
using System.Data.Common;
using System.Reflection;
using affolterNET.Data.Extensions;
using affolterNET.Data.SessionHandler;
using DbUp;
using Spectre.Console;
using Spectre.Console.Cli;

namespace affolterNET.Data.DbUp.Services;

public class UpdateService
{
    // ReSharper disable once ClassNeverInstantiated.Global

    public class Settings: CommandSettings
    {
        [CommandArgument(0, "<connstring>")]
        public string ConnString { get; set; } = null!;

        [CommandOption("-c|--historyConnString")]
        [DefaultValue(null)]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string? HistoryConnString { get; set;  } 
        
        [CommandOption("-h|--historyMode")]
        [DefaultValue(EnumHistoryMode.None)]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public EnumHistoryMode HistoryMode{ get; set;  } 
        
        [CommandOption("-t|--historyTableName")]
        [DefaultValue("T_History")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string? HistoryTableName { get; set;  } 
        
        [CommandOption("-u|--historyUserName")]
        [DefaultValue("dbup")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string? HistoryUserName { get; set;  }

        [CommandOption("-p|--postgresql")]
        [DefaultValue(false)]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool PostgreSql { get; set; }
    }

    public async Task<int> UpdateDb(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine($"[blue]DbUpdateCommand[/]");
        var connectionString = settings.ConnString;
        var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };

        AnsiConsole.MarkupLine($"[orange3]ConnectionString: {connectionString}[/]");

        connectionString = builder.ConnectionString;
        await connectionString.WaitForDbConnectionAsync(useNpgsql: settings.PostgreSql);

        var ass = Assembly.GetEntryAssembly();
        var upg = (settings.PostgreSql
                ? DeployChanges.To.PostgresqlDatabase(connectionString)
                : DeployChanges.To.SqlDatabase(connectionString))
            .WithScriptsEmbeddedInAssembly(ass)
            .LogToConsole();
        if (settings.HistoryMode != EnumHistoryMode.None)
        {
            var saver = new HistorySaver(settings.HistoryConnString ?? connectionString, settings.HistoryMode, settings.HistoryTableName);
            var writer = new HistoryLog(saver, settings.HistoryUserName!);
            upg.LogTo(writer);
        }
        var upgrader = upg.Build();

        var updateResult = upgrader.PerformUpgrade();
        if (!updateResult.Successful)
        {
            throw new InvalidOperationException("db update failed", updateResult.Error);
        }

        return await Task.FromResult(0);
    }
}