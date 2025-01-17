using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;
using LocalMod.API.IoC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SDG.Unturned;

namespace LocalMod.Core.Logging;

[Service(ServiceLifetime.Singleton, typeof(ILoggerWriter))]
internal class LoggerWriter : IDisposable, ILoggerWriter
{
    private readonly IConfigurationSection _Logging;

    private readonly string _Path;
    private readonly string _Name;
    private readonly string _Extension;

    private string _FullPath => $"{_Path}/{_Name}{_Extension}";
    private string _SavePath => $"{_Path}/{_Name}-{DateTime.Now.ToString("MM-dd-yyyy_hh-mm-ss-tt")}{_Extension}";
    
    private readonly TextWriter _FileWriter;
    private readonly TextWriter _ConsoleWriter;
    private readonly SemaphoreSlim _Semaphore; 

    public LoggerWriter(IConfiguration configuration)
    {
        _Logging = configuration.GetRequiredSection("Logging");
        string path = _Logging.GetValue<string>("LogFilePath") ?? throw new KeyNotFoundException("Missing log file path");

        _Path = Path.GetDirectoryName(path);
        _Name = Path.GetFileNameWithoutExtension(path);
        _Extension = Path.GetExtension(path);

        Directory.CreateDirectory(_Path);

        ClearOldFiles();
        SaveFile();
        _FileWriter = new StreamWriter(File.Create(_FullPath));
        _ConsoleWriter = Console.Out;
        _Semaphore = new(1, 1);
    }

    public void Dispose()
    {
        _FileWriter.Dispose();
        _ConsoleWriter.Dispose();
        _Semaphore.Dispose();
        SaveFile();
    }

    private void ClearOldFiles()
    {
        uint age = _Logging.GetValue<uint>("MaxArchiveAgeDays");
        foreach (string log in Directory.GetFiles(_Path))
        {
            if (File.GetCreationTimeUtc(log).Day > age)
            {
                File.Delete(log);
            }
        }
    }

    private void SaveFile()
    {
        if (!File.Exists(_FullPath))
        {
            return;
        }

        File.Copy(_FullPath, _SavePath);
    }

    public void EnqueueMessage(string message)
    {
        _MessageQueue.Enqueue(message);
        _ = WriteMessages();
    }

    private ConcurrentQueue<string> _MessageQueue = new();
    private bool _IsWriting = false;
    internal async UniTask WriteMessages()
    {
        if (_IsWriting)
        {
            return;
        }
        _IsWriting = true;

        while (_MessageQueue.TryDequeue(out string message))
        {
            try
            {
                await _FileWriter.WriteLineAsync(message);
                await _ConsoleWriter.WriteLineAsync(message);
            }
            catch (Exception ex)
            {
                UnturnedLog.exception(ex);
            }
        }

        _IsWriting = false;
        await _FileWriter.FlushAsync();
        await _ConsoleWriter.FlushAsync();
    }
}
