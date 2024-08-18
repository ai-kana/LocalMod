using Microsoft.Extensions.Logging;

namespace LocalMod.Core.Logging;

internal class LocalModLogger : ILogger
{
    private readonly LoggerWriter _Writer;
    private readonly List<LoggerScope> _Scopes;
    private readonly string _Name;

    internal LocalModLogger(string name, LoggerWriter writer)
    {
        _Writer = writer;
        _Scopes = new();
        _Name = name;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return new LoggerScope(state.ToString(), _Scopes);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel <= LogLevel.Trace;
    }

    private string GetLevelTag(LogLevel level) => level switch
    {
        LogLevel.Trace => "TRC",
        LogLevel.Critical => "CRT",
        LogLevel.Error => "ERR",
        LogLevel.Warning => "WRN",
        LogLevel.Debug => "DBG",
        LogLevel.Information => "INF",
        _ => "its broken"
    };

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        string scopeFormat = _Name;
        foreach (LoggerScope scope in _Scopes)
        {
            scopeFormat += $"::{scope.Name}";
        }

        string message = $"[{DateTime.Now.ToString()}] [{GetLevelTag(logLevel)}] [{scopeFormat}] -> {formatter(state, exception)}";
        _ = _Writer.Write(message);
    }
}
