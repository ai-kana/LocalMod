using Cysharp.Threading.Tasks;
using LocalMod.API.Plugins;
using Microsoft.Extensions.Logging;

namespace TestPlugin;

public class Plugin : IPlugin
{
    private readonly ILogger _Logger;

    public string Author => "Kana";
    public string Name => "TestPlugin";
    
    public Plugin(ILogger logger)
    {
        _Logger = logger;
    }

    public UniTask LoadAsync()
    {
        return UniTask.CompletedTask;
    }

    public UniTask UnloadAsync()
    {
        return UniTask.CompletedTask;
    }
}
