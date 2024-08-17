using System.Reflection;
using Cysharp.Threading.Tasks;
using LocalMod.Bootstrapper;
using LocalMod.Core.Configuration;
using LocalMod.Core.Logging;
using LocalMod.Core.Plugins;
using Microsoft.Extensions.Logging;
using SDG.Unturned;

namespace LocalMod.Core;

public class LocalModEntry : ILocalModEntry
{
    public const string LocalModDirectory = "./LocalMod";

    private const string ConfigName = "LocalMod";
    internal LocalModConfig? Configuration;

    private LocalModLoggerProvider? LoggerProvider;
    private ILogger? _Logger;

    public async UniTask LoadAsync()
    {
        Configuration = await ConfigSaver.Load<LocalModConfig>(ConfigName);
        await ConfigSaver.Save(Configuration, ConfigName);

        LoggerProvider = new();
        _Logger = LoggerProvider.CreateLogger<LocalModEntry>();
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
