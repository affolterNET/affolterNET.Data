using Microsoft.Data.SqlClient;
using System.Reflection;
using affolterNET.Data.Interfaces.SessionHandler;
using DbUp.Engine.Output;

namespace affolterNET.Data.DbUp.Services;

public class HistoryLog: IUpgradeLog
{
    private readonly IHistorySaver _historySaver;
    private readonly string _user;

    public HistoryLog(IHistorySaver historySaver, string user)
    {
        _historySaver = historySaver;
        _user = user;
    }
    
    public void LogTrace(string format, params object[] args)
    { }

    public void LogDebug(string format, params object[] args)
    { }

    public void LogInformation(string format, params object[] args)
    {
        if (format.StartsWith("Executing Database Server script '{0}'"))
        {
            if (args.Length < 1 || args.FirstOrDefault() == null)
            {
                throw new InvalidOperationException("filename not found");
            }

            var fileName = args.First().ToString();
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new InvalidOperationException("first argument with fileName was empty");
            }

            var contents = ReadContents(fileName);
            _historySaver.SaveHistory(fileName, contents, _user).GetAwaiter().GetResult();
        }
    }

    public void LogWarning(string format, params object[] args)
    { }

    public void LogError(string format, params object[] args)
    { }

    public void LogError(Exception ex, string format, params object[] args)
    { }

    private string ReadContents(string fileName)
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null)
        {
            throw new InvalidOperationException("could not get entry assembly");
        }

        using var stream = assembly.GetManifestResourceStream(fileName);
        if (stream == null)
        {
            throw new InvalidOperationException($"file stream {fileName} was null");
        }
        using var reader = new StreamReader(stream);
        var result = reader.ReadToEnd();
        return result;
    }
}