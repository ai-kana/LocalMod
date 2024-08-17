using Cysharp.Threading.Tasks;

namespace LocalMod.Commands;

public interface IAsyncCommand
{
    public UniTask ExecuteAsync();
}
