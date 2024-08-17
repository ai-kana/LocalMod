using Cysharp.Threading.Tasks;
using LocalMod.Commands;

namespace LocalMod.Plugins;

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
