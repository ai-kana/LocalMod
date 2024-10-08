using Microsoft.Extensions.Logging;

namespace LocalMod.Core.Logging;

public static class LoggerExt
{
    public static ILogger CreateLogger<T>(this ILoggerProvider provider)
    {
        return provider.CreateLogger(typeof(T).ToString());
    }
}
