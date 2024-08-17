namespace LocalMod.Core.Logging;

internal class LoggerScope : IDisposable
{
    private readonly List<LoggerScope> _Owner;
    public readonly string Name;
    public LoggerScope(string name, List<LoggerScope> owner)
    {
        Name = name;
        _Owner = owner;
        _Owner.Add(this);
    }

    public void Dispose()
    {
        _Owner.Remove(this);
    }
}
