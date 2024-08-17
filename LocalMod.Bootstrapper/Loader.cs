using SDG.Framework.Modules;
using Cysharp.Threading.Tasks;
using System.Reflection;

namespace LocalMod.Bootstrapper;

internal class Loader : IModuleNexus
{
    private AppDomain? _LocalModDomain;
    private ILocalModEntry? _LocalMod;
    //private Harmony? _Harmony;

    public async void initialize()
    {
        //_Harmony = new("LocalMod");
        //_Harmony.PatchAll();
        await LoadAsync();
    }

    public async void shutdown()
    {
        await UnloadAsync();
    }

    // Hot reloading doesnt really work,, Maybe im not understanding something will look later
    private async UniTask LoadAsync()
    {
        _LocalModDomain = AppDomain.CreateDomain("LocalMod");

        Assembly coreAssembly = Assembly.LoadFile(Path.GetFullPath("./LocalMod/LocalMod.Core.dll"));
        Type localModType = coreAssembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(ILocalModEntry))).First();

        _LocalMod = (ILocalModEntry)Activator.CreateInstance(localModType);

        await _LocalMod.LoadAsync();
        OnAssetsRefreshingPatch.OnAssetsRefreshing += Reload;
    }

    private async UniTask UnloadAsync()
    {
        if (_LocalMod != null)
        {
            await _LocalMod.UnloadAsync();
        }

        AppDomain.Unload(_LocalModDomain);
        OnAssetsRefreshingPatch.OnAssetsRefreshing -= Reload;
    }

    public async void Reload()
    {
        await UnloadAsync();
        await LoadAsync();
    }
}
