using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Extensions.DependencyInjection;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using LocalMod.Core.IoC;
using LocalMod.Core.Manifest;
using LocalMod.Core.NetAbstractions;
using LocalMod.Core.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SDG.Framework.Modules;
using UnityEngine.LowLevel;

namespace LocalMod.Core; 

internal class Entry : IModuleNexus 
{
    private IContainer? _Container;
    private ILogger? _Logger;

    private const string LocalModPath = "LocalMod";
    private const string ConfigurationPath = $"Configuration.json";
    private async UniTask<IConfiguration> CreateConfiguration()
    {
        if (!File.Exists(ConfigurationPath))
        {
            Assembly assembly = Assembly.GetAssembly(typeof(Entry));
            await ManifestHelper.CopyToFile(assembly, "LocalMod.Core.Configuration.json", ConfigurationPath);
        }

        ConfigurationBuilder builder = new();
        builder.AddJsonFile(Path.Combine(LocalModPath, ConfigurationPath));
        return builder.Build();
    }

    private async UniTask LoadAsync()
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        Directory.CreateDirectory(LocalModPath);
        Directory.SetCurrentDirectory(LocalModPath);
        Console.WriteLine(Directory.GetCurrentDirectory());

        ContainerBuilder builder = new();

        IConfiguration configuration = await CreateConfiguration();
        builder.RegisterInstance(configuration).As<IConfiguration>().SingleInstance();
        builder.RegisterInstance(new Harmony("LocalMod.Core")).As<Harmony>().SingleInstance();
        builder.Populate(new ServiceCollection());

        ServiceLoader.LoadServices(Assembly.GetExecutingAssembly(), builder);

        _Container = builder.Build(ContainerBuildOptions.ExcludeDefaultModules);

        NetMethodManager netMethodManager = _Container.Resolve<NetMethodManager>();

        Harmony harmony = _Container.Resolve<Harmony>();
        harmony.PatchAll();

        PluginLoader pluginLoader = _Container.Resolve<PluginLoader>();
        await pluginLoader.LoadPluginsAsync();

        _Logger = _Container.Resolve<ILogger<Entry>>();
        _Logger.LogInformation("Started LocalMod");
    }

    private async UniTask UnloadAsync()
    {
        if (_Container == null)
        {
            return;
        }

        _Container.Resolve<Harmony>().UnpatchAll();

        _Logger?.LogInformation("Shutting down LocalMod");
        await _Container.DisposeAsync();
    }

    private void InitializeUniTask()
    {
        if (PlayerLoopHelper.IsInjectedUniTaskPlayerLoop())
        {
            return;
        }

        PlayerLoopSystem system = PlayerLoop.GetCurrentPlayerLoop();
        PlayerLoopHelper.Initialize(ref system);
    }

    public void initialize()
    {
        InitializeUniTask();
        UniTask.RunOnThreadPool(LoadAsync);
    }

    public void shutdown()
    {
        UniTask.RunOnThreadPool(UnloadAsync);
    }
}
