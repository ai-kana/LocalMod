using System.Collections.Concurrent;
using System.Reflection;
using Cysharp.Threading.Tasks;
using LocalMod.Commands;
using LocalMod.Logging;
using Microsoft.Extensions.Logging;
using SDG.Unturned;

namespace LocalMod.Plugins;

public class PluginManager
{
    internal static ConcurrentBag<IPlugin> Plugins = new();

    private static ILogger? _Logger;
    private const string PluginDirectory = LocalMod.LocalModDirectory + "/Plugins";

    public static T? GetPlugin<T>() where T : IPlugin
    { 
        foreach (IPlugin plugin in Plugins)
        {
            if (plugin is T)
            {
                return (T)plugin;
            }
        }

        return default(T);
    }

    private static bool IsValidConstructorPredicate(ConstructorInfo constructor)
    {
        return constructor.GetParameters().Count() == 0;
    }

    private static bool IsPluginPredicate(Type type)
    {
        return type.GetInterfaces().Contains(typeof(IPlugin)) && type.GetConstructors().Count(IsValidConstructorPredicate) == 1;
    }

    private static void LoadPlugin(string path, Queue<IAsyncCommand> queue)
    {
        Assembly assembly = Assembly.LoadFile(path);
        NetReflection.RegisterFromAssembly(assembly);
        IEnumerable<Type> pluginTypes = assembly.GetTypes().Where(IsPluginPredicate);
        foreach (Type type in pluginTypes)
        {
            queue.Enqueue(new LoadPluginCommand(_Logger!, type));
        }
    }

    internal static async UniTask LoadAsync()
    {
        _Logger = Logger.CreateLogger<PluginManager>();
        _Logger.LogInformation("Loading plugins...");
        Directory.CreateDirectory(PluginDirectory);

        IEnumerable<string> paths = Directory.GetFiles(PluginDirectory, "*.dll");
        Queue<IAsyncCommand> commandQueue = new();

        foreach (string path in paths)
        {
            LoadPlugin(Path.GetFullPath(path), commandQueue);
        }

        AsyncCommandWorker worker = new(commandQueue);
        int count = await worker.ExecuteAsync();

        _Logger.LogInformation($"Loaded {count} plugins!");
    }

    internal static async UniTask UnloadAsync()
    {
        Queue<IAsyncCommand> commandQueue = new();
        foreach (IPlugin plugin in Plugins)
        {
            commandQueue.Enqueue(new UnloadPluginCommand(plugin));
        }

        AsyncCommandWorker worker = new(commandQueue);
        await worker.ExecuteAsync();

        Plugins.Clear();
    }
}
