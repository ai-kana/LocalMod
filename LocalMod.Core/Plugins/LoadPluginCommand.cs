using Cysharp.Threading.Tasks;
using LocalMod.Core.Commands;
using Microsoft.Extensions.Logging;

namespace LocalMod.Core.Plugins;

internal class LoadPluginCommand : IAsyncCommand
{
    private Type _PluginType;
    private ILogger _Logger;

    public LoadPluginCommand(ILogger logger, Type plugin)
    {
        _PluginType = plugin;
        _Logger = logger;
    }

    public async UniTask ExecuteAsync()
    {
        IPlugin plugin = (IPlugin)Activator.CreateInstance(_PluginType);
        _Logger.LogInformation($"Loading plugin {plugin.Name} by {plugin.Author}");

        await plugin.LoadAsync();

        PluginManager.Plugins.Add(plugin);
    }
}
