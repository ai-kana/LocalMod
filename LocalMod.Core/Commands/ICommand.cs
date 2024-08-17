using Cysharp.Threading.Tasks;

namespace LocalMod.Core.Commands;

public interface ICommand
{
    public UniTask Execute();
}
