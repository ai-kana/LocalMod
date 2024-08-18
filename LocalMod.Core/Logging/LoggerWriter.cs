namespace LocalMod.Core.Logging;

internal class LoggerWriter : IDisposable
{
    private const string FileName = "Log";
    private const string FileExt = ".log";
    private const string LogName = FileName + FileExt;
    private const string LogDir = LocalModEntry.LocalModDirectory + "/Logs/";
    private const string LogPath = LogDir + LogName;

    private readonly FileStream _Stream;
    private readonly StreamWriter _Writer;
    private readonly SemaphoreSlim _Semaphore;

    public LoggerWriter() 
    {
        if (!Directory.Exists(LogDir))
        {
            Directory.CreateDirectory(LogDir);
        }

        if (File.Exists(LogPath))
        {
            SaveFile();
        }

        //DeleteFiles();

        _Stream = new(LogPath, FileMode.Create, FileAccess.Write);
        _Writer = new(_Stream);
        _Semaphore = new(1, 1);
    }

    private void DeleteFiles()
    {
        IEnumerable<string> files = Directory.GetFiles(LogDir).Where(x => File.GetLastWriteTime(x).Day > 5);
        foreach (string file in files)
        {
            File.Delete(file);
        }
    }

    private void SaveFile()
    {
        string newName = LogDir + FileName + DateTime.UtcNow.ToString("MM-dd-yyyy_hh-mm-ss-tt") + FileExt;
        File.Copy(LogPath, newName);
        File.Delete(LogPath);
    }

    public async Task Write(string message)
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(LoggerWriter));
        }

        await _Semaphore.WaitAsync();
        await _Writer.WriteLineAsync(message);
        await _Writer.FlushAsync();
        _Semaphore.Release();
    }

    private bool IsDisposed = false;
    public void Dispose()
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(LoggerWriter));
        }

        _Semaphore.WaitAsync();

        IsDisposed = true;
        GC.SuppressFinalize(this);

        _Semaphore.Dispose();
        _Writer.DisposeAsync();
        _Stream.DisposeAsync();

        SaveFile();
    }
}
