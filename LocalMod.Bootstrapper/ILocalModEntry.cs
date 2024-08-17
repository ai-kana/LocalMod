using Cysharp.Threading.Tasks;

namespace LocalMod.Bootstrapper;

public interface ILocalModEntry
{
    public UniTask LoadAsync();
    public UniTask UnloadAsync();
}
