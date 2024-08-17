using Cysharp.Threading.Tasks;

namespace LocalMod.Plugins;

public interface IPlugin
{
    public string Name {get;}
    public string Author {get;}
    public string Description {get;}

    public UniTask LoadAsync();
    public UniTask UnloadAsync();
}
