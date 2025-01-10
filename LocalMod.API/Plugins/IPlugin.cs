using Cysharp.Threading.Tasks;

namespace LocalMod.API.Plugins;

public interface IPlugin
{
    public string Author {get;}
    public string Name {get;}

    public UniTask LoadAsync();
    public UniTask UnloadAsync();
}
