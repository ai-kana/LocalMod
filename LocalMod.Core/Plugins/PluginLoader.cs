using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Extensions.DependencyInjection;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using LocalMod.API.IoC;
using LocalMod.API.NetAbstractions;
using LocalMod.API.Plugins;
using LocalMod.Core.IoC;
using LocalMod.Core.NetAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LocalMod.Core.Plugins;

[Service(ServiceLifetime.Singleton)]
internal class PluginLoader : IAsyncDisposable
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

    private const string PluginPath = "Plugins";

    private readonly HashSet<PluginData> _Plugins;
    private readonly ILogger _Logger;
    private readonly IServiceProvider _Provider;
    private readonly IConfiguration _Congfiguration;
    private readonly NetMethodManager _NetMethodManager;
    private readonly INetMethodResolver _Resolver;

    public PluginLoader(
            INetMethodResolver resolver,
            NetMethodManager manager, 
            IServiceProvider provider, 
            IConfiguration configuration, 
            ILogger<PluginLoader> logger)
    {
        _Plugins = new();
        _Resolver = resolver;
        _NetMethodManager = manager;
        _Logger = logger;
        _Provider = provider;
        _Congfiguration = configuration;
    }

    public async ValueTask DisposeAsync()
    {
        await UnloadPluginsAsync();
    }

    internal async UniTask LoadPluginsAsync()
    {
        Directory.CreateDirectory(PluginPath);

        foreach (string file in Directory.GetFiles(PluginPath, "*.dll"))
        {
            string fullpath = Path.GetFullPath(file);
            await LoadPluginAsync(Assembly.LoadFile(fullpath));
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

    internal async UniTask UnloadPluginAsync(string name)
    {
        PluginData? data = GetPluginDataFromName(name);
        if (data == null)
        {
            return;
        }

        await UnloadPluginAsync(data.Value);
    }

    private async UniTask UnloadPluginAsync(PluginData data, bool remove = true)
    {
        IPlugin plugin = data.Container.Resolve<IPlugin>();
        Harmony harmony = data.Container.Resolve<Harmony>();
        harmony.UnpatchAll(harmony.Id);

        try
        {
            await plugin.UnloadAsync();
            _Logger.LogInformation($"Unloaded {plugin.Name}");
        }
        catch (Exception exception)
        {
            _Logger.LogError(exception, $"Failed to unload plugin: {plugin.Name}");
        }

        if (remove)
        {
            _Plugins.Remove(data);
        }
    }

    internal async UniTask UnloadPluginsAsync()
    {
        foreach (PluginData data in _Plugins)
        {
            await UnloadPluginAsync(data, false);
        }

        _Plugins.Clear();
    }

    private async UniTask<IConfiguration?> LoadConfiguration(Assembly assembly, Type pluginType)
    {
        string name = assembly.GetName().Name;
        Directory.CreateDirectory(name);

        string path = Path.Combine(PluginPath, name, "Configuration.json");

        if (File.Exists(path))
        {
            return CreateConfiguration();
        }

        string manifestPath = pluginType.Namespace + ".Configuration.json";
        await using Stream? stream = assembly.GetManifestResourceStream(manifestPath);
        if (stream == null)
        {
            _Logger.LogWarning($"Failed to load configuration for plugin: {assembly.FullName}");
            return null;
        }

        using StreamReader reader = new(stream);
        string buf = await reader.ReadToEndAsync();

        await using StreamWriter writer = new(File.Open(path, FileMode.Create));
        await writer.WriteAsync(buf);
        await writer.FlushAsync();

        return CreateConfiguration();

        IConfiguration CreateConfiguration() => new ConfigurationBuilder().AddJsonFile(path).Build();
    }

    private async UniTask LoadPluginAsync(Assembly assembly)
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

        IConfiguration? configuration = await LoadConfiguration(assembly, pluginType);
        if (configuration != null)
        {
            builder.RegisterInstance(configuration).As<IConfiguration>().SingleInstance();
        }

        builder.RegisterInstance(_Resolver).As<INetMethodResolver>().SingleInstance();
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

        _NetMethodManager.RegisterFromAssembly(assembly);

        await StartPluginAsync(container);
    }

    private async UniTask StartPluginAsync(IContainer container)
    {
        IPlugin plugin = container.Resolve<IPlugin>();
        try
        {
            await plugin.LoadAsync();
            _Logger.LogInformation($"Loaded {plugin.Name} by {plugin.Author}");
            _Plugins.Add(new(container, plugin.Name));
        }
        catch (Exception exception)
        {
            _Logger.LogError(exception, $"Failed to load {plugin.Name}");
            await container.DisposeAsync();
        }
    }
}
