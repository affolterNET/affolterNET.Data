using System.Reflection;
using ExamplePgUserDate.Update.Commands;
using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(config =>
{
    config.PropagateExceptions();

    config.AddCommand<GenerateCommand>("generate")
        .WithAlias("gen")
        .WithDescription("Generates a CSharp File to be compiled as DTO-Classes.")
        .WithExample(new[] {$"dotnet run {Assembly.GetExecutingAssembly().FullName}.dll", "dbup", "Host=w;Database=x;Username=y;Password=z", "Namespace", "Targetfile"});

    config.AddCommand<DbUpdateCommand>("dbupdate")
        .WithAlias("dbup")
        .WithDescription("Updates the Db with all embedded scripts of the current assembly.")
        .WithExample(new[] {$"dotnet run {Assembly.GetExecutingAssembly().FullName}.dll", "dbup", "Host=w;Database=x;Username=y;Password=z"});
});

try
{
    var exitCode = app.Run(args);
    return exitCode;
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
    return -1;
}
