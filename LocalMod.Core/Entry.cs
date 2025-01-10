using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Extensions.DependencyInjection;
using HarmonyLib;
using LocalMod.Core.IoC;
using LocalMod.Core.NetAbstractions;
using LocalMod.Core.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SDG.Framework.Modules;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace LocalMod.Core; 

internal class Entry : IModuleNexus 
{
    private IContainer? _Container;
    private ILogger? _Logger;

    private const string LocalModPath = "LocalMod";
    private const string ConfigurationPath = $"{LocalModPath}/Configuration.json";
    private IConfiguration CreateConfiguration()
    {
        if (!File.Exists(ConfigurationPath))
        {
            CreateConfigurationFile();
        }

        ConfigurationBuilder builder = new();
        builder.AddJsonFile(ConfigurationPath);
        return builder.Build();
    }

    private void CreateConfigurationFile()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(Entry));
        using StreamReader manifest = new(assembly.GetManifestResourceStream("LocalMod.Core.Configuration.json"));
        using StreamWriter config = new(ConfigurationPath);

        string content = manifest.ReadToEnd();

        config.Write(content);
    }

    public void initialize()
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        Directory.CreateDirectory(LocalModPath);

        ContainerBuilder builder = new();

        IConfiguration configuration = CreateConfiguration();
        builder.RegisterInstance(configuration).As<IConfiguration>().SingleInstance();
        builder.Register(c => new Harmony("LocalMod.Core")).As<Harmony>().SingleInstance();
        builder.Populate(new ServiceCollection());

        ServiceLoader.LoadServices(Assembly.GetExecutingAssembly(), builder);

        _Container = builder.Build(ContainerBuildOptions.ExcludeDefaultModules);

        NetMethodManager netMethodManager = _Container.Resolve<NetMethodManager>();
        netMethodManager.RegisterFromAssembly(Assembly.GetExecutingAssembly());

        Harmony harmony = _Container.Resolve<Harmony>();
        harmony.PatchAll();

        PluginLoader pluginLoader = _Container.Resolve<PluginLoader>();
        pluginLoader.LoadPlugins();

        _Logger = _Container.Resolve<ILogger<Entry>>();
        _Logger.LogInformation("Started LocalMod");
    }

    public void shutdown()
    {
        if (_Container == null)
        {
            return;
        }

        _Container.Resolve<Harmony>().UnpatchAll();

        _Logger?.LogInformation("Shutting down LocalMod");
        _Container.Dispose();
    }
}
