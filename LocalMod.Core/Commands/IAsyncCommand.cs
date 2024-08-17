using Cysharp.Threading.Tasks;

namespace LocalMod.Core.Commands;

public interface IAsyncCommand
{
    public UniTask ExecuteAsync();
}
