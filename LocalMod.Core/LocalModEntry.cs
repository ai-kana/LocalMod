using System.Reflection;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using LocalMod.Bootstrapper;
using LocalMod.Core.Configuration;
using LocalMod.Core.Logging;
using LocalMod.Core.NetAbstractions;
using LocalMod.Core.Plugins;
using Microsoft.Extensions.Logging;

namespace LocalMod.Core;

public class LocalModEntry : ILocalModEntry
{
    public const string LocalModDirectory = "./LocalMod";

    private const string ConfigName = "LocalMod";
    internal LocalModConfig? Configuration = new();

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

        //NetMethodManager.RegisterFromType(types);
        NetMethodManager.RegisterNetMethods(Assembly.GetExecutingAssembly());

        await PluginManager.LoadAsync();
        NetMethodManager.DumpRPCs();
    }

    public async UniTask UnloadAsync()
    {
        await ConfigSaver.Save(Configuration!, ConfigName);

        _Logger?.LogInformation("Shutdowning LocalMod...");
        await PluginManager.UnloadAsync();

        _Harmony?.UnpatchAll();

        LoggerProvider!.Dispose();
    }
}
