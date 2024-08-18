using Cysharp.Threading.Tasks;
using LocalMod.Core.Logging;
using LocalMod.Core.Plugins;
using Microsoft.Extensions.Logging;

namespace TestPlugin;

public class TestPlugin : IPlugin
{
    public string Name => "TestPlugin";
    public string Author => "Me";
    public string Description => "Plugin for testing";

    private ILogger? _Logger;

    public UniTask LoadAsync()
    {
        _Logger = Logger.CreateLogger<TestPlugin>();
        return UniTask.CompletedTask;
    }

    public UniTask UnloadAsync()
    {
        return UniTask.CompletedTask;
    }
}
