namespace LocalMod.Core.Logging;

internal interface ILoggerWriter
{
    public void EnqueueMessage(string message);
}
