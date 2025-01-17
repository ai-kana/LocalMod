using LocalMod.API.IoC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LocalMod.Core.Logging;

[Service(ServiceLifetime.Scoped, typeof(ILogger<>))]
internal class Logger<T> : ILogger<T>
{
    private readonly IConfigurationSection _Logging;
    private readonly ILoggerWriter _Writer;
    private readonly string _Name;

    public Logger(IConfiguration configuration, ILoggerWriter writer)
    {
        _Logging = configuration.GetRequiredSection("Logging");
        _Writer = writer;
        _Name = typeof(T).FullName;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    { 
        throw new NotImplementedException();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return _Logging.GetValue<LogLevel>("LogLevel") <= logLevel;
    }

    private const string Red = "\u001b[31m";
    private const string Yellow = "\u001b[33m";
    private const string White = "\u001b[37m";
    private string GetLevelTag(LogLevel level) => level switch
    {
        LogLevel.Trace => "TRC",
        LogLevel.Debug => "DBG",
        LogLevel.Information => "INF",
        LogLevel.Warning => "WRN",
        LogLevel.Error => "ERR",
        LogLevel.Critical => "CRT",
        _ => "",
    };

    private string Formatter(string message, Exception? exception)
    {
        return $"{message}{(exception == null ? "" : '\n' + exception.ToString())}";
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        bool enabled = IsEnabled(logLevel);
        if (!enabled)
        {
            return;
        }

        string formatted = Formatter(state?.ToString() ?? "", exception);
        string message = $"[{DateTime.Now}][{GetLevelTag(logLevel)}][{_Name}] >> {formatted}";
        _Writer.EnqueueMessage(message);
    }
}
