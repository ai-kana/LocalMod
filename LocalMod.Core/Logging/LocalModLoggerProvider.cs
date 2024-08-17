using Microsoft.Extensions.Logging;

namespace LocalMod.Core.Logging;

internal class LocalModLoggerProvider : ILoggerProvider
{
    private LoggerWriter _Writer;

    public LocalModLoggerProvider()
    {
        _Writer = new();
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new LocalModLogger(categoryName, _Writer);
    }

    public ILogger CreateLogger<T>()
    {
        return CreateLogger(typeof(T).ToString());
    }

    public void Dispose()
    {
        _Writer.Dispose();
    }
}
