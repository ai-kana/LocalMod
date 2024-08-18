using Cysharp.Threading.Tasks;
using HarmonyLib;
using LocalMod.Bootstrapper;
using LocalMod.Core.Configuration;
using LocalMod.Core.Logging;
using LocalMod.Core.Plugins;
using LocalMod.NetAbstractions;
using Microsoft.Extensions.Logging;

namespace LocalMod.Core;

public class LocalModEntry : ILocalModEntry
{
    public const string LocalModDirectory = "./LocalMod";

    private const string ConfigName = "LocalMod";
    internal LocalModConfig? Configuration;

    private LocalModLoggerProvider? LoggerProvider;
    private Harmony? _Harmony;
    private ILogger? _Logger;

    public async UniTask LoadAsync()
    {
        Configuration = await ConfigSaver.Load<LocalModConfig>(ConfigName);
        await ConfigSaver.Save(Configuration, ConfigName);

        LoggerProvider = new();
        _Logger = LoggerProvider.CreateLogger<LocalModEntry>();
        Logger.Init(_Logger, LoggerProvider);
        _Logger.LogInformation("Starting LocalMod...");

        _Harmony = new("LocalMod");
        _Harmony.PatchAll();

        List<Type> types = new();
        types.Add(typeof(TestRPC));
        NetMethodManager.RegisterFromType(types);
        NetMethodManager.LogAllRPCs();

        await PluginManager.LoadAsync();
    }

    public async UniTask UnloadAsync()
    {
        _Logger?.LogInformation("Shutdowning LocalMod...");
        await PluginManager.UnloadAsync();

        _Harmony?.UnpatchAll();

        LoggerProvider!.Dispose();
    }
}
