using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Extensions.DependencyInjection;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using LocalMod.API.IoC;
using LocalMod.API.Plugins;
using LocalMod.Core.IoC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LocalMod.Core.Plugins;

[Service(ServiceLifetime.Singleton)]
internal class PluginLoader : IDisposable
{
    private readonly struct PluginData
    {
        public readonly IContainer Container;
        public readonly string Name;

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public PluginData(IContainer container, string name)
        {
            Container = container;
            Name = name;
        }
    }

    private readonly HashSet<PluginData> _Plugins;
    private readonly ILogger _Logger;
    private readonly IServiceProvider _Provider;
    private readonly IConfiguration _Congfiguration;

    public PluginLoader(IServiceProvider provider, IConfiguration configuration, ILogger<PluginLoader> logger)
    {
        _Plugins = new();
        _Logger = logger;
        _Provider = provider;
        _Congfiguration = configuration;
    }

    public void Dispose()
    {
        UnloadPlugins();
    }

    internal void LoadPlugins()
    {
        string path = _Congfiguration.GetValue<string>("PluginPath") ?? throw new KeyNotFoundException("No plugin path set");
        Directory.CreateDirectory(path);

        foreach (string directory in Directory.GetDirectories(path))
        foreach (string file in Directory.GetFiles(directory))
        {
            if (!file.EndsWith(".dll"))
            {
                continue;
            }

            string fullpath = Path.GetFullPath(file);
            LoadPlugin(Assembly.LoadFile(fullpath));
        }
    }

    private PluginData? GetPluginDataFromName(string name)
    {
        foreach (PluginData data in _Plugins)
        {
            if (data.Name.Contains(name, StringComparison.InvariantCultureIgnoreCase))
            {
                return data;
            }
        }

        return null;
    }

    internal void UnloadPlugin(string name)
    {
        PluginData? data = GetPluginDataFromName(name);
        if (data == null)
        {
            return;
        }

        UnloadPlugin(data.Value);
    }

    private void UnloadPlugin(PluginData data, bool remove = true)
    {
        IPlugin plugin = data.Container.Resolve<IPlugin>();
        Harmony harmony = data.Container.Resolve<Harmony>();
        harmony.UnpatchAll(harmony.Id);

        plugin.UnloadAsync().Forget(PluginUnloadExceptionHandler);
        _Logger.LogInformation($"Unloaded {plugin.Name}");

        if (remove)
        {
            _Plugins.Remove(data);
        }
    }

    internal void UnloadPlugins()
    {
        foreach (PluginData data in _Plugins)
        {
            UnloadPlugin(data, false);
        }

        _Plugins.Clear();
    }

    private void LoadPlugin(Assembly assembly)
    {
        Type pluginType = assembly.GetTypes().FirstOrDefault(x => x.GetInterfaces().Contains(typeof(IPlugin)));
        Type configurationType = assembly.GetTypes().FirstOrDefault(x => x.GetInterfaces().Contains(typeof(IContainerConfiguring)));
        if (pluginType == null)
        {
            _Logger.LogWarning($"{assembly.FullName} does not contain an implemenation of IPlugin");
            return;
        }

        ContainerBuilder builder = new();
        ServiceLoader.LoadServices(assembly, builder);

        Type loggerType = typeof(ILogger<>).MakeGenericType(pluginType);
        ILogger logger = (ILogger)_Provider.GetRequiredService(loggerType);

        builder.RegisterInstance(logger).As(loggerType).As<ILogger>().SingleInstance();
        builder.RegisterInstance(new Harmony(pluginType.FullName)).As<Harmony>().SingleInstance();
        builder.RegisterType(pluginType).As<IPlugin>().SingleInstance();
        builder.Populate(new ServiceCollection());

        if (configurationType != null)
        {
            IContainerConfiguring configurator = (IContainerConfiguring)Activator.CreateInstance(configurationType);
            configurator.OnConfiguring(builder);
        }

        IContainer container = builder.Build(ContainerBuildOptions.ExcludeDefaultModules);
        container.Resolve<Harmony>().PatchAll();

        StartPluginAsync(container).Forget(PluginLoadExceptionHandler);
    }

    private void PluginLoadExceptionHandler(Exception exception)
    {
        _Logger.LogError(exception, "Error while loading plugin");
    }

    private void PluginUnloadExceptionHandler(Exception exception)
    {
        _Logger.LogError(exception, "Error while unloading plugin");
    }

    private async UniTask StartPluginAsync(IContainer container)
    {
        IPlugin plugin = container.Resolve<IPlugin>();
        bool loaded = false;
        try
        {
            await plugin.LoadAsync();
            loaded = true;
        }
        catch (Exception exception)
        {
            _Logger.LogError(exception, $"Failed to load {plugin.Name}");
        }

        if (loaded)
        {
            _Logger.LogInformation($"Loaded {plugin.Name} by {plugin.Author}");
            _Plugins.Add(new(container, plugin.Name));
            return;
        }

        await container.DisposeAsync();
    }
}
