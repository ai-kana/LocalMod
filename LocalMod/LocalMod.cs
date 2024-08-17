using System.Reflection;
using Cysharp.Threading.Tasks;
using LocalMod.Configuration;
using LocalMod.Logging;
using LocalMod.Plugins;
using Microsoft.Extensions.Logging;
using SDG.Unturned;

namespace LocalMod;

internal class LocalMod
{
    public const string LocalModDirectory = "./LocalMod";

    private const string ConfigName = "LocalMod";
    public LocalModConfig? Configuration;

    private LocalModLoggerProvider? LoggerProvider;
    private ILogger? _Logger;

    public async UniTask LoadAsync()
    {
        Configuration = await ConfigSaver.Load<LocalModConfig>(ConfigName);
        await ConfigSaver.Save(Configuration, ConfigName);

        LoggerProvider = new();
        _Logger = LoggerProvider.CreateLogger<LocalMod>();
        Logger.Init(_Logger, LoggerProvider);

        _Logger.LogInformation("Starting LocalMod...");

        NetReflection.RegisterFromAssembly(Assembly.GetExecutingAssembly());

        await PluginManager.LoadAsync();
    }

    public async UniTask UnloadAsync()
    {
        _Logger?.LogInformation("Shutdowning LocalMod...");
        await PluginManager.UnloadAsync();

        LoggerProvider!.Dispose();
    }
}
