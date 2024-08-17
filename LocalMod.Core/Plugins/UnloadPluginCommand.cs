using Cysharp.Threading.Tasks;
using LocalMod.Core.Commands;

namespace LocalMod.Core.Plugins;

internal class UnloadPluginCommand : IAsyncCommand
{
    private IPlugin _Plugin;

    public UnloadPluginCommand(IPlugin plugin)
    {
        _Plugin = plugin;
    }

    public async UniTask ExecuteAsync()
    {
        await _Plugin.UnloadAsync();
    }
}
