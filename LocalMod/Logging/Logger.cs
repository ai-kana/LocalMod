using Microsoft.Extensions.Logging;

namespace LocalMod.Logging;

public static class Logger
{
    private static ILogger? _Logger;
    private static ILoggerProvider? _LoggerProvider;

    internal static void Init(ILogger logger, ILoggerProvider provider)
    {
        _Logger = logger;
        _LoggerProvider = provider;
    }

    public static ILogger CreateLogger(string name)
    {
        return _LoggerProvider!.CreateLogger(name);
    }

    public static ILogger CreateLogger<T>()
    {
        return _LoggerProvider!.CreateLogger<T>();
    }

    public static void LogInformation(object message, Exception? exception = null)
    {
        _Logger!.LogInformation(exception, message.ToString());
    }

    public static void LogWarning(object message, Exception? exception = null)
    {
        _Logger!.LogWarning(exception, message.ToString());
    }

    public static void LogCritical(object message, Exception? exception = null)
    {
        _Logger!.LogCritical(exception, message.ToString());
    }

    public static void LogError(object message, Exception? exception = null)
    {
        _Logger!.LogError(exception, message.ToString());
    }

    public static void LogDebug(object message, Exception? exception = null)
    {
        _Logger!.LogDebug(exception, message.ToString());
    }
}
