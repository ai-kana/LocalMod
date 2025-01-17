using Cysharp.Threading.Tasks;
using LocalMod.API.NetAbstractions;
using LocalMod.API.Plugins;
using Microsoft.Extensions.Logging;
using SDG.Unturned;

namespace TestPlugin;

public class Plugin : IPlugin
{
    public string Author => "Kana";
    public string Name => "TestPlugin";
    
    private readonly ILogger _Logger;
    private ServerNetMethod<float> _FlipRpc = null!;
    private readonly INetMethodResolver _Resolver;

    public Plugin(ILogger logger, INetMethodResolver resolver)
    {
        _Logger = logger;
        _Resolver = resolver;
    }

    public UniTask LoadAsync()
    {
        Provider.onServerHosted += OnHosted;

        return UniTask.CompletedTask;
    }

    private void OnHosted()
    {
        _FlipRpc = (ServerNetMethod<float>)_Resolver.ResolveServerMethod<ExampleRPC>()!;
        PlayerInput.onPluginKeyTick += OnKeyTicked;
        if (_FlipRpc == null)
        {
            throw new("Failed to resolve flip rpc");
        }
    }

    private void OnKeyTicked(Player player, uint simulation, byte key, bool state)
    {
        if (!state)
        {
            return;
        }

        switch (key)
        {
            case 1:
                _FlipRpc.Invoke(2f);
                return;
            case 2:
                _FlipRpc.Invoke(-2f);
                return;
        }
    }

    public UniTask UnloadAsync()
    {
        return UniTask.CompletedTask;
    }
}
