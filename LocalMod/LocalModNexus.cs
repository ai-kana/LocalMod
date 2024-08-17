using SDG.Framework.Modules;

namespace LocalMod;

public class LocalModNexus : IModuleNexus
{
    private LocalMod? _LocalMod;

    public async void initialize()
    {
        _LocalMod = new();
        await _LocalMod.LoadAsync();
    }

    public async void shutdown()
    {
        await _LocalMod!.UnloadAsync();
    }
}
